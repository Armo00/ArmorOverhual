using System;
using System.Collections.Generic;
using KSP.IO;
using KSP.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorOverhaul.CrewSorting
{
    /// <summary>
    /// Adds non-destructive sorting controls to the stock Available Crew list.
    /// The roster and vessel manifest are never reordered; only the existing
    /// UIList items are swapped through the stock list API.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public sealed class CrewListSorterAddon : MonoBehaviour
    {
        private const float ControlHeight = 26f;
        private const string LogPrefix = "[CrewListSorter] ";

        internal enum SortMode
        {
            Original,
            Name
        }

        private sealed class CrewEntry
        {
            public UIListItem Item;
            public string DisplayName;
            public int TypeOrder;
            public int RosterIndex;
        }

        private CrewAssignmentDialog dialog;
        private UIList availableList;
        private GameObject controlRoot;
        private Button originalButton;
        private Button nameButton;
        private Button directionButton;
        private Text directionText;
        private PluginConfiguration configuration;
        private SortMode sortMode = SortMode.Original;
        private bool descending;
        private bool sortPending;
        private int visibleListSignature;
        private readonly Vector3[] listWorldCorners = new Vector3[4];

        private void Awake()
        {
            LoadSettings();
            BaseCrewAssignmentDialog.onCrewDialogChange.Add(OnCrewDialogChange);
            GameEvents.onEditorShipCrewModified.Add(OnEditorShipCrewModified);
        }

        private void Update()
        {
            TryAttachToDialog();
            if (dialog == null || availableList == null) return;

            bool controlsVisible =
                dialog.gameObject.activeInHierarchy &&
                availableList.gameObject.activeInHierarchy;
            if (controlRoot != null && controlRoot.activeSelf != controlsVisible)
                controlRoot.SetActive(controlsVisible);

            if (!controlsVisible) return;

            int currentSignature = CalculateVisibleListSignature();
            if (currentSignature != visibleListSignature)
                sortPending = true;
        }

        private void LateUpdate()
        {
            if (controlRoot == null || !controlRoot.activeSelf) return;

            PositionControls();
            if (!sortPending) return;

            ApplyCurrentSort();
            sortPending = false;
            visibleListSignature = CalculateVisibleListSignature();
        }

        private void OnDestroy()
        {
            BaseCrewAssignmentDialog.onCrewDialogChange.Remove(OnCrewDialogChange);
            GameEvents.onEditorShipCrewModified.Remove(OnEditorShipCrewModified);
            DetachFromDialog();
        }

        private void TryAttachToDialog()
        {
            CrewAssignmentDialog current = CrewAssignmentDialog.Instance;
            if (current == dialog && dialog != null) return;

            DetachFromDialog();
            if (current == null || current.scrollListAvail == null) return;

            dialog = current;
            availableList = current.scrollListAvail;
            CreateControls();
            UpdateControlVisuals();
            sortPending = true;
        }

        private void DetachFromDialog()
        {
            if (controlRoot != null)
                Destroy(controlRoot);

            dialog = null;
            availableList = null;
            controlRoot = null;
            originalButton = null;
            nameButton = null;
            directionButton = null;
            directionText = null;
            visibleListSignature = 0;
        }

        private void OnCrewDialogChange(VesselCrewManifest manifest)
        {
            sortPending = true;
        }

        private void OnEditorShipCrewModified(VesselCrewManifest manifest)
        {
            sortPending = true;
        }

        private void SelectOriginalSort()
        {
            sortMode = SortMode.Original;
            OnSortSettingChanged();
        }

        private void SelectNameSort()
        {
            sortMode = SortMode.Name;
            OnSortSettingChanged();
        }

        private void ToggleDirection()
        {
            descending = !descending;
            OnSortSettingChanged();
        }

        private void OnSortSettingChanged()
        {
            UpdateControlVisuals();
            SaveSettings();
            sortPending = true;
        }

        private void ApplyCurrentSort()
        {
            if (availableList == null || dialog == null) return;

            List<UIListItem> currentItems =
                new List<UIListItem>(availableList.GetUiListItems());
            List<int> crewSlots = new List<int>();
            List<CrewEntry> entries = new List<CrewEntry>();
            KerbalRoster roster = dialog.CurrentCrewRoster;

            for (int index = 0; index < currentItems.Count; index++)
            {
                UIListItem item = currentItems[index];
                if (item == null) continue;

                CrewListItem crewItem = item.GetComponent<CrewListItem>();
                if (crewItem == null || crewItem.isEmpty) continue;

                ProtoCrewMember crew = crewItem.GetCrewRef();
                if (crew == null) continue;

                crewSlots.Add(index);
                entries.Add(new CrewEntry
                {
                    Item = item,
                    DisplayName = GetSortableName(crew),
                    TypeOrder = GetOriginalTypeOrder(crew.type),
                    RosterIndex = roster == null ? int.MaxValue : roster.IndexOf(crew)
                });
            }

            if (entries.Count < 2) return;

            entries.Sort((left, right) => CompareValues(
                sortMode,
                descending,
                left.DisplayName,
                left.TypeOrder,
                left.RosterIndex,
                right.DisplayName,
                right.TypeOrder,
                right.RosterIndex));

            for (int target = 0; target < crewSlots.Count; target++)
            {
                UIListItem desiredItem = entries[target].Item;
                UIListItem currentItem =
                    availableList.GetUilistItemAt(crewSlots[target]);
                if (currentItem == null || desiredItem == null ||
                    ReferenceEquals(currentItem, desiredItem))
                {
                    continue;
                }

                availableList.SwapItems(
                    currentItem,
                    desiredItem,
                    true,
                    true);
            }
        }

        internal static int CompareValues(
            SortMode mode,
            bool reverse,
            string leftName,
            int leftTypeOrder,
            int leftRosterIndex,
            string rightName,
            int rightTypeOrder,
            int rightRosterIndex)
        {
            int comparison;
            if (mode == SortMode.Name)
            {
                comparison = StringComparer.CurrentCultureIgnoreCase.Compare(
                    leftName ?? string.Empty,
                    rightName ?? string.Empty);
                if (comparison == 0)
                    comparison = CompareOriginalKeys(
                        leftTypeOrder,
                        leftRosterIndex,
                        rightTypeOrder,
                        rightRosterIndex);
            }
            else
            {
                comparison = CompareOriginalKeys(
                    leftTypeOrder,
                    leftRosterIndex,
                    rightTypeOrder,
                    rightRosterIndex);
            }

            comparison = Math.Sign(comparison);
            return reverse ? -comparison : comparison;
        }

        private static int CompareOriginalKeys(
            int leftTypeOrder,
            int leftRosterIndex,
            int rightTypeOrder,
            int rightRosterIndex)
        {
            int typeComparison = leftTypeOrder.CompareTo(rightTypeOrder);
            return typeComparison != 0
                ? typeComparison
                : leftRosterIndex.CompareTo(rightRosterIndex);
        }

        private static int GetOriginalTypeOrder(
            ProtoCrewMember.KerbalType kerbalType)
        {
            if (kerbalType == ProtoCrewMember.KerbalType.Crew) return 0;
            if (kerbalType == ProtoCrewMember.KerbalType.Tourist) return 1;
            return 2;
        }

        private static string GetSortableName(ProtoCrewMember crew)
        {
            if (!string.IsNullOrEmpty(crew.displayName))
                return crew.displayName;
            return crew.name ?? string.Empty;
        }

        private int CalculateVisibleListSignature()
        {
            if (availableList == null) return 0;

            unchecked
            {
                int signature = 17;
                List<UIListItem> items = availableList.GetUiListItems();
                signature = signature * 31 + items.Count;
                foreach (UIListItem item in items)
                {
                    CrewListItem crewItem =
                        item == null ? null : item.GetComponent<CrewListItem>();
                    ProtoCrewMember crew =
                        crewItem == null || crewItem.isEmpty
                            ? null
                            : crewItem.GetCrewRef();
                    signature = signature * 31 +
                        (crew == null ? 0 : (int)crew.persistentID);
                }

                return signature;
            }
        }

        private void CreateControls()
        {
            RectTransform dialogRect = dialog.transform as RectTransform;
            if (dialogRect == null) return;

            controlRoot = new GameObject(
                "CrewListSorterControls",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(LayoutElement),
                typeof(HorizontalLayoutGroup));
            controlRoot.transform.SetParent(dialogRect, false);
            controlRoot.transform.SetAsLastSibling();

            Image background = controlRoot.GetComponent<Image>();
            background.color = new Color(0.08f, 0.09f, 0.10f, 0.92f);
            background.raycastTarget = false;

            LayoutElement rootLayout = controlRoot.GetComponent<LayoutElement>();
            rootLayout.ignoreLayout = true;

            HorizontalLayoutGroup layout =
                controlRoot.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(3, 3, 2, 2);
            layout.spacing = 3f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            CreateLabel(controlRoot.transform, "Sort:", 34f);
            originalButton = CreateButton(
                controlRoot.transform,
                "Original",
                68f,
                SelectOriginalSort,
                out Text unusedOriginalText);
            nameButton = CreateButton(
                controlRoot.transform,
                "Name",
                52f,
                SelectNameSort,
                out Text unusedNameText);
            directionButton = CreateButton(
                controlRoot.transform,
                "ASC",
                44f,
                ToggleDirection,
                out directionText);
        }

        private void PositionControls()
        {
            if (controlRoot == null || availableList == null || dialog == null)
                return;

            RectTransform dialogRect = dialog.transform as RectTransform;
            RectTransform listRect =
                availableList.transform as RectTransform;
            RectTransform controlsRect =
                controlRoot.transform as RectTransform;
            if (dialogRect == null || listRect == null || controlsRect == null)
                return;

            listRect.GetWorldCorners(listWorldCorners);
            Vector3 firstCorner =
                dialogRect.InverseTransformPoint(listWorldCorners[0]);
            float minimumX = firstCorner.x;
            float maximumX = firstCorner.x;
            float maximumY = firstCorner.y;
            for (int index = 1; index < listWorldCorners.Length; index++)
            {
                Vector3 corner =
                    dialogRect.InverseTransformPoint(listWorldCorners[index]);
                minimumX = Math.Min(minimumX, corner.x);
                maximumX = Math.Max(maximumX, corner.x);
                maximumY = Math.Max(maximumY, corner.y);
            }

            controlsRect.anchorMin = new Vector2(0.5f, 0.5f);
            controlsRect.anchorMax = new Vector2(0.5f, 0.5f);
            controlsRect.pivot = new Vector2(0f, 0f);
            controlsRect.localPosition = new Vector3(
                minimumX,
                maximumY + 2f,
                0f);
            controlsRect.sizeDelta = new Vector2(
                Math.Max(180f, maximumX - minimumX),
                ControlHeight);
            controlRoot.transform.SetAsLastSibling();
        }

        private static void CreateLabel(
            Transform parent,
            string value,
            float width)
        {
            GameObject labelObject = new GameObject(
                "Label_" + value,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text),
                typeof(LayoutElement));
            labelObject.transform.SetParent(parent, false);

            Text label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 12;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = Color.white;
            label.text = value;
            label.raycastTarget = false;

            LayoutElement layout = labelObject.GetComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.minWidth = width;
        }

        private static Button CreateButton(
            Transform parent,
            string labelValue,
            float width,
            UnityEngine.Events.UnityAction callback,
            out Text label)
        {
            GameObject buttonObject = new GameObject(
                "Button_" + labelValue,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.23f, 0.25f, 0.27f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(callback);

            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.23f, 0.25f, 0.27f, 1f);
            colors.highlightedColor = new Color(0.34f, 0.37f, 0.40f, 1f);
            colors.pressedColor = new Color(0.15f, 0.17f, 0.19f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
            colors.colorMultiplier = 1f;
            button.colors = colors;

            LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.minWidth = width;

            GameObject textObject = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            label = textObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 11;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.text = labelValue;
            label.raycastTarget = false;

            return button;
        }

        private void UpdateControlVisuals()
        {
            SetButtonSelected(originalButton, sortMode == SortMode.Original);
            SetButtonSelected(nameButton, sortMode == SortMode.Name);
            if (directionText != null)
                directionText.text = descending ? "DESC" : "ASC";
        }

        private static void SetButtonSelected(Button button, bool selected)
        {
            if (button == null) return;

            ColorBlock colors = button.colors;
            colors.normalColor = selected
                ? new Color(0.18f, 0.45f, 0.62f, 1f)
                : new Color(0.23f, 0.25f, 0.27f, 1f);
            button.colors = colors;

            Image image = button.targetGraphic as Image;
            if (image != null)
                image.color = colors.normalColor;
        }

        private void LoadSettings()
        {
            try
            {
                configuration =
                    PluginConfiguration.CreateForType<CrewListSorterAddon>();
                configuration.load();

                string savedMode = configuration.GetValue(
                    "sortMode",
                    SortMode.Original.ToString());
                SortMode parsedMode;
                if (Enum.TryParse(savedMode, true, out parsedMode))
                    sortMode = parsedMode;

                descending = configuration.GetValue("descending", false);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    LogPrefix + "Could not load settings: " + exception.Message);
                configuration = null;
            }
        }

        private void SaveSettings()
        {
            if (configuration == null) return;

            try
            {
                configuration.SetValue("sortMode", sortMode.ToString());
                configuration.SetValue("descending", descending);
                configuration.save();
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    LogPrefix + "Could not save settings: " + exception.Message);
            }
        }
    }
}
