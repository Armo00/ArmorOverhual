using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

[assembly: KSPAssembly("RoboticsKJRCompat", 1, 4)]
[assembly: KSPAssemblyDependency("HarmonyKSP", 1, 0)]

namespace ArmorOverhaul.Robotics
{
    internal static class RoboticsKjrCompatibility
    {
        internal const string HarmonyId = "DrArmor.RoboticsKJRCompat";
        internal const string LogPrefix = "[RoboticsKJRCompat] ";

        internal static bool IsActive { get; set; }
        internal static Type BaseServoType { get; set; }
        internal static FieldInfo ServoIsLockedField { get; set; }
        internal static FieldInfo PrevServoIsLockedField { get; set; }
        internal static FieldInfo ServoNameField { get; set; }
        internal static MethodInfo ModifyServoLockedMethod { get; set; }

        private static readonly HashSet<uint> LoggedRoboticParts = new HashSet<uint>();
        private static readonly Dictionary<uint, bool> LastServoLockStates = new Dictionary<uint, bool>();
        private static readonly HashSet<Guid> SuspendedVessels = new HashSet<Guid>();

        internal static bool IsKjrLoaded()
        {
            return FindKjrAssembly() != null;
        }

        internal static Assembly FindKjrAssembly()
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                assembly => string.Equals(assembly.GetName().Name, "KerbalJointReinforcement", StringComparison.OrdinalIgnoreCase));
        }

        internal static bool IsVesselSuspended(Vessel vessel)
        {
            return vessel != null && SuspendedVessels.Contains(vessel.id);
        }

        internal static IEnumerable<PartModule> GetServos(Vessel vessel)
        {
            if (vessel == null || vessel.parts == null || BaseServoType == null)
            {
                yield break;
            }

            for (int partIndex = 0; partIndex < vessel.parts.Count; partIndex++)
            {
                Part part = vessel.parts[partIndex];
                if (part == null || part.Modules == null)
                {
                    continue;
                }

                for (int moduleIndex = 0; moduleIndex < part.Modules.Count; moduleIndex++)
                {
                    PartModule module = part.Modules[moduleIndex];
                    if (module != null && BaseServoType.IsInstanceOfType(module))
                    {
                        yield return module;
                    }
                }
            }
        }

        internal static bool IsServoLocked(PartModule servo)
        {
            return servo != null && ServoIsLockedField != null && (bool)ServoIsLockedField.GetValue(servo);
        }

        internal static string GetServoName(PartModule servo)
        {
            if (servo == null)
            {
                return "Unknown Servo";
            }

            string configuredName = ServoNameField == null ? null : ServoNameField.GetValue(servo) as string;
            if (!string.IsNullOrEmpty(configuredName))
            {
                return configuredName;
            }

            return servo.part != null && servo.part.partInfo != null ? servo.part.partInfo.title : "Unknown Servo";
        }

        internal static bool HasUnlockedServo(Vessel vessel)
        {
            return GetServos(vessel).Any(servo => !IsServoLocked(servo));
        }

        internal static void SuspendVesselAutoStruts(Vessel vessel, string reason)
        {
            if (!HighLogic.LoadedSceneIsFlight || vessel == null || vessel.parts == null)
            {
                return;
            }

            bool newlySuspended = SuspendedVessels.Add(vessel.id);
            for (int index = 0; index < vessel.parts.Count; index++)
            {
                Part part = vessel.parts[index];
                if (part != null)
                {
                    part.ReleaseAutoStruts();
                }
            }

            if (newlySuspended)
            {
                Debug.Log(LogPrefix + "suspended all physical Auto Struts on vessel "
                    + vessel.vesselName + " (" + reason + "); selected modes were preserved.");
            }
        }

        internal static void ResumeVesselAutoStrutsIfSafe(Vessel vessel, string reason)
        {
            if (vessel == null || !IsVesselSuspended(vessel) || HasUnlockedServo(vessel))
            {
                return;
            }

            SuspendedVessels.Remove(vessel.id);
            vessel.CycleAllAutoStrut();
            Debug.Log(LogPrefix + "restored Auto Struts on vessel " + vessel.vesselName
                + " from their preserved modes (" + reason + ").");
        }

        internal static void ForgetVessel(Vessel vessel)
        {
            if (vessel != null)
            {
                SuspendedVessels.Remove(vessel.id);
            }
        }

        internal static bool SetServoLocked(PartModule servo, bool locked)
        {
            if (servo == null || IsServoLocked(servo) == locked)
            {
                return true;
            }

            bool previousState = IsServoLocked(servo);
            try
            {
                // KSP's PAW first changes this KSPField and then invokes
                // ModifyLocked. Reproduce that exact route so stock events,
                // KJR cleanup and this plug-in's Harmony hooks all execute.
                ServoIsLockedField.SetValue(servo, locked);
                ModifyServoLockedMethod.Invoke(servo, new object[] { null });
                return IsServoLocked(servo) == locked;
            }
            catch (Exception exception)
            {
                ServoIsLockedField.SetValue(servo, previousState);
                Debug.LogError(LogPrefix + "failed to change servo lock state: " + exception);
                return false;
            }
        }

        internal static void LogDirectReinforcementBypass(Part part)
        {
            if (part != null && LoggedRoboticParts.Add(part.flightID))
            {
                Debug.Log(LogPrefix + "bypassed KJR direct reinforcement for robotic part "
                    + part.partInfo.name + " (" + part.flightID + ").");
            }
        }

        internal static void RecordServoLockState(object servo)
        {
            PartModule module = servo as PartModule;
            if (module == null || module.part == null || ServoIsLockedField == null)
            {
                return;
            }

            LastServoLockStates[module.part.flightID] = (bool)ServoIsLockedField.GetValue(servo);
        }

        internal static void PrepareServoLockTransition(object servo)
        {
            PartModule module = servo as PartModule;
            if (module == null || module.part == null || ServoIsLockedField == null || PrevServoIsLockedField == null)
            {
                return;
            }

            uint flightId = module.part.flightID;
            bool currentState = (bool)ServoIsLockedField.GetValue(servo);
            bool previousState;
            if (!LastServoLockStates.TryGetValue(flightId, out previousState))
            {
                LastServoLockStates[flightId] = currentState;
                previousState = !currentState;
            }

            if (!currentState)
            {
                // Runs before stock/KJR callbacks so constraints cannot be recreated mid-unlock.
                SuspendVesselAutoStruts(module.vessel, "servo unlock");
            }

            if (currentState == previousState)
            {
                return;
            }

            bool stockPreviousState = (bool)PrevServoIsLockedField.GetValue(servo);
            if (stockPreviousState != previousState)
            {
                PrevServoIsLockedField.SetValue(servo, previousState);
                Debug.Log(LogPrefix + "corrected stale servo lock transition for "
                    + module.part.partInfo.name + " (" + flightId + "): " + previousState + " -> " + currentState + ".");
            }
        }

        internal static void RecoverMissingPawControlCallback(PartModule servo)
        {
            if (servo == null || servo.part == null || ModifyServoLockedMethod == null)
            {
                return;
            }

            bool currentState = IsServoLocked(servo);
            bool lastProcessedState;
            if (LastServoLockStates.TryGetValue(servo.part.flightID, out lastProcessedState)
                && lastProcessedState == currentState)
            {
                return;
            }

            Debug.Log(LogPrefix + "recovered missing PAW UI_Control lock callback for "
                + servo.part.partInfo.name + " (" + servo.part.flightID + "): "
                + lastProcessedState + " -> " + currentState + ".");
            ModifyServoLockedMethod.Invoke(servo, new object[] { null });
        }

        internal static void CompleteServoLockTransition(object servo)
        {
            RecordServoLockState(servo);
            PartModule module = servo as PartModule;
            if (module != null && IsServoLocked(module))
            {
                ResumeVesselAutoStrutsIfSafe(module.vessel, "last servo locked");
            }
        }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public sealed class RoboticsKjrCompatBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (!RoboticsKjrCompatibility.IsKjrLoaded())
            {
                Debug.Log(RoboticsKjrCompatibility.LogPrefix
                    + "KerbalJointReinforcement is not loaded; compatibility patch is inactive.");
                return;
            }

            try
            {
                Type baseServoType = typeof(Part).Assembly.GetType("Expansions.Serenity.BaseServo");
                RoboticsKjrCompatibility.BaseServoType = baseServoType;
                RoboticsKjrCompatibility.ServoIsLockedField = AccessTools.Field(baseServoType, "servoIsLocked");
                RoboticsKjrCompatibility.PrevServoIsLockedField = AccessTools.Field(baseServoType, "prevServoIsLocked");
                RoboticsKjrCompatibility.ServoNameField = AccessTools.Field(baseServoType, "servoName");
                RoboticsKjrCompatibility.ModifyServoLockedMethod = AccessTools.Method(
                    baseServoType, "ModifyLocked", new[] { typeof(object) });

                Assembly kjrAssembly = RoboticsKjrCompatibility.FindKjrAssembly();
                Type kjrManagerType = kjrAssembly == null ? null : kjrAssembly.GetType("KerbalJointReinforcement.KJRManager");
                MethodInfo autoStrutAnchorTarget = AccessTools.Method(typeof(Part), "GetAutoStrutAnchor");
                MethodInfo updateAutoStrutTarget = AccessTools.Method(typeof(Part), "UpdateAutoStrut");
                MethodInfo kjrUpdatePartJointTarget = kjrManagerType == null
                    ? null : AccessTools.Method(kjrManagerType, "UpdatePartJoint", new[] { typeof(Part) });
                MethodInfo baseServoOnStartTarget = baseServoType == null
                    ? null : AccessTools.Method(baseServoType, "OnStart", new[] { typeof(PartModule.StartState) });
                MethodInfo baseServoModifyLockedTarget = baseServoType == null
                    ? null : AccessTools.Method(baseServoType, "ModifyLocked", new[] { typeof(object) });
                if (autoStrutAnchorTarget == null || updateAutoStrutTarget == null
                    || kjrUpdatePartJointTarget == null || baseServoOnStartTarget == null
                    || baseServoModifyLockedTarget == null
                    || RoboticsKjrCompatibility.ServoIsLockedField == null
                    || RoboticsKjrCompatibility.PrevServoIsLockedField == null
                    || RoboticsKjrCompatibility.ModifyServoLockedMethod == null)
                {
                    Debug.LogError(RoboticsKjrCompatibility.LogPrefix
                        + "could not locate one or more patch targets; no patch was applied.");
                    return;
                }

                Harmony harmony = new Harmony(RoboticsKjrCompatibility.HarmonyId);
                harmony.Patch(autoStrutAnchorTarget,
                    prefix: new HarmonyMethod(typeof(VesselAutoStrutAnchorPatch), nameof(VesselAutoStrutAnchorPatch.Prefix)));
                harmony.Patch(updateAutoStrutTarget,
                    prefix: new HarmonyMethod(typeof(VesselUpdateAutoStrutPatch), nameof(VesselUpdateAutoStrutPatch.Prefix)));
                harmony.Patch(kjrUpdatePartJointTarget,
                    prefix: new HarmonyMethod(typeof(KjrDirectRoboticJointPatch), nameof(KjrDirectRoboticJointPatch.Prefix)));
                harmony.Patch(baseServoOnStartTarget,
                    postfix: new HarmonyMethod(typeof(BaseServoLockStatePatch), nameof(BaseServoLockStatePatch.OnStartPostfix)));
                harmony.Patch(baseServoModifyLockedTarget,
                    prefix: new HarmonyMethod(typeof(BaseServoLockStatePatch), nameof(BaseServoLockStatePatch.ModifyLockedPrefix)),
                    postfix: new HarmonyMethod(typeof(BaseServoLockStatePatch), nameof(BaseServoLockStatePatch.ModifyLockedPostfix)));
                RoboticsKjrCompatibility.IsActive = true;
                Debug.Log(RoboticsKjrCompatibility.LogPrefix
                    + "active: vessel-wide Auto Strut suspension, KJR robotic-joint protection, and servo diagnostics enabled.");
            }
            catch (Exception exception)
            {
                Debug.LogError(RoboticsKjrCompatibility.LogPrefix + "failed to install compatibility patch: " + exception);
            }
        }
    }

    internal static class VesselAutoStrutAnchorPatch
    {
        internal static bool Prefix(Part __instance, ref Part __result,
            [HarmonyArgument(0)] ref AttachNode nodeFromPart,
            [HarmonyArgument(1)] ref AttachNode nodeFromParent,
            [HarmonyArgument(2)] ref bool srfAttached)
        {
            if (!RoboticsKjrCompatibility.IsVesselSuspended(__instance == null ? null : __instance.vessel))
            {
                return true;
            }

            nodeFromPart = null;
            nodeFromParent = null;
            srfAttached = false;
            __result = null;
            return false;
        }
    }

    internal static class VesselUpdateAutoStrutPatch
    {
        internal static bool Prefix(Part __instance)
        {
            return !RoboticsKjrCompatibility.IsVesselSuspended(__instance == null ? null : __instance.vessel);
        }
    }

    internal static class KjrDirectRoboticJointPatch
    {
        internal static bool Prefix([HarmonyArgument(0)] Part part)
        {
            if (part == null || !part.isRobotic())
            {
                return true;
            }

            RoboticsKjrCompatibility.LogDirectReinforcementBypass(part);
            return false;
        }
    }

    internal static class BaseServoLockStatePatch
    {
        internal static void OnStartPostfix(object __instance)
        {
            RoboticsKjrCompatibility.RecordServoLockState(__instance);
        }

        internal static void ModifyLockedPrefix(object __instance)
        {
            RoboticsKjrCompatibility.PrepareServoLockTransition(__instance);
        }

        internal static void ModifyLockedPostfix(object __instance)
        {
            RoboticsKjrCompatibility.CompleteServoLockTransition(__instance);
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public sealed class RoboticsKjrCompatFlightController : MonoBehaviour
    {
        private static bool subscribed;

        private void Start()
        {
            if (!RoboticsKjrCompatibility.IsActive)
            {
                return;
            }

            UnsubscribeEvents();
            GameEvents.onRoboticPartLockChanged.Add(OnRoboticPartLockChanged);
            GameEvents.onVesselGoOffRails.Add(OnVesselGoOffRails);
            GameEvents.onVesselDestroy.Add(OnVesselDestroy);
            subscribed = true;
            SanitizeVessel(FlightGlobals.ActiveVessel);
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private static void UnsubscribeEvents()
        {
            if (!subscribed)
            {
                return;
            }

            GameEvents.onRoboticPartLockChanged.Remove(OnRoboticPartLockChanged);
            GameEvents.onVesselGoOffRails.Remove(OnVesselGoOffRails);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
            InputLockManager.RemoveControlLock(RoboticsConstraintWindow.InputLockName);
            subscribed = false;
        }

        private static void OnVesselGoOffRails(Vessel vessel)
        {
            SanitizeVessel(vessel);
        }

        private static void OnVesselDestroy(Vessel vessel)
        {
            RoboticsKjrCompatibility.ForgetVessel(vessel);
        }

        private static void SanitizeVessel(Vessel vessel)
        {
            if (vessel == null || !RoboticsKjrCompatibility.HasUnlockedServo(vessel))
            {
                return;
            }

            RoboticsKjrCompatibility.SuspendVesselAutoStruts(vessel, "physics activation with unlocked servo");
            foreach (PartModule servo in RoboticsKjrCompatibility.GetServos(vessel))
            {
                if (!RoboticsKjrCompatibility.IsServoLocked(servo))
                {
                    GameEvents.onRoboticPartLockChanged.Fire(servo.part, false);
                }
            }
        }

        private static void OnRoboticPartLockChanged(Part part, bool servoIsLocked)
        {
            if (part == null)
            {
                return;
            }

            if (!servoIsLocked)
            {
                RoboticsKjrCompatibility.SuspendVesselAutoStruts(part.vessel, "robotic lock event");
                return;
            }

            RoboticsKjrCompatibility.ResumeVesselAutoStrutsIfSafe(part.vessel, "robotic lock event");
        }
    }

    public sealed class ModuleRoboticsConstraintManager : PartModule
    {
        private BaseField servoLockField;
        private Callback<BaseField, object> pawLockCallback;
        private PartModule servo;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (!HighLogic.LoadedSceneIsFlight || !RoboticsKjrCompatibility.IsActive
                || RoboticsKjrCompatibility.BaseServoType == null)
            {
                return;
            }

            for (int index = 0; index < part.Modules.Count; index++)
            {
                PartModule candidate = part.Modules[index];
                if (candidate != null && RoboticsKjrCompatibility.BaseServoType.IsInstanceOfType(candidate))
                {
                    servo = candidate;
                    break;
                }
            }

            if (servo == null)
            {
                return;
            }

            servoLockField = servo.Fields["servoIsLocked"];
            if (servoLockField == null || servoLockField.uiControlFlight == null)
            {
                return;
            }

            pawLockCallback = OnPawLockFieldChanged;
            servoLockField.uiControlFlight.onFieldChanged += pawLockCallback;
            Debug.Log(RoboticsKjrCompatibility.LogPrefix + "registered direct PAW lock callback for "
                + part.partInfo.name + " (" + part.flightID + ").");
        }

        private void OnDestroy()
        {
            if (servoLockField != null && servoLockField.uiControlFlight != null && pawLockCallback != null)
            {
                servoLockField.uiControlFlight.onFieldChanged -= pawLockCallback;
            }
        }

        private void OnPawLockFieldChanged(BaseField field, object previousValue)
        {
            RoboticsKjrCompatibility.RecoverMissingPawControlCallback(servo);
        }

        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Robotics / Auto Strut Manager")]
        public void OpenConstraintManager()
        {
            if (RoboticsConstraintWindow.Instance != null && vessel != null)
            {
                RoboticsConstraintWindow.Instance.Open(vessel);
            }
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public sealed class RoboticsConstraintWindow : MonoBehaviour
    {
        internal const string InputLockName = "RoboticsKJRCompatWindow";
        internal static RoboticsConstraintWindow Instance { get; private set; }

        private static readonly FieldInfo AutoStrutJointsField = AccessTools.Field(typeof(Part), "autoStrutJoints");
        private readonly int windowId = Guid.NewGuid().GetHashCode();
        private Rect windowRect = new Rect(380f, 120f, 690f, 620f);
        private Vector2 scrollPosition;
        private Vessel vessel;
        private bool visible;
        private string resultMessage = string.Empty;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            InputLockManager.RemoveControlLock(InputLockName);
        }

        internal void Open(Vessel selectedVessel)
        {
            vessel = selectedVessel;
            visible = true;
            resultMessage = string.Empty;
        }

        private void Update()
        {
            if (!visible)
            {
                InputLockManager.RemoveControlLock(InputLockName);
                return;
            }

            Vector2 mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            if (windowRect.Contains(mouse))
            {
                InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, InputLockName);
            }
            else
            {
                InputLockManager.RemoveControlLock(InputLockName);
            }
        }

        private void OnGUI()
        {
            if (!visible || vessel == null)
            {
                return;
            }

            GUI.skin = HighLogic.Skin;
            windowRect = GUILayout.Window(windowId, windowRect, DrawWindow,
                "Robotics / Auto Strut Manager - " + vessel.vesselName);
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            string suspension = RoboticsKjrCompatibility.IsVesselSuspended(vessel)
                ? "SUSPENDED (one or more servos unlocked)" : "ACTIVE";
            GUILayout.Label("Vessel Auto Struts: " + suspension);
            if (GUILayout.Button("Close", GUILayout.Width(70f)))
            {
                visible = false;
                InputLockManager.RemoveControlLock(InputLockName);
            }
            GUILayout.EndHorizontal();

            List<PartModule> servos = RoboticsKjrCompatibility.GetServos(vessel).ToList();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Servos: " + servos.Count, GUILayout.Width(100f));
            if (GUILayout.Button("Lock All", GUILayout.Width(110f)))
            {
                SetAllServos(servos, true);
            }
            if (GUILayout.Button("Unlock All", GUILayout.Width(110f)))
            {
                SetAllServos(servos, false);
            }
            GUILayout.Label(resultMessage);
            GUILayout.EndHorizontal();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(500f));
            GUILayout.Label("Servo lock states");
            for (int index = 0; index < servos.Count; index++)
            {
                PartModule servo = servos[index];
                bool locked = RoboticsKjrCompatibility.IsServoLocked(servo);
                GUILayout.BeginHorizontal();
                GUILayout.Label(RoboticsKjrCompatibility.GetServoName(servo), GUILayout.Width(330f));
                GUILayout.Label(locked ? "LOCKED" : "UNLOCKED", GUILayout.Width(100f));
                if (GUILayout.Button(locked ? "Unlock" : "Lock", GUILayout.Width(90f)))
                {
                    bool succeeded = RoboticsKjrCompatibility.SetServoLocked(servo, !locked);
                    resultMessage = succeeded ? "Command accepted." : "Servo rejected the lock command.";
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10f);
            GUILayout.Label("Part Auto Strut states (selected mode / physical joints)");
            if (vessel.parts != null)
            {
                for (int index = 0; index < vessel.parts.Count; index++)
                {
                    Part part = vessel.parts[index];
                    if (part == null)
                    {
                        continue;
                    }

                    GUILayout.BeginHorizontal();
                    string title = part.partInfo == null ? part.name : part.partInfo.title;
                    GUILayout.Label(title, GUILayout.Width(330f));
                    GUILayout.Label(part.autoStrutMode.ToString(), GUILayout.Width(140f));
                    GUILayout.Label(GetPhysicalAutoStrutCount(part).ToString(), GUILayout.Width(50f));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
            GUI.DragWindow(new Rect(0f, 0f, 610f, 28f));
        }

        private void SetAllServos(List<PartModule> servos, bool locked)
        {
            int failures = 0;
            for (int index = 0; index < servos.Count; index++)
            {
                if (!RoboticsKjrCompatibility.SetServoLocked(servos[index], locked))
                {
                    failures++;
                }
            }

            resultMessage = failures == 0
                ? (locked ? "All servos locked." : "All servos unlocked.")
                : failures + " servo command(s) failed.";
        }

        private static int GetPhysicalAutoStrutCount(Part part)
        {
            IList joints = AutoStrutJointsField == null ? null : AutoStrutJointsField.GetValue(part) as IList;
            return joints == null ? 0 : joints.Count;
        }
    }
}
