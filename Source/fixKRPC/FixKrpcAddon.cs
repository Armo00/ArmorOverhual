using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

[assembly: KSPAssembly("fixKRPC", 1, 0)]
[assembly: KSPAssemblyDependency("HarmonyKSP", 1, 0)]

namespace ArmorOverhaul.FixKRPC
{
    internal static class FixKrpcRuntime
    {
        internal const string HarmonyId = "DrArmor.ArmorOverhaul.fixKRPC";
        internal const string LogPrefix = "[fixKRPC] ";

        private static readonly Version SupportedKrpcVersion = new Version(0, 6, 0, 0);
        private static readonly HashSet<Guid> DetachedVessels = new HashSet<Guid>();

        internal static bool IsActive { get; private set; }
        internal static MethodInfo FlyMethod { get; private set; }
        internal static MethodInfo OnFlyByWireMethod { get; private set; }

        private static MethodInfo hasControlConnectionMethod;
        private static FieldInfo controlDelegatesField;
        private static FieldInfo sanctionedVesselsField;
        private static FieldInfo manualInputsField;
        private static PropertyInfo throttleProperty;
        private static PropertyInfo throttleUpdatedProperty;

        internal static bool Initialize()
        {
            Type pilotAddonType = AccessTools.TypeByName("KRPC.SpaceCenter.PilotAddon");
            if (pilotAddonType == null)
            {
                Debug.Log(LogPrefix + "KRPC.SpaceCenter.PilotAddon is not loaded; compatibility patch is inactive.");
                return false;
            }

            Version installedVersion = pilotAddonType.Assembly.GetName().Version;
            if (installedVersion != SupportedKrpcVersion)
            {
                Debug.LogError(LogPrefix + "unsupported KRPC.SpaceCenter version " + installedVersion
                    + "; expected " + SupportedKrpcVersion + ". No patch was applied.");
                return false;
            }

            Type controlInputsType = pilotAddonType.GetNestedType(
                "ControlInputs", BindingFlags.Public | BindingFlags.NonPublic);
            FlyMethod = AccessTools.Method(pilotAddonType, "Fly", new[] { typeof(Vessel) });
            OnFlyByWireMethod = AccessTools.Method(
                pilotAddonType, "OnFlyByWire", new[] { typeof(Vessel), typeof(FlightCtrlState) });
            hasControlConnectionMethod = AccessTools.Method(
                pilotAddonType, "HasControlConnection", new[] { typeof(Vessel) });
            controlDelegatesField = AccessTools.Field(pilotAddonType, "controlDelegates");
            sanctionedVesselsField = AccessTools.Field(pilotAddonType, "remoteTechSanctionedDelegates");
            manualInputsField = AccessTools.Field(pilotAddonType, "manualInputs");
            throttleProperty = controlInputsType == null ? null : AccessTools.Property(controlInputsType, "Throttle");
            throttleUpdatedProperty = controlInputsType == null
                ? null : AccessTools.Property(controlInputsType, "ThrottleUpdated");

            if (FlyMethod == null || OnFlyByWireMethod == null || hasControlConnectionMethod == null
                || controlDelegatesField == null || sanctionedVesselsField == null
                || manualInputsField == null || throttleProperty == null || throttleUpdatedProperty == null)
            {
                Debug.LogError(LogPrefix
                    + "KRPC 0.6.0 internals do not match the validated layout; no patch was applied.");
                return false;
            }

            IsActive = true;
            return true;
        }

        internal static void DetachOrdinaryCallbackWhenSanctioned(Vessel vessel)
        {
            if (!IsActive || vessel == null || !IsRemoteTechSanctioned(vessel))
            {
                return;
            }

            IDictionary delegates = controlDelegatesField.GetValue(null) as IDictionary;
            if (delegates == null || !delegates.Contains(vessel))
            {
                return;
            }

            Action<FlightCtrlState> source = delegates[vessel] as Action<FlightCtrlState>;
            if (source == null || vessel.OnFlyByWire == null)
            {
                return;
            }

            // KRPC registers `new FlightInputCallback(action)`. That conversion produces
            // a wrapper delegate whose target/method identity is not the identity of the
            // underlying Action. Recreate the same conversion and compare the delegates.
            FlightInputCallback callbackToRemove = new FlightInputCallback(source);
            bool isAttached = vessel.OnFlyByWire.GetInvocationList().Any(
                callback => callback.Equals(callbackToRemove));
            if (!isAttached)
            {
                return;
            }

            vessel.OnFlyByWire -= callbackToRemove;

            if (DetachedVessels.Add(vessel.id))
            {
                Debug.Log(LogPrefix + "removed duplicate ordinary kRPC FlyByWire callback from RemoteTech vessel "
                    + vessel.vesselName + "; sanctioned-pilot callback retained.");
            }
        }

