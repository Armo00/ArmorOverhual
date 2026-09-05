using UnityEngine;

[assembly: KSPAssembly("ControlledCorridorLink", 1, 0)]

namespace ArmorOverhaul.ControlledCorridorLink
{
    /// <summary>
    /// Exposes KAS's existing interactive link event to the active vessel when
    /// that vessel has control. KAS remains responsible for all link-state and
    /// target validation.
    /// </summary>
    public sealed class ModuleControlledCorridorLink : PartModule
    {
        private const string KasSourceModuleName = "KASLinkSourceInteractive";
        private const string KasStartEventName = "StartLinkContextMenuAction";
        private const string KasBreakEventName = "BreakLinkContextMenuAction";
        private const string LogPrefix = "[ControlledCorridorLink] ";

        private BaseEvent kasStartEvent;
        private bool missingEventReported;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            FindKasStartEvent();
            UpdateEventVisibility();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!HighLogic.LoadedSceneIsFlight)
            {
                return;
            }

            if (kasStartEvent == null)
            {
                FindKasStartEvent();
            }

            UpdateEventVisibility();
        }

        private void FindKasStartEvent()
        {
            if (part == null)
            {
                return;
            }

            PartModule kasSource = null;
            for (int index = 0; index < part.Modules.Count; index++)
            {
                PartModule candidate = part.Modules[index];
                if (candidate != null && candidate.moduleName == KasSourceModuleName)
                {
                    kasSource = candidate;
                    break;
                }
            }

            if (kasSource != null && kasSource.Events != null)
            {
                kasStartEvent = kasSource.Events[KasStartEventName];
            }

            if (kasStartEvent == null && !missingEventReported)
            {
                missingEventReported = true;
                Debug.LogWarning(LogPrefix + "KAS start-link event was not found on part "
                    + part.partInfo?.name + "; remote corridor control is inactive for this part.");
            }
        }

        private void UpdateEventVisibility()
        {
            if (kasStartEvent == null)
            {
                return;
            }

            Vessel owner = vessel;
            bool canOperate = HighLogic.LoadedSceneIsFlight
                && owner != null
                && owner == FlightGlobals.ActiveVessel
                && !owner.packed
                && owner.IsControllable;

            // Do not touch guiActiveUnfocused or active. KAS owns EVA access and
            // the Available/Linking/Linked state of its event.
            kasStartEvent.guiActive = canOperate;

            // KAS owns a break event on the source and injects the same action
            // into the target while linked. Scan all modules so either endpoint
            // can expose the native disconnect action to a controlled vessel.
            for (int moduleIndex = 0; moduleIndex < part.Modules.Count; moduleIndex++)
            {
                PartModule module = part.Modules[moduleIndex];
                if (module == null || module.Events == null)
                {
                    continue;
                }

                BaseEvent breakEvent = module.Events[KasBreakEventName];
                if (breakEvent != null)
                {
                    breakEvent.guiActive = canOperate;
                }
            }
        }
    }
}
