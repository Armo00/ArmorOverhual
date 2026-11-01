using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using KSP.UI.Screens;
using UnityEngine;
using Debug = UnityEngine.Debug;

[assembly: KSPAssembly("EditorSearchOptimizer", 0, 1)]
[assembly: KSPAssemblyDependency("HarmonyKSP", 1, 0)]

namespace ArmorOverhaul.EditorSearch
{
    // Does not replace matching/sorting logic, thumbnails, or other mods' icon hooks.
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public sealed class SearchOptimizer : MonoBehaviour
    {
        internal const string Prefix = "[EditorSearchOptimizer] ";
        private const string PatchId = "DrArmor.EditorSearchOptimizer";
        private static readonly FieldInfo Category = AccessTools.Field(typeof(EditorPartList), "categoryList");
        private static readonly FieldInfo Icons = AccessTools.Field(typeof(EditorPartList), "icons");
        private static readonly FieldInfo Cache = AccessTools.Field(typeof(EditorPartList), "iconCache");
        private static readonly FieldInfo State = AccessTools.Field(typeof(EditorPartList), "state");
        private static readonly MethodInfo Clear = AccessTools.Method(typeof(EditorPartList), "ClearAllItems", Type.EmptyTypes);
        private static readonly MethodInfo UpdateIcon = AccessTools.Method(typeof(EditorPartList), "UpdatePartIcon", new[] { typeof(EditorPartIcon), typeof(AvailablePart), typeof(bool) });
        private static readonly MethodInfo UpdateIcons = AccessTools.Method(typeof(EditorPartList), "UpdatePartIcons", new[] { typeof(bool) });
        private static readonly MethodInfo Mask = AccessTools.Method(typeof(EditorPartList), "OnPartListUpdate", new[] { typeof(Vector2) });
        private static SearchJob active;
        private static bool fallback;
        private static bool enabledPatch;

        public void Awake()
        {
            try
            {
                if (Category == null || Icons == null || Cache == null || State == null || Clear == null || UpdateIcon == null || UpdateIcons == null || Mask == null)
                    throw new InvalidOperationException("EditorPartList API does not match; refusing to patch.");
                var harmony = new Harmony(PatchId);
                Patch(harmony, UpdateIcons, nameof(BuildPrefix), null);
                Patch(harmony, AccessTools.Method(typeof(EditorPartList), "Refresh", Type.EmptyTypes), nameof(CancelPrefix), null);
                Patch(harmony, AccessTools.Method(typeof(EditorPartList), "RefreshSearchList", Type.EmptyTypes), nameof(TimingPrefix), nameof(TimingPostfix));
                Patch(harmony, AccessTools.Method(typeof(BasePartCategorizer), "SearchField_OnValueChange"), nameof(CancelPrefix), null);
                Patch(harmony, AccessTools.Method(typeof(BasePartCategorizer), "SearchField_OnClick"), nameof(EmptyClickPrefix), null);
                enabledPatch = true;
                Debug.Log(Prefix + "v0.1.0 enabled: search icons budget=5ms, max=24/frame; stock matching and mod hooks retained.");
            }
            catch (Exception error)
            {
                enabledPatch = false;
                new Harmony(PatchId).UnpatchAll(PatchId);
                Debug.LogError(Prefix + error);
            }
        }

        private static void Patch(Harmony harmony, MethodInfo original, string before, string after)
        {
            if (original == null) throw new MissingMethodException("Patch target missing");
            harmony.Patch(original, before == null ? null : new HarmonyMethod(typeof(SearchOptimizer), before),
                after == null ? null : new HarmonyMethod(typeof(SearchOptimizer), after));
        }

        private static bool EmptyClickPrefix(BasePartCategorizer __instance)
        {
            return !enabledPatch || !(__instance is PartCategorizer) || !string.IsNullOrWhiteSpace(__instance.searchField.text);
        }

        private static void CancelPrefix()
        {
            if (active != null) active.Cancel();
        }

        private static void TimingPrefix(out Stopwatch __state) { __state = Stopwatch.StartNew(); }
        private static void TimingPostfix(EditorPartList __instance, Stopwatch __state)
        {
            var parts = Category.GetValue(__instance) as List<AvailablePart>;
            Debug.Log(Prefix + "filter/sort/dispatch=" + __state.Elapsed.TotalMilliseconds.ToString("F1") + "ms; matches=" + (parts == null ? 0 : parts.Count));
        }

        private static bool BuildPrefix(EditorPartList __instance, bool customCategory)
        {
            if (!enabledPatch || fallback) return true;
            CancelPrefix();
            if (customCategory || (EditorPartList.State)State.GetValue(__instance) != EditorPartList.State.PartSearch) return true;
            try
            {
                var list = (List<AvailablePart>)Category.GetValue(__instance);
                var job = __instance.gameObject.GetComponent<SearchJob>() ?? __instance.gameObject.AddComponent<SearchJob>();
                job.Begin(__instance, list.ToArray());
                return false;
            }
            catch (Exception error)
            {
                Debug.LogError(Prefix + "Dispatch failed; using stock path. " + error);
                return true;
            }
        }

        // Pure batch policy, also exercised by the regression harness.
        internal static bool BatchFinished(int count, double milliseconds) { return count >= 24 || milliseconds >= 5; }

        public sealed class SearchJob : MonoBehaviour
        {
            private int generation;
            private EditorPartList owner;
            private bool running;
            public void Cancel()
            {
                generation++;
                if (running) Debug.Log(Prefix + "cancelled stale search; already cached icons retained.");
                running = false;
            }
            private void OnDisable() { Cancel(); }
            private void OnDestroy() { Cancel(); if (ReferenceEquals(active, this)) active = null; owner = null; }
            public void Begin(EditorPartList list, AvailablePart[] parts)
            {
                owner = list;
                Cancel();
                active = this;
                running = true;
                StartCoroutine(Build(parts, generation));
            }
            private bool Current(int ticket)
            {
                return ticket == generation && owner != null && owner.isActiveAndEnabled
                    && (EditorPartList.State)State.GetValue(owner) == EditorPartList.State.PartSearch;
            }
            private void Layout()
            {
                var method = AccessTools.Method(AccessTools.TypeByName("VABOrganizer.SubcategorySorting"), "ForceLayoutRebuild");
                if (method != null) method.Invoke(null, new object[] { owner });
                Mask.Invoke(owner, new object[] { Vector2.zero });
            }
            private IEnumerator Build(AvailablePart[] parts, int ticket)
            {
                // Return immediately to input processing; never construct a cold result in one frame.
                yield return null;
                if (!Current(ticket)) yield break;
                var wall = Stopwatch.StartNew();
                var work = new Stopwatch();
                var cache = (Dictionary<string, EditorPartIcon>)Cache.GetValue(owner);
                var icons = (List<EditorPartIcon>)Icons.GetValue(owner);
                int built = 0, created = 0, frames = 0;
                double maxIcon = 0;
                Exception failure = null;
                try
                {
                    Clear.Invoke(owner, null);
                    owner.scrollRectState.SetState("parts");
                }
                catch (Exception error) { failure = error; }
                while (failure == null && built < parts.Length && Current(ticket))
                {
                    var batch = Stopwatch.StartNew();
                    int batchCount = 0;
                    work.Start();
                    while (built < parts.Length && !BatchFinished(batchCount, batch.Elapsed.TotalMilliseconds))
                    {
                        var part = parts[built];
                        EditorPartIcon icon = null;
                        bool fresh = false;
                        var perIcon = Stopwatch.StartNew();
                        try
                        {
                            if (!cache.TryGetValue(part.name, out icon) || icon == null)
                            {
                                fresh = true;
                                icon = UnityEngine.Object.Instantiate(owner.partPrefab);
                                icon.transform.SetParent(owner.partGrid, false);
                                icon.Create(owner, part, owner.iconSize, owner.iconOverScale, owner.iconOverSpin);
                                cache[part.name] = icon;
                                created++;
                            }
                            else icon.transform.SetAsLastSibling();
                            // Invoke the existing patched method: VABOrganizer grouping, greyout and mod behavior remain intact.
                            UpdateIcon.Invoke(owner, new object[] { icon, part, false });
                            icons.Add(icon);
                            built++;
                            batchCount++;
                            maxIcon = Math.Max(maxIcon, perIcon.Elapsed.TotalMilliseconds);
                        }
                        catch (Exception error)
                        {
                            if (fresh && icon != null && !cache.ContainsKey(part.name)) UnityEngine.Object.Destroy(icon.gameObject);
                            failure = error;
                            break;
                        }
                    }
                    work.Stop();
                    frames++;
                    if (built < parts.Length && failure == null) yield return null;
                }
                if (!Current(ticket)) yield break;
                running = false;
                if (failure != null)
                {
                    Debug.LogError(Prefix + "Batch failed; restoring stock list. " + failure);
                    try { fallback = true; UpdateIcons.Invoke(owner, new object[] { false }); }
                    finally { fallback = false; }
                    yield break;
                }
                var layout = Stopwatch.StartNew();
                try { Layout(); } catch (Exception error) { Debug.LogError(Prefix + "Layout: " + error); }
                Debug.Log(Prefix + "complete: results=" + built + "; newIcons=" + created + "; batches=" + frames
                    + "; iconCPU=" + work.Elapsed.TotalMilliseconds.ToString("F1") + "ms; maxIcon=" + maxIcon.ToString("F1")
                    + "ms; finalLayout=" + layout.Elapsed.TotalMilliseconds.ToString("F1") + "ms; wall=" + wall.Elapsed.TotalMilliseconds.ToString("F1") + "ms");
            }
        }
    }
}