        internal static void ForwardPendingThrottle(Vessel vessel, FlightCtrlState state)
        {
            if (!IsActive || vessel == null || state == null || !IsRemoteTechSanctioned(vessel))
            {
                return;
            }

            IDictionary manualInputs = manualInputsField.GetValue(null) as IDictionary;
            if (manualInputs == null || !manualInputs.Contains(vessel))
            {
                return;
            }

            object inputs = manualInputs[vessel];
            if (inputs == null || !(bool)throttleUpdatedProperty.GetValue(inputs, null))
            {
                return;
            }

            bool hasControlConnection = (bool)hasControlConnectionMethod.Invoke(null, new object[] { vessel });
            if (!hasControlConnection)
            {
                return;
            }

            float throttle = (float)throttleProperty.GetValue(inputs, null);
            state.mainThrottle = Mathf.Clamp01(throttle);
            Debug.Log(LogPrefix + "forwarded pending kRPC throttle " + state.mainThrottle.ToString("F3")
                + " to RemoteTech's authoritative FlightCtrlState for " + vessel.vesselName + ".");
        }

        private static bool IsRemoteTechSanctioned(Vessel vessel)
        {
            IEnumerable sanctionedVessels = sanctionedVesselsField.GetValue(null) as IEnumerable;
            if (sanctionedVessels == null)
            {
                return false;
            }

            foreach (object candidate in sanctionedVessels)
            {
                if (ReferenceEquals(candidate, vessel))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public sealed class FixKrpcBootstrap : MonoBehaviour
    {
        private static bool initialized;

        private void Awake()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            try
            {
                if (!FixKrpcRuntime.Initialize())
                {
                    return;
                }

                Harmony harmony = new Harmony(FixKrpcRuntime.HarmonyId);
                harmony.Patch(
                    FixKrpcRuntime.FlyMethod,
                    postfix: new HarmonyMethod(typeof(KrpcFlyPatch), nameof(KrpcFlyPatch.Postfix)));
                harmony.Patch(
                    FixKrpcRuntime.OnFlyByWireMethod,
                    prefix: new HarmonyMethod(typeof(KrpcOnFlyByWirePatch), nameof(KrpcOnFlyByWirePatch.Prefix)));

                Debug.Log(FixKrpcRuntime.LogPrefix
                    + "v1.0.1 active for KRPC 0.6.0: RemoteTech callback de-duplication and throttle forwarding enabled.");
            }
            catch (Exception exception)
            {
                Debug.LogError(FixKrpcRuntime.LogPrefix + "failed to install compatibility patch: " + exception);
            }
        }
    }

    internal static class KrpcFlyPatch
    {
        internal static void Postfix(Vessel vessel)
        {
            try
            {
                FixKrpcRuntime.DetachOrdinaryCallbackWhenSanctioned(vessel);
            }
            catch (Exception exception)
            {
                Debug.LogError(FixKrpcRuntime.LogPrefix
                    + "failed while removing duplicate FlyByWire callback: " + exception);
            }
        }
    }

    internal static class KrpcOnFlyByWirePatch
    {
        internal static void Prefix(Vessel vessel, FlightCtrlState state)
        {
            try
            {
                FixKrpcRuntime.ForwardPendingThrottle(vessel, state);
            }
            catch (Exception exception)
            {
                Debug.LogError(FixKrpcRuntime.LogPrefix
                    + "failed while forwarding throttle to FlightCtrlState: " + exception);
            }
        }
    }
}
