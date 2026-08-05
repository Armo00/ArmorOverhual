using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ArmorOverhaul
{
    public class ModuleRFInFlightConfigSwitcher : PartModule
    {
        private sealed class ConfigurationOption
        {
            public string Name;
            public string DisplayName;
        }

        [KSPField]
        public string engineID = string.Empty;

        [KSPField]
        public string allowedConfigurations = string.Empty;

        [KSPField]
        public bool allowSwitchWhileRunning = true;

        [KSPField]
        public bool keepWindowOpen = true;

        [KSPField]
        public bool showScreenMessage = true;

        [KSPField]
        public float switchCooldown = 0.25f;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Engine Configuration")]
        public string currentConfigurationDisplay = "Unavailable";

        private readonly List<ConfigurationOption> configurations = new List<ConfigurationOption>();
        private ModuleEnginesFX engine;
        private PartModule engineConfigs;
        private MethodInfo setConfiguration;
        private MethodInfo getConfigDisplayName;
        private FieldInfo configurationField;
        private FieldInfo configsField;
        private FieldInfo filteredDisplayConfigsField;
        private FieldInfo throttleResponseRateField;
        private PopupDialog configurationDialog;
        private bool initialized;
        private bool initializationFailed;
        private float lastSwitchTime = float.NegativeInfinity;
        private float nextDisplayRefresh;

        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Engine Configuration...")]
        public void ToggleConfigurationWindow()
        {
            if (configurationDialog != null)
            {
                CloseConfigurationWindow();
                return;
            }

            if (!initialized || configurations.Count == 0)
            {
                PostMessage("No switchable RF configurations were found.");
                return;
            }

            OpenConfigurationWindow();
        }

        [KSPAction("Next Engine Configuration")]
        public void NextConfigurationAction(KSPActionParam _)
        {
            CycleConfiguration(1);
        }

        [KSPAction("Previous Engine Configuration")]
        public void PreviousConfigurationAction(KSPActionParam _)
        {
            CycleConfiguration(-1);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
        }

        // Unity Start runs after RealFuels has populated ModuleEngineConfigs.
        public void Start()
        {
            Initialize();
        }

        public void Update()
        {
            if (!initialized || Time.realtimeSinceStartup < nextDisplayRefresh) return;

            nextDisplayRefresh = Time.realtimeSinceStartup + 0.25f;
            RefreshCurrentConfigurationDisplay();
        }

        public void OnDestroy()
        {
            CloseConfigurationWindow();
        }

        private void Initialize()
        {
            FindEngineConfigs();
            FindEngine();

            if (engineConfigs == null || engine == null || setConfiguration == null || configurationField == null)
            {
                FailInitialization("could not match ModuleEnginesRF and ModuleEngineConfigs");
                return;
            }

            DiscoverConfigurations();
            if (configurations.Count == 0)
            {
                FailInitialization("the matched ModuleEngineConfigs contains no available configurations");
                return;
            }

            throttleResponseRateField = FindField(engine.GetType(), "throttleResponseRate");
            initialized = true;
            RefreshCurrentConfigurationDisplay();
            ConfigureUserInterface(true);
        }

        private void FindEngineConfigs()
        {
            List<PartModule> candidates = new List<PartModule>();
            foreach (PartModule candidate in part.Modules)
            {
                if (candidate.moduleName != "ModuleEngineConfigs") continue;
                candidates.Add(candidate);
            }

            if (!string.IsNullOrEmpty(engineID))
            {
                foreach (PartModule candidate in candidates)
                {
                    FieldInfo candidateEngineIdField = FindField(candidate.GetType(), "engineID");
                    string candidateEngineId = candidateEngineIdField != null
                        ? candidateEngineIdField.GetValue(candidate) as string
                        : null;
                    if (candidateEngineId == engineID)
                    {
                        engineConfigs = candidate;
                        break;
                    }
                }
            }
            else if (candidates.Count == 1)
            {
                engineConfigs = candidates[0];
                FieldInfo candidateEngineIdField = FindField(engineConfigs.GetType(), "engineID");
                engineID = candidateEngineIdField != null
                    ? candidateEngineIdField.GetValue(engineConfigs) as string ?? string.Empty
                    : string.Empty;
            }

            if (engineConfigs == null) return;

            setConfiguration = engineConfigs.GetType().GetMethod(
                "SetConfiguration",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(string), typeof(bool) },
                null);
            getConfigDisplayName = engineConfigs.GetType().GetMethod(
                "GetConfigDisplayName",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(ConfigNode) },
                null);
            configurationField = FindField(engineConfigs.GetType(), "configuration");
            configsField = FindField(engineConfigs.GetType(), "configs");
            filteredDisplayConfigsField = FindField(engineConfigs.GetType(), "filteredDisplayConfigs");
        }

        private void FindEngine()
        {
            List<ModuleEnginesFX> candidates = part.FindModulesImplementing<ModuleEnginesFX>();

            if (!string.IsNullOrEmpty(engineID))
            {
                foreach (ModuleEnginesFX candidate in candidates)
                {
                    if (candidate.engineID == engineID)
                    {
                        engine = candidate;
                        return;
                    }
                }
            }
            else if (candidates.Count == 1)
            {
                engine = candidates[0];
                engineID = engine.engineID;
            }
        }

        private void DiscoverConfigurations()
        {
            configurations.Clear();
            HashSet<string> allowed = ParseAllowedConfigurations();

            IEnumerable configNodes = GetConfigNodeCollection(filteredDisplayConfigsField);
            if (configNodes == null || !CollectionHasItems(configNodes))
                configNodes = GetConfigNodeCollection(configsField);
            if (configNodes == null) return;

            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (object value in configNodes)
            {
                ConfigNode node = value as ConfigNode;
                if (node == null) continue;

                string name = node.GetValue("name");
                if (string.IsNullOrEmpty(name) || !seen.Add(name)) continue;
                if (allowed.Count > 0 && !allowed.Contains(name)) continue;

                configurations.Add(new ConfigurationOption
                {
                    Name = name,
                    DisplayName = ResolveDisplayName(node, name)
                });
            }
        }

        private IEnumerable GetConfigNodeCollection(FieldInfo field)
        {
            return field != null ? field.GetValue(engineConfigs) as IEnumerable : null;
        }

        private static bool CollectionHasItems(IEnumerable values)
        {
            IEnumerator enumerator = values.GetEnumerator();
            try
            {
                return enumerator.MoveNext();
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
        }

        private string ResolveDisplayName(ConfigNode node, string fallback)
        {
            if (getConfigDisplayName != null)
            {
                try
                {
                    string displayName = getConfigDisplayName.Invoke(engineConfigs, new object[] { node }) as string;
                    if (!string.IsNullOrEmpty(displayName)) return displayName;
                }
                catch (Exception)
                {
                    // Fall back to the stable RF configuration name.
                }
            }

            return fallback;
        }

        private HashSet<string> ParseAllowedConfigurations()
        {
            HashSet<string> result = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(allowedConfigurations)) return result;

            string[] values = allowedConfigurations.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string value in values)
            {
                string trimmed = value.Trim();
                if (trimmed.Length > 0) result.Add(trimmed);
            }

            return result;
        }

        private void OpenConfigurationWindow()
        {
            List<DialogGUIBase> controls = new List<DialogGUIBase>
            {
                new DialogGUILabel(
                    () => "Current: " + currentConfigurationDisplay,
                    true,
                    false)
            };

            foreach (ConfigurationOption optionValue in configurations)
            {
                ConfigurationOption option = optionValue;
                controls.Add(new DialogGUIButton(
                    () => ButtonLabel(option),
                    () => SelectConfiguration(option.Name),
                    () => CanSelectConfiguration(option.Name),
                    320f,
                    30f,
                    !keepWindowOpen));
            }

            controls.Add(new DialogGUIButton("Close", CloseConfigurationWindow, false));

            string partTitle = part.partInfo != null ? part.partInfo.title : part.name;
            MultiOptionDialog dialog = new MultiOptionDialog(
                "ArmorOverhaul_RFConfig_" + part.flightID,
                string.Empty,
                partTitle + " - Engine Configuration",
                HighLogic.UISkin,
                new DialogGUIVerticalLayout(controls.ToArray()));

            configurationDialog = PopupDialog.SpawnPopupDialog(
                dialog,
                false,
                HighLogic.UISkin,
                false,
                "ArmorOverhaulRFConfigSwitcher");
            configurationDialog.SetDraggable(true);
        }

        private string ButtonLabel(ConfigurationOption option)
        {
            return option.Name == CurrentConfigurationName()
                ? "[Current] " + option.DisplayName
                : option.DisplayName;
        }

        private bool CanSelectConfiguration(string configurationName)
        {
            if (!initialized || configurationName == CurrentConfigurationName()) return false;
            if (!allowSwitchWhileRunning && engine != null && engine.EngineIgnited) return false;
            return Time.realtimeSinceStartup - lastSwitchTime >= switchCooldown;
        }

        private void CloseConfigurationWindow()
        {
            if (configurationDialog == null) return;

            configurationDialog.Dismiss();
            configurationDialog = null;
        }

        private void CycleConfiguration(int direction)
        {
            if (!initialized || configurations.Count < 2) return;

            string current = CurrentConfigurationName();
            int currentIndex = configurations.FindIndex(option => option.Name == current);
            if (currentIndex < 0) currentIndex = direction > 0 ? -1 : 0;

            int nextIndex = (currentIndex + direction + configurations.Count) % configurations.Count;
            SelectConfiguration(configurations[nextIndex].Name);
        }

        private void SelectConfiguration(string configurationName)
        {
            if (!CanSelectConfiguration(configurationName)) return;

            float previousLimiter = engine.thrustPercentage;
            object originalResponseRate = throttleResponseRateField != null
                ? throttleResponseRateField.GetValue(engine)
                : null;

            try
            {
                setConfiguration.Invoke(engineConfigs, new object[] { configurationName, false });
                engine.thrustPercentage = previousLimiter;
                lastSwitchTime = Time.realtimeSinceStartup;
                RefreshCurrentConfigurationDisplay();

                if (originalResponseRate is float)
                    StartCoroutine(TemporarilyRemoveSpoolUp((float)originalResponseRate));

                if (showScreenMessage)
                    PostMessage("Engine configuration: " + currentConfigurationDisplay);

                if (!keepWindowOpen)
                    configurationDialog = null;
            }
            catch (TargetInvocationException exception)
            {
                Exception cause = exception.InnerException ?? exception;
                Debug.LogError("[ArmorOverhaul] RF configuration switch failed: " + cause);
                PostMessage("Engine configuration switch failed. See KSP.log.");
            }
            catch (Exception exception)
            {
                Debug.LogError("[ArmorOverhaul] RF configuration switch failed: " + exception);
                PostMessage("Engine configuration switch failed. See KSP.log.");
            }
        }

        private IEnumerator TemporarilyRemoveSpoolUp(float originalRate)
        {
            throttleResponseRateField.SetValue(engine, 1000000f);
            yield return new WaitForFixedUpdate();

            if (engine != null)
                throttleResponseRateField.SetValue(engine, originalRate);
        }

        private string CurrentConfigurationName()
        {
            return configurationField != null
                ? configurationField.GetValue(engineConfigs) as string ?? string.Empty
                : string.Empty;
        }

        private void RefreshCurrentConfigurationDisplay()
        {
            string current = CurrentConfigurationName();
            ConfigurationOption option = configurations.Find(value => value.Name == current);
            currentConfigurationDisplay = option != null ? option.DisplayName : current;
            if (string.IsNullOrEmpty(currentConfigurationDisplay))
                currentConfigurationDisplay = "Unavailable";
        }

        private void ConfigureUserInterface(bool available)
        {
            Events[nameof(ToggleConfigurationWindow)].guiActive = available;
            Actions[nameof(NextConfigurationAction)].active = available && configurations.Count > 1;
            Actions[nameof(PreviousConfigurationAction)].active = available && configurations.Count > 1;
            Fields[nameof(currentConfigurationDisplay)].guiActive = available;
        }

        private void FailInitialization(string reason)
        {
            if (initializationFailed) return;
            initializationFailed = true;
            ConfigureUserInterface(false);
            Debug.LogError("[ArmorOverhaul] " + part.name + " RF config switcher " + reason + ".");
        }

        private void PostMessage(string message)
        {
            ScreenMessages.PostScreenMessage(message, 3f, ScreenMessageStyle.UPPER_CENTER);
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(
                    fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null) return field;
                type = type.BaseType;
            }

            return null;
        }
    }
}
