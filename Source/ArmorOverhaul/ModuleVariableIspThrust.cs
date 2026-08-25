using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ArmorOverhaul
{
    /// <summary>
    /// Continuously adjusts the atmosphere curve and thrust of one stock
    /// ModuleEnginesFX. Derived engine types are deliberately unsupported.
    /// </summary>
    public class ModuleVariableIspThrust : PartModule
    {
        private const float ComparisonTolerance = 0.0001f;
        private const double ResourceTolerance = 1e-9;

        private enum ThrustInterpolationMode
        {
            Linear,
            ConstantPower
        }

        private sealed class PerformancePoint
        {
            public float Percent;
            public float MaxThrust;
            public float MinThrust;
            public bool HasMinThrust;
            public float VacuumIsp;
            public FloatCurve AtmosphereCurve;
        }

        private sealed class ThrottleResource
        {
            public string ResourceName;
            public int ResourceId = -1;
            public double Ratio;
            public ResourceFlowMode FlowMode = ResourceFlowMode.NULL;
            public bool DumpExcess = true;
        }

        [KSPField]
        public string engineID = string.Empty;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true,
            guiName = "Engine Performance", guiFormat = "F0", guiUnits = "%")]
        [UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f,
            affectSymCounterparts = UI_Scene.Editor)]
        public float performanceSetting = 50f;

        [KSPField]
        public float sliderStep = 1f;

        [KSPField]
        public bool allowInFlight = true;

        [KSPField]
        public bool allowWhileRunning = true;

        [KSPField]
        public bool scaleMinThrust = true;

        [KSPField]
        public string thrustInterpolation = "linear";

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Vacuum ISP")]
        public string vacuumIspDisplay = "Unavailable";

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Vacuum Max Thrust")]
        public string maxThrustDisplay = "Unavailable";

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Current ISP")]
        public string currentIspDisplay = "Unavailable";

        private readonly List<ConfigNode> performancePointNodes = new List<ConfigNode>();
        private readonly List<ConfigNode> inputResourceNodes = new List<ConfigNode>();
        private readonly List<ConfigNode> outputResourceNodes = new List<ConfigNode>();
        private readonly List<PerformancePoint> performancePoints = new List<PerformancePoint>();
        private readonly List<ThrottleResource> inputResources = new List<ThrottleResource>();
        private readonly List<ThrottleResource> outputResources = new List<ThrottleResource>();
        private ModuleEnginesFX engine;
        private FloatCurve originalAtmosphereCurve;
        private float originalMinThrust;
        private float originalMaxThrust;
        private float lastAppliedSetting = float.NaN;
        private bool initialized;
        private bool initializationFailed;
        private bool lastControlsEnabled;
        private float nextEnvironmentDisplayRefresh;
        private ThrustInterpolationMode interpolationMode;

        [KSPAction("Set Engine Performance: 0%")]
        public void SetPerformance0Action(KSPActionParam _)
        {
            SetPreset(0f);
        }

        [KSPAction("Set Engine Performance: 20%")]
        public void SetPerformance20Action(KSPActionParam _)
        {
            SetPreset(20f);
        }

        [KSPAction("Set Engine Performance: 40%")]
        public void SetPerformance40Action(KSPActionParam _)
        {
            SetPreset(40f);
        }

        [KSPAction("Set Engine Performance: 60%")]
        public void SetPerformance60Action(KSPActionParam _)
        {
            SetPreset(60f);
        }

        [KSPAction("Set Engine Performance: 80%")]
        public void SetPerformance80Action(KSPActionParam _)
        {
            SetPreset(80f);
        }

        [KSPAction("Set Engine Performance: 100%")]
        public void SetPerformance100Action(KSPActionParam _)
        {
            SetPreset(100f);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            ConfigNode[] nodes = node.GetNodes("PERFORMANCE_POINT");
            if (nodes.Length > 0)
            {
                performancePointNodes.Clear();
                foreach (ConfigNode performancePointNode in nodes)
                    performancePointNodes.Add(performancePointNode.CreateCopy());
            }

            LoadResourceNodeCopies(node, "INPUT_RESOURCE", inputResourceNodes);
            LoadResourceNodeCopies(node, "OUTPUT_RESOURCE", outputResourceNodes);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            ConfigureUserInterface(false);
        }

        // Unity Start runs after all PartModule.OnStart calls on the part.
        public void Start()
        {
            Initialize();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!initialized) return;

            bool controlsEnabled = CanAdjustNow();
            if (controlsEnabled != lastControlsEnabled)
                ConfigureControlState(controlsEnabled);

            if (Math.Abs(performanceSetting - lastAppliedSetting) > ComparisonTolerance)
            {
                if (controlsEnabled)
                    ApplyCurrentSetting();
                else
                    performanceSetting = lastAppliedSetting;
            }

            if (HighLogic.LoadedSceneIsFlight && Time.realtimeSinceStartup >= nextEnvironmentDisplayRefresh)
            {
                nextEnvironmentDisplayRefresh = Time.realtimeSinceStartup + 0.25f;
                RefreshEnvironmentDisplay();
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (!initialized || engine == null || part == null || !HighLogic.LoadedSceneIsFlight)
                return;
            if (!engine.EngineIgnited || !engine.isOperational || engine.flameout)
                return;

            double throttle = Clamp01(engine.currentThrottle);
            double deltaTime = TimeWarp.fixedDeltaTime;
            if (throttle <= ResourceTolerance || deltaTime <= ResourceTolerance)
                return;

            ProcessThrottleResources(throttle, deltaTime);
        }

        private void Initialize()
        {
            if (initialized || initializationFailed) return;

            if (!FindEngine())
            {
                FailInitialization("could not find exactly one supported ModuleEnginesFX");
                return;
            }

            originalAtmosphereCurve = CloneCurve(engine.atmosphereCurve);
            originalMinThrust = engine.minThrust;
            originalMaxThrust = engine.maxThrust;

            if (originalAtmosphereCurve == null || originalAtmosphereCurve.Curve.length == 0)
            {
                FailInitialization("the targeted engine has no atmosphereCurve");
                return;
            }

            if (!IsFinitePositive(originalAtmosphereCurve.Evaluate(0f)))
            {
                FailInitialization("the targeted engine has an invalid vacuum ISP");
                return;
            }

            if (!TryResolveInterpolationMode()) return;

            if (performancePointNodes.Count == 0)
                FindPerformancePointNodesInPartConfig();

            if (inputResourceNodes.Count == 0 && outputResourceNodes.Count == 0)
                FindResourceNodesInPartConfig();

            string validationError;
            if (!BuildPerformancePoints(out validationError))
            {
                FailInitialization(validationError);
                return;
            }

            if (!BuildThrottleResources(out validationError))
            {
                FailInitialization(validationError);
                return;
            }

            ConfigureSlider();
            initialized = true;
            performanceSetting = Mathf.Clamp(performanceSetting, 0f, 100f);
            ConfigureUserInterface(true);
            ApplyCurrentSetting();
        }

        private bool FindEngine()
        {
            List<ModuleEnginesFX> matches = new List<ModuleEnginesFX>();

            foreach (PartModule module in part.Modules)
            {
                ModuleEnginesFX candidate = module as ModuleEnginesFX;
                if (candidate == null) continue;

                // Do not broaden this check: ModuleEnginesRF and other derived
                // engines are intentionally outside this module's contract.
                if (candidate.GetType() != typeof(ModuleEnginesFX)) continue;
                if (!string.IsNullOrEmpty(engineID) && candidate.engineID != engineID) continue;
                matches.Add(candidate);
            }

            if (matches.Count != 1) return false;

            engine = matches[0];
            if (string.IsNullOrEmpty(engineID)) engineID = engine.engineID;
            return true;
        }

        private bool TryResolveInterpolationMode()
        {
            if (string.Equals(thrustInterpolation, "linear", StringComparison.OrdinalIgnoreCase))
            {
                interpolationMode = ThrustInterpolationMode.Linear;
                return true;
            }

            if (string.Equals(thrustInterpolation, "constantPower", StringComparison.OrdinalIgnoreCase))
            {
                interpolationMode = ThrustInterpolationMode.ConstantPower;
                return true;
            }

            FailInitialization("thrustInterpolation must be linear or constantPower");
            return false;
        }

        private void FindPerformancePointNodesInPartConfig()
        {
            if (part.partInfo == null || part.partInfo.partConfig == null) return;

            ConfigNode[] moduleNodes = part.partInfo.partConfig.GetNodes("MODULE");
            List<ConfigNode> candidates = new List<ConfigNode>();
            foreach (ConfigNode moduleNode in moduleNodes)
            {
                if (moduleNode.GetValue("name") != nameof(ModuleVariableIspThrust)) continue;
                string configuredEngineId = moduleNode.GetValue("engineID");
                if (!string.IsNullOrEmpty(engineID) &&
                    !string.IsNullOrEmpty(configuredEngineId) &&
                    configuredEngineId != engineID) continue;
                candidates.Add(moduleNode);
            }

            if (candidates.Count != 1) return;

            foreach (ConfigNode pointNode in candidates[0].GetNodes("PERFORMANCE_POINT"))
                performancePointNodes.Add(pointNode.CreateCopy());
        }

        private static void LoadResourceNodeCopies(
            ConfigNode source,
            string nodeName,
            List<ConfigNode> destination)
        {
            ConfigNode[] nodes = source.GetNodes(nodeName);
            if (nodes.Length == 0) return;

            destination.Clear();
            foreach (ConfigNode resourceNode in nodes)
                destination.Add(resourceNode.CreateCopy());
        }

        private void FindResourceNodesInPartConfig()
        {
            if (part.partInfo == null || part.partInfo.partConfig == null) return;

            List<ConfigNode> candidates = new List<ConfigNode>();
            foreach (ConfigNode moduleNode in part.partInfo.partConfig.GetNodes("MODULE"))
            {
                if (moduleNode.GetValue("name") != nameof(ModuleVariableIspThrust)) continue;
                string configuredEngineId = moduleNode.GetValue("engineID");
                if (!string.IsNullOrEmpty(engineID) &&
                    !string.IsNullOrEmpty(configuredEngineId) &&
                    configuredEngineId != engineID) continue;
                candidates.Add(moduleNode);
            }

            if (candidates.Count != 1) return;

            LoadResourceNodeCopies(candidates[0], "INPUT_RESOURCE", inputResourceNodes);
            LoadResourceNodeCopies(candidates[0], "OUTPUT_RESOURCE", outputResourceNodes);
        }

        private bool BuildThrottleResources(out string error)
        {
            inputResources.Clear();
            outputResources.Clear();
            error = string.Empty;

            HashSet<string> resourceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ConfigNode node in inputResourceNodes)
            {
                ThrottleResource resource;
                if (!TryParseThrottleResource(node, false, out resource, out error)) return false;
                if (!resourceNames.Add(resource.ResourceName))
                {
                    error = "throttle resource " + resource.ResourceName + " is configured more than once";
                    return false;
                }
                if (!ResolveThrottleResource(resource, out error)) return false;
                inputResources.Add(resource);
            }

            foreach (ConfigNode node in outputResourceNodes)
            {
                ThrottleResource resource;
                if (!TryParseThrottleResource(node, true, out resource, out error)) return false;
                if (!resourceNames.Add(resource.ResourceName))
                {
                    error = "throttle resource " + resource.ResourceName +
                        " cannot be configured more than once or as both input and output";
                    return false;
                }
                if (!ResolveThrottleResource(resource, out error)) return false;
                outputResources.Add(resource);
            }

            return true;
        }

        private static bool TryParseThrottleResource(
            ConfigNode node,
            bool isOutput,
            out ThrottleResource resource,
            out string error)
        {
            resource = new ThrottleResource();
            error = string.Empty;

            string resourceName = GetNodeValue(node, "name", "ResourceName");
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                error = (isOutput ? "OUTPUT_RESOURCE" : "INPUT_RESOURCE") +
                    " requires a resource name";
                return false;
            }

            string rawRatio = GetNodeValue(node, "ratio", "Ratio");
            double ratio;
            if (string.IsNullOrWhiteSpace(rawRatio) ||
                !double.TryParse(
                    rawRatio,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out ratio) ||
                !IsFinite(ratio) || ratio < 0d)
            {
                error = (isOutput ? "OUTPUT_RESOURCE" : "INPUT_RESOURCE") + " " +
                    resourceName + " requires a non-negative ratio in units per second";
                return false;
            }

            ResourceFlowMode flowMode = ResourceFlowMode.NULL;
            string rawFlowMode = GetNodeValue(node, "flowMode", "FlowMode");
            if (!string.IsNullOrWhiteSpace(rawFlowMode) &&
                !Enum.TryParse(rawFlowMode.Trim(), true, out flowMode))
            {
                error = (isOutput ? "OUTPUT_RESOURCE" : "INPUT_RESOURCE") + " " +
                    resourceName + " has invalid flowMode " + rawFlowMode;
                return false;
            }

            bool dumpExcess = true;
            string rawDumpExcess = GetNodeValue(node, "dumpExcess", "DumpExcess");
            if (isOutput && !string.IsNullOrWhiteSpace(rawDumpExcess) &&
                !bool.TryParse(rawDumpExcess.Trim(), out dumpExcess))
            {
                error = "OUTPUT_RESOURCE " + resourceName +
                    " has invalid dumpExcess value " + rawDumpExcess;
                return false;
            }

            resource.ResourceName = resourceName.Trim();
            resource.Ratio = ratio;
            resource.FlowMode = flowMode;
            resource.DumpExcess = dumpExcess;
            return true;
        }

        private static string GetNodeValue(ConfigNode node, string preferredName, string alternateName)
        {
            string value = node.GetValue(preferredName);
            return value ?? node.GetValue(alternateName);
        }

        private static bool ResolveThrottleResource(ThrottleResource resource, out string error)
        {
            error = string.Empty;
            PartResourceDefinition definition =
                PartResourceLibrary.Instance.GetDefinition(resource.ResourceName);
            if (definition == null)
            {
                error = "could not resolve throttle resource " + resource.ResourceName;
                return false;
            }

            resource.ResourceId = definition.id;
            if (resource.FlowMode == ResourceFlowMode.NULL)
                resource.FlowMode = definition.resourceFlowMode;
            return true;
        }

        private bool BuildPerformancePoints(out string error)
        {
            performancePoints.Clear();
            error = string.Empty;

            if (performancePointNodes.Count < 2)
            {
                error = "at least two PERFORMANCE_POINT nodes are required";
                return false;
            }

            foreach (ConfigNode node in performancePointNodes)
            {
                PerformancePoint point;
                if (!TryParsePerformancePoint(node, out point, out error)) return false;
                performancePoints.Add(point);
            }

            performancePoints.Sort((left, right) => left.Percent.CompareTo(right.Percent));

            if (Math.Abs(performancePoints[0].Percent) > ComparisonTolerance ||
                Math.Abs(performancePoints[performancePoints.Count - 1].Percent - 100f) > ComparisonTolerance)
            {
                error = "PERFORMANCE_POINT nodes must include percent 0 and 100";
                return false;
            }

            for (int index = 1; index < performancePoints.Count; index++)
            {
                if (performancePoints[index].Percent - performancePoints[index - 1].Percent <= ComparisonTolerance)
                {
                    error = "PERFORMANCE_POINT percent values must be unique";
                    return false;
                }
            }

            float originalVacuumIsp = originalAtmosphereCurve.Evaluate(0f);
            float originalMinRatio = originalMaxThrust > 0f ? originalMinThrust / originalMaxThrust : 0f;

            foreach (PerformancePoint point in performancePoints)
            {
                if (point.AtmosphereCurve == null)
                {
                    point.AtmosphereCurve = ScaleCurve(originalAtmosphereCurve, point.VacuumIsp / originalVacuumIsp);
                }

                point.VacuumIsp = point.AtmosphereCurve.Evaluate(0f);
                if (!IsFinitePositive(point.VacuumIsp))
                {
                    error = "every PERFORMANCE_POINT must have a positive vacuum ISP";
                    return false;
                }

                if (!point.HasMinThrust)
                {
                    point.MinThrust = scaleMinThrust
                        ? point.MaxThrust * originalMinRatio
                        : Math.Min(originalMinThrust, point.MaxThrust);
                }

                if (point.MinThrust < 0f || point.MinThrust > point.MaxThrust)
                {
                    error = "PERFORMANCE_POINT minThrust must be between zero and maxThrust";
                    return false;
                }
            }

            for (int index = 1; index < performancePoints.Count; index++)
            {
                if (!HaveMatchingKeys(performancePoints[0].AtmosphereCurve, performancePoints[index].AtmosphereCurve))
                {
                    error = "all PERFORMANCE_POINT atmosphere curves must use identical pressure keys";
                    return false;
                }
            }

            return true;
        }

        private static bool TryParsePerformancePoint(
            ConfigNode node,
            out PerformancePoint point,
            out string error)
        {
            point = new PerformancePoint();
            error = string.Empty;

            if (!node.TryGetValue("percent", ref point.Percent) ||
                !IsFinite(point.Percent) || point.Percent < 0f || point.Percent > 100f)
            {
                error = "PERFORMANCE_POINT percent must be between 0 and 100";
                return false;
            }

            if (!node.TryGetValue("maxThrust", ref point.MaxThrust) || !IsFinitePositive(point.MaxThrust))
            {
                error = "PERFORMANCE_POINT maxThrust must be positive";
                return false;
            }

            point.HasMinThrust = node.TryGetValue("minThrust", ref point.MinThrust);
            if (point.HasMinThrust && !IsFinite(point.MinThrust))
            {
                error = "PERFORMANCE_POINT minThrust is invalid";
                return false;
            }

            ConfigNode atmosphereCurveNode = node.GetNode("atmosphereCurve");
            if (atmosphereCurveNode != null)
            {
                point.AtmosphereCurve = new FloatCurve();
                point.AtmosphereCurve.Load(atmosphereCurveNode);
                if (point.AtmosphereCurve.Curve.length == 0)
                {
                    error = "PERFORMANCE_POINT atmosphereCurve has no keys";
                    return false;
                }
            }
            else if (!node.TryGetValue("vacuumIsp", ref point.VacuumIsp) || !IsFinitePositive(point.VacuumIsp))
            {
                error = "PERFORMANCE_POINT requires atmosphereCurve or positive vacuumIsp";
                return false;
            }

            return true;
        }

        private void ConfigureSlider()
        {
            if (!IsFinitePositive(sliderStep) || sliderStep > 100f) sliderStep = 1f;

            BaseField field = Fields[nameof(performanceSetting)];
            ConfigureSliderControl(field.uiControlEditor as UI_FloatRange);
            ConfigureSliderControl(field.uiControlFlight as UI_FloatRange);
        }

        private void ConfigureSliderControl(UI_FloatRange control)
        {
            if (control == null) return;

            control.minValue = 0f;
            control.maxValue = 100f;
            control.stepIncrement = sliderStep;
            control.onFieldChanged = OnPerformanceSettingChanged;
            control.onSymmetryFieldChanged = OnPerformanceSettingChanged;
        }

        private void OnPerformanceSettingChanged(BaseField _, object oldValue)
        {
            if (!initialized) return;

            if (!CanAdjustNow())
            {
                performanceSetting = oldValue is float ? (float)oldValue : lastAppliedSetting;
                return;
            }

            performanceSetting = Mathf.Clamp(performanceSetting, 0f, 100f);
            ApplyCurrentSetting();
        }

        private void SetPreset(float percent)
        {
            if (!initialized || !CanAdjustNow()) return;

            performanceSetting = percent;
            ApplyCurrentSetting();
        }

        private void ApplyCurrentSetting()
        {
            PerformancePoint lower;
            PerformancePoint upper;
            float blend;
            FindPerformanceInterval(performanceSetting, out lower, out upper, out blend);

            FloatCurve curve = BlendCurves(lower.AtmosphereCurve, upper.AtmosphereCurve, blend);
            float vacuumIsp = curve.Evaluate(0f);
            float maxThrust = interpolationMode == ThrustInterpolationMode.ConstantPower
                ? InterpolateConstantPowerThrust(
                    lower.MaxThrust,
                    lower.VacuumIsp,
                    upper.MaxThrust,
                    upper.VacuumIsp,
                    vacuumIsp,
                    blend)
                : Mathf.Lerp(lower.MaxThrust, upper.MaxThrust, blend);
            float minThrust = Mathf.Lerp(lower.MinThrust, upper.MinThrust, blend);
            float appliedMinThrust = Math.Min(minThrust, maxThrust);

            engine.atmosphereCurve = curve;
            engine.maxThrust = maxThrust;
            engine.minThrust = appliedMinThrust;
            engine.maxFuelFlow = CalculateFuelFlow(maxThrust, vacuumIsp, engine.g);
            engine.minFuelFlow = CalculateFuelFlow(appliedMinThrust, vacuumIsp, engine.g);
            engine.SetupPropellant();

            lastAppliedSetting = performanceSetting;
            vacuumIspDisplay = vacuumIsp.ToString("0.##", CultureInfo.InvariantCulture) + " s";
            maxThrustDisplay = maxThrust.ToString("0.###", CultureInfo.InvariantCulture) + " kN";
            RefreshEnvironmentDisplay();
        }

        private void ProcessThrottleResources(double throttle, double deltaTime)
        {
            double recipeFraction = CalculateInputRecipeFraction(throttle, deltaTime);
            if (recipeFraction <= ResourceTolerance)
            {
                engine.Shutdown();
                return;
            }

            double processedFraction = recipeFraction;
            foreach (ThrottleResource input in inputResources)
            {
                double fullAmount = CalculateThrottleResourceAmount(
                    input.Ratio,
                    throttle,
                    deltaTime);
                if (fullAmount <= ResourceTolerance) continue;

                double requestedAmount = fullAmount * recipeFraction;
                double consumedAmount = part.RequestResource(
                    input.ResourceId,
                    requestedAmount,
                    input.FlowMode);
                processedFraction = Math.Min(
                    processedFraction,
                    Math.Max(0d, consumedAmount) / fullAmount);
            }

            bool mustShutdown = processedFraction + ResourceTolerance < 1d;
            foreach (ThrottleResource output in outputResources)
            {
                double outputAmount = CalculateThrottleResourceAmount(
                    output.Ratio,
                    throttle,
                    deltaTime) * processedFraction;
                if (outputAmount <= ResourceTolerance) continue;

                double transferredAmount = part.RequestResource(
                    output.ResourceId,
                    -outputAmount,
                    output.FlowMode);
                double storedAmount = Math.Max(0d, -transferredAmount);
                if (!output.DumpExcess && storedAmount + ResourceTolerance < outputAmount)
                    mustShutdown = true;
            }

            if (mustShutdown)
                engine.Shutdown();
        }

        private double CalculateInputRecipeFraction(double throttle, double deltaTime)
        {
            double recipeFraction = 1d;
            foreach (ThrottleResource input in inputResources)
            {
                double requestedAmount = CalculateThrottleResourceAmount(
                    input.Ratio,
                    throttle,
                    deltaTime);
                if (requestedAmount <= ResourceTolerance) continue;

                double availableAmount;
                double maxAmount;
                part.GetConnectedResourceTotals(
                    input.ResourceId,
                    input.FlowMode,
                    out availableAmount,
                    out maxAmount,
                    true);
                recipeFraction = Math.Min(
                    recipeFraction,
                    Math.Max(0d, availableAmount) / requestedAmount);
            }

            return Math.Max(0d, Math.Min(1d, recipeFraction));
        }

        private static double CalculateThrottleResourceAmount(
            double ratio,
            double throttle,
            double deltaTime)
        {
            if (!IsFinite(ratio) || ratio <= 0d ||
                !IsFinite(throttle) || throttle <= 0d ||
                !IsFinite(deltaTime) || deltaTime <= 0d)
            {
                return 0d;
            }

            return ratio * Clamp01(throttle) * deltaTime;
        }

        private static double Clamp01(double value)
        {
            if (!IsFinite(value)) return 0d;
            return Math.Max(0d, Math.Min(1d, value));
        }

        private void RefreshEnvironmentDisplay()
        {
            if (engine == null || engine.atmosphereCurve == null || part == null) return;

            float currentIsp = engine.realIsp;
            if (!IsFinitePositive(currentIsp))
                currentIsp = engine.atmosphereCurve.Evaluate((float)part.staticPressureAtm);

            currentIspDisplay = IsFinitePositive(currentIsp)
                ? currentIsp.ToString("0.##", CultureInfo.InvariantCulture) + " s"
                : "Unavailable";
        }

        private void FindPerformanceInterval(
            float percent,
            out PerformancePoint lower,
            out PerformancePoint upper,
            out float blend)
        {
            float clamped = Mathf.Clamp(percent, 0f, 100f);
            lower = performancePoints[0];
            upper = performancePoints[performancePoints.Count - 1];

            for (int index = 1; index < performancePoints.Count; index++)
            {
                if (clamped > performancePoints[index].Percent) continue;

                lower = performancePoints[index - 1];
                upper = performancePoints[index];
                break;
            }

            float width = upper.Percent - lower.Percent;
            blend = width > ComparisonTolerance ? (clamped - lower.Percent) / width : 0f;
            blend = Mathf.Clamp01(blend);
        }

        private bool CanAdjustNow()
        {
            if (!initialized) return false;
            if (HighLogic.LoadedSceneIsFlight && !allowInFlight) return false;
            if (HighLogic.LoadedSceneIsFlight && !allowWhileRunning && engine.EngineIgnited) return false;
            return true;
        }

        private void ConfigureUserInterface(bool available)
        {
            Fields[nameof(performanceSetting)].guiActive = available && allowInFlight;
            Fields[nameof(performanceSetting)].guiActiveEditor = available;
            Fields[nameof(vacuumIspDisplay)].guiActive = available && allowInFlight;
            Fields[nameof(vacuumIspDisplay)].guiActiveEditor = available;
            Fields[nameof(maxThrustDisplay)].guiActive = available && allowInFlight;
            Fields[nameof(maxThrustDisplay)].guiActiveEditor = available;
            Fields[nameof(currentIspDisplay)].guiActive = available && allowInFlight;
            Fields[nameof(currentIspDisplay)].guiActiveEditor = false;

            bool controlsEnabled = available && (!HighLogic.LoadedSceneIsFlight || CanAdjustNow());
            ConfigureControlState(controlsEnabled);
        }

        private void ConfigureControlState(bool enabled)
        {
            lastControlsEnabled = enabled;
            Fields[nameof(performanceSetting)].guiInteractable = enabled;

            bool actionsEnabled = enabled && allowInFlight;
            Actions[nameof(SetPerformance0Action)].active = actionsEnabled;
            Actions[nameof(SetPerformance20Action)].active = actionsEnabled;
            Actions[nameof(SetPerformance40Action)].active = actionsEnabled;
            Actions[nameof(SetPerformance60Action)].active = actionsEnabled;
            Actions[nameof(SetPerformance80Action)].active = actionsEnabled;
            Actions[nameof(SetPerformance100Action)].active = actionsEnabled;
        }

        private void FailInitialization(string reason)
        {
            if (initializationFailed) return;

            initializationFailed = true;
            ConfigureUserInterface(false);
            string partName = part != null ? part.name : "unknown part";
            Debug.LogError("[ArmorOverhaul] " + partName + " variable ISP/thrust module " + reason + ".");
        }

        private static FloatCurve CloneCurve(FloatCurve source)
        {
            if (source == null || source.Curve == null) return null;

            FloatCurve result = new FloatCurve();
            foreach (Keyframe key in source.Curve.keys)
                result.Add(key.time, key.value, key.inTangent, key.outTangent);
            return result;
        }

        private static FloatCurve ScaleCurve(FloatCurve source, float scale)
        {
            FloatCurve result = new FloatCurve();
            foreach (Keyframe key in source.Curve.keys)
            {
                result.Add(
                    key.time,
                    key.value * scale,
                    key.inTangent * scale,
                    key.outTangent * scale);
            }

            return result;
        }

        private static FloatCurve BlendCurves(FloatCurve lower, FloatCurve upper, float blend)
        {
            Keyframe[] lowerKeys = lower.Curve.keys;
            Keyframe[] upperKeys = upper.Curve.keys;
            FloatCurve result = new FloatCurve();

            for (int index = 0; index < lowerKeys.Length; index++)
            {
                result.Add(
                    lowerKeys[index].time,
                    Mathf.Lerp(lowerKeys[index].value, upperKeys[index].value, blend),
                    BlendTangent(lowerKeys[index].inTangent, upperKeys[index].inTangent, blend),
                    BlendTangent(lowerKeys[index].outTangent, upperKeys[index].outTangent, blend));
            }

            return result;
        }

        private static float BlendTangent(float lower, float upper, float blend)
        {
            if (lower.Equals(upper)) return lower;
            if (!IsFinite(lower) || !IsFinite(upper)) return blend < 0.5f ? lower : upper;
            return Mathf.Lerp(lower, upper, blend);
        }

        private static bool HaveMatchingKeys(FloatCurve left, FloatCurve right)
        {
            Keyframe[] leftKeys = left.Curve.keys;
            Keyframe[] rightKeys = right.Curve.keys;
            if (leftKeys.Length != rightKeys.Length) return false;

            for (int index = 0; index < leftKeys.Length; index++)
            {
                if (Math.Abs(leftKeys[index].time - rightKeys[index].time) > ComparisonTolerance)
                    return false;
            }

            return true;
        }

        private static float InterpolateConstantPowerThrust(
            float lowerThrust,
            float lowerVacuumIsp,
            float upperThrust,
            float upperVacuumIsp,
            float currentVacuumIsp,
            float blend)
        {
            float lowerPowerFactor = lowerThrust * lowerVacuumIsp;
            float upperPowerFactor = upperThrust * upperVacuumIsp;
            float currentPowerFactor = Mathf.Lerp(lowerPowerFactor, upperPowerFactor, blend);
            return currentPowerFactor / currentVacuumIsp;
        }

        private static float CalculateFuelFlow(
            float thrust,
            float vacuumIsp,
            float gravity)
        {
            if (!IsFinite(thrust) || thrust <= 0f ||
                !IsFinitePositive(vacuumIsp) || !IsFinitePositive(gravity))
            {
                return 0f;
            }

            return thrust / (vacuumIsp * gravity);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static bool IsFinitePositive(float value)
        {
            return IsFinite(value) && value > 0f;
        }
    }
}
