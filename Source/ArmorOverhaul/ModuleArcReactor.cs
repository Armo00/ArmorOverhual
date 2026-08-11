using System;
using System.Collections.Generic;
using KSP.Localization;

namespace ArmorOverhaul
{
    /// <summary>
    /// A stock resource converter with a persistent output-power ceiling and
    /// output-by-output load-following behavior near the configured fill target.
    /// </summary>
    public class ModuleArcReactor : ModuleResourceConverter
    {
        private const double ResourceTolerance = 1e-9;
        private const double KilowattsPerElectricChargePerSecond = 3.6d;
        private const string ElectricChargeResourceName = "ElectricCharge";

        private sealed class RegulatedOutput
        {
            public string ResourceName;
            public int ResourceId = -1;
            public ResourceFlowMode FlowMode = ResourceFlowMode.NULL;
        }

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true,
            guiName = "Output Power", guiFormat = "F0", guiUnits = "%")]
        [UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f,
            affectSymCounterparts = UI_Scene.Editor)]
        public float powerPercentage = 100f;

        [KSPField]
        public float sliderStep = 1f;

        [KSPField(guiActive = true, guiActiveEditor = false,
            guiName = "Power Output")]
        public string currentPowerOutput = "0 W";

        private readonly List<RegulatedOutput> regulatedOutputs = new List<RegulatedOutput>();
        private bool regulationConfigurationLoaded;
        private float fillTarget = 1f;
        private double recipeDeltaTime;
        private ConversionRecipe activeRecipe;
        private bool regulationLimitedOutputToZero;

        public override void OnLoad(ConfigNode node)
        {
            ConfigNode translatedNode = TranslateOutputResourceSettings(node);
            base.OnLoad(translatedNode);

            ConfigNode[] outputNodes = node.GetNodes("OUTPUT_RESOURCE");
            if (outputNodes.Length > 0)
                LoadRegulatedOutputs(outputNodes);
        }

        public override void OnStart(StartState state)
        {
            powerPercentage = ClampPercentage(powerPercentage);
            base.OnStart(state);
            ConfigureSlider();

            if (!regulationConfigurationLoaded)
                FindRegulatedOutputsInPartConfig();

            fillTarget = NormalizeFillTarget();
            ResolveRegulatedOutputResources();

            currentPowerOutput = "0 W";
            if (!IsActivated)
                status = "Offline";
        }

        protected override ConversionRecipe PrepareRecipe(double deltaTime)
        {
            recipeDeltaTime = deltaTime;
            activeRecipe = base.PrepareRecipe(deltaTime);
            return activeRecipe;
        }

        public override double GetEfficiencyMultiplier()
        {
            double requestedEfficiency = ApplyPowerPercentage(
                base.GetEfficiencyMultiplier(),
                powerPercentage);
            double limitedEfficiency = ApplyLoadFollowingLimit(requestedEfficiency);
            regulationLimitedOutputToZero =
                requestedEfficiency > ResourceTolerance &&
                limitedEfficiency <= ResourceTolerance;
            return limitedEfficiency;
        }

        protected override void PostProcess(
            ConverterResults converterResults,
            double deltaTime)
        {
            base.PostProcess(converterResults, deltaTime);

            double loadFraction = CalculateLoadFraction(
                converterResults.TimeFactor,
                deltaTime);
            double electricChargeRate = GetConfiguredOutputRate(
                ElectricChargeResourceName);
            currentPowerOutput = FormatPowerOutput(
                electricChargeRate * loadFraction *
                KilowattsPerElectricChargePerSecond);

            if (!IsActivated)
            {
                status = "Offline";
                currentPowerOutput = "0 W";
            }
            else if (powerPercentage <= ResourceTolerance)
            {
                status = "Output Disabled";
                currentPowerOutput = "0 W";
            }
            else if (regulationLimitedOutputToZero &&
                converterResults.TimeFactor <= ResourceTolerance)
            {
                status = "Standby";
                currentPowerOutput = "0 W";
            }
            else if (converterResults.TimeFactor <= ResourceTolerance &&
                string.Equals(
                    converterResults.Status,
                    Localizer.Format("#autoLOC_6002353"),
                    StringComparison.Ordinal))
            {
                status = "Starting";
            }
        }

        public override void StopResourceConverter()
        {
            base.StopResourceConverter();
            status = "Offline";
            currentPowerOutput = "0 W";
        }

        private double ApplyLoadFollowingLimit(double requestedEfficiency)
        {
            if (requestedEfficiency <= 0d) return 0d;
            if (regulatedOutputs.Count == 0 || activeRecipe == null ||
                recipeDeltaTime <= ResourceTolerance || part == null || vessel == null)
            {
                return requestedEfficiency;
            }

            double limitedEfficiency = requestedEfficiency;
            foreach (RegulatedOutput output in regulatedOutputs)
            {
                double outputRate = GetRecipeOutputRate(activeRecipe, output.ResourceName);
                double headroom;
                if (outputRate <= ResourceTolerance || !TryGetTargetHeadroom(output, out headroom))
                    return 0d;

                limitedEfficiency = LimitEfficiencyToHeadroom(
                    limitedEfficiency,
                    headroom,
                    outputRate,
                    recipeDeltaTime);
                if (limitedEfficiency <= 0d) return 0d;
            }

            return limitedEfficiency;
        }

        private bool TryGetTargetHeadroom(RegulatedOutput output, out double headroom)
        {
            headroom = 0d;
            if (output.ResourceId < 0) return false;

            double spareCapacity;
            double maxAmount;
            part.GetConnectedResourceTotals(
                output.ResourceId,
                output.FlowMode,
                out spareCapacity,
                out maxAmount,
                false);

            if (maxAmount <= ResourceTolerance) return false;
            headroom = CalculateTargetHeadroomFromSpareCapacity(
                spareCapacity,
                maxAmount,
                fillTarget);
            return !double.IsNaN(headroom) && !double.IsInfinity(headroom);
        }

        private static double CalculateTargetHeadroomFromSpareCapacity(
            double spareCapacity,
            double maxAmount,
            float targetFraction)
        {
            if (maxAmount <= ResourceTolerance) return 0d;

            // GetConnectedResourceTotals(..., pulling: false) returns the
            // output-reachable spare capacity, not the stored amount. Keep
            // the space above FillAmount reserved and use only the remainder.
            double reservedCapacity = maxAmount * (1d - targetFraction);
            return Math.Max(0d, spareCapacity - reservedCapacity);
        }

        private static double LimitEfficiencyToHeadroom(
            double requestedEfficiency,
            double headroom,
            double outputRate,
            double deltaTime)
        {
            if (requestedEfficiency <= 0d || headroom <= ResourceTolerance ||
                outputRate <= ResourceTolerance || deltaTime <= ResourceTolerance)
            {
                return 0d;
            }

            double requestedOutputAmount =
                outputRate * deltaTime * requestedEfficiency;
            if (headroom + ResourceTolerance >= requestedOutputAmount)
                return requestedEfficiency;

            double storageLimitedEfficiency = headroom / (outputRate * deltaTime);
            return Math.Max(0d, Math.Min(requestedEfficiency, storageLimitedEfficiency));
        }

        private static double GetRecipeOutputRate(ConversionRecipe recipe, string resourceName)
        {
            double outputRate = 0d;
            foreach (ResourceRatio output in recipe.Outputs)
            {
                if (!string.Equals(
                    output.ResourceName,
                    resourceName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                outputRate += Math.Max(0d, output.Ratio);
            }

            return outputRate;
        }

        private double GetConfiguredOutputRate(string resourceName)
        {
            double outputRate = 0d;
            foreach (ResourceRatio output in outputList)
            {
                if (!string.Equals(
                    output.ResourceName,
                    resourceName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                outputRate += Math.Max(0d, output.Ratio);
            }

            return outputRate;
        }

        private static double CalculateLoadFraction(
            double processedTime,
            double deltaTime)
        {
            if (processedTime <= ResourceTolerance ||
                deltaTime <= ResourceTolerance ||
                double.IsNaN(processedTime) ||
                double.IsInfinity(processedTime) ||
                double.IsNaN(deltaTime) ||
                double.IsInfinity(deltaTime))
            {
                return 0d;
            }

            return Math.Max(0d, processedTime / deltaTime);
        }

        private static string FormatPowerOutput(double kilowatts)
        {
            if (double.IsNaN(kilowatts) ||
                double.IsInfinity(kilowatts) ||
                kilowatts <= ResourceTolerance)
            {
                return "0 W";
            }

            if (kilowatts >= 1000000d)
                return (kilowatts / 1000000d).ToString("F2") + " GW";
            if (kilowatts >= 1000d)
                return (kilowatts / 1000d).ToString("F2") + " MW";
            if (kilowatts >= 1d)
                return kilowatts.ToString("F2") + " kW";

            return (kilowatts * 1000d).ToString("F0") + " W";
        }

        private static ConfigNode TranslateOutputResourceSettings(ConfigNode source)
        {
            ConfigNode translated = source.CreateCopy();

            foreach (ConfigNode outputNode in translated.GetNodes("OUTPUT_RESOURCE"))
            {
                string rawBlockedWhenFull = outputNode.GetValue("blocked_when_full");
                bool valid;
                bool dumpExcess = ResolveDumpExcess(rawBlockedWhenFull, out valid);

                if (!valid)
                {
                    string resourceName = outputNode.GetValue("ResourceName") ?? "unknown resource";
                    UnityEngine.Debug.LogError(
                        "[ArmorOverhaul] ModuleArcReactor OUTPUT_RESOURCE " + resourceName +
                        " has invalid blocked_when_full value '" + rawBlockedWhenFull +
                        "'; treating it as false.");
                }

                // A regulated output also keeps the stock storage limit as a
                // final guard against numerical overshoot. Unregulated outputs
                // vent excess by default.
                outputNode.RemoveValues("DumpExcess");
                outputNode.AddValue("DumpExcess", dumpExcess);
            }

            return translated;
        }

        private static bool ResolveDumpExcess(string rawBlockedWhenFull, out bool valid)
        {
            if (string.IsNullOrWhiteSpace(rawBlockedWhenFull))
            {
                valid = true;
                return true;
            }

            bool blockedWhenFull;
            valid = bool.TryParse(rawBlockedWhenFull.Trim(), out blockedWhenFull);
            return valid ? !blockedWhenFull : true;
        }

        private void LoadRegulatedOutputs(ConfigNode[] outputNodes)
        {
            regulatedOutputs.Clear();
            regulationConfigurationLoaded = true;

            foreach (ConfigNode outputNode in outputNodes)
            {
                bool blockedValueValid;
                bool dumpExcess = ResolveDumpExcess(
                    outputNode.GetValue("blocked_when_full"),
                    out blockedValueValid);
                if (!blockedValueValid || dumpExcess) continue;

                string resourceName = outputNode.GetValue("ResourceName");
                if (string.IsNullOrWhiteSpace(resourceName))
                {
                    UnityEngine.Debug.LogError(
                        "[ArmorOverhaul] ModuleArcReactor has a regulated OUTPUT_RESOURCE without ResourceName.");
                    continue;
                }

                regulatedOutputs.Add(new RegulatedOutput
                {
                    ResourceName = resourceName.Trim()
                });
            }
        }

        private void FindRegulatedOutputsInPartConfig()
        {
            if (part.partInfo == null || part.partInfo.partConfig == null) return;

            List<ConfigNode> moduleNodes = new List<ConfigNode>();
            foreach (ConfigNode moduleNode in part.partInfo.partConfig.GetNodes("MODULE"))
            {
                if (moduleNode.GetValue("name") != nameof(ModuleArcReactor)) continue;
                moduleNodes.Add(moduleNode);
            }

            int occurrence = 0;
            foreach (PartModule module in part.Modules)
            {
                ModuleArcReactor reactor = module as ModuleArcReactor;
                if (reactor == null) continue;
                if (ReferenceEquals(reactor, this)) break;
                occurrence++;
            }

            if (occurrence < 0 || occurrence >= moduleNodes.Count) return;

            ConfigNode[] outputNodes = moduleNodes[occurrence].GetNodes("OUTPUT_RESOURCE");
            if (outputNodes.Length > 0)
                LoadRegulatedOutputs(outputNodes);
        }

        private void ResolveRegulatedOutputResources()
        {
            foreach (RegulatedOutput regulatedOutput in regulatedOutputs)
            {
                PartResourceDefinition definition =
                    PartResourceLibrary.Instance.GetDefinition(regulatedOutput.ResourceName);
                if (definition == null)
                {
                    UnityEngine.Debug.LogError(
                        "[ArmorOverhaul] ModuleArcReactor could not resolve output resource " +
                        regulatedOutput.ResourceName + ".");
                    continue;
                }

                regulatedOutput.ResourceId = definition.id;
                regulatedOutput.FlowMode = definition.resourceFlowMode;

                foreach (ResourceRatio output in outputList)
                {
                    if (!string.Equals(
                        output.ResourceName,
                        regulatedOutput.ResourceName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (output.FlowMode != ResourceFlowMode.NULL)
                        regulatedOutput.FlowMode = output.FlowMode;
                    break;
                }
            }
        }

        private float NormalizeFillTarget()
        {
            if (!float.IsNaN(FillAmount) && !float.IsInfinity(FillAmount) &&
                FillAmount > 0f && FillAmount <= 1f)
            {
                return FillAmount;
            }

            UnityEngine.Debug.LogError(
                "[ArmorOverhaul] ModuleArcReactor FillAmount must be greater than 0 and no greater than 1; using 1.0.");
            FillAmount = 1f;
            return FillAmount;
        }

        private static double ApplyPowerPercentage(double baseEfficiency, float percentage)
        {
            return baseEfficiency * ClampPercentage(percentage) / 100d;
        }

        private static float ClampPercentage(float percentage)
        {
            if (float.IsNaN(percentage) || float.IsInfinity(percentage)) return 0f;
            return Math.Max(0f, Math.Min(100f, percentage));
        }

        private void ConfigureSlider()
        {
            if (float.IsNaN(sliderStep) || float.IsInfinity(sliderStep) ||
                sliderStep <= 0f || sliderStep > 100f)
            {
                sliderStep = 1f;
            }

            BaseField field = Fields[nameof(powerPercentage)];
            ConfigureSliderControl(field.uiControlEditor as UI_FloatRange);
            ConfigureSliderControl(field.uiControlFlight as UI_FloatRange);
        }

        private void ConfigureSliderControl(UI_FloatRange control)
        {
            if (control == null) return;

            control.minValue = 0f;
            control.maxValue = 100f;
            control.stepIncrement = sliderStep;
        }
    }
}
