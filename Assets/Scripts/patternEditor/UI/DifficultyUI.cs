using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorDifficultyUI : MonoBehaviour
{
    [Serializable]
    public class DifficultySlot
    {
        public string displayName;
        public Button difficultyButton;
        public Toggle enabledToggle;
        public TMP_InputField levelInput;
        public Image buttonImage;
        public TextMeshProUGUI buttonText;
    }

    [Header("References")]
    [SerializeField] private EditorSongFileLoader songFileLoader;
    [SerializeField] private MeasureList measureList;
    [SerializeField] private DirtyState dirtyState;

    [Header("Difficulty Slots")]
    [SerializeField] private DifficultySlot[] slots = new DifficultySlot[4];

    [Header("Colors")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color enabledColor = Color.white;
    [SerializeField] private Color disabledColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private int currentDifficultyIndex = -1;
    private bool isSongLoaded = false;
    private Pattern[] editingPatterns = new Pattern[4];
    private bool[] hasEditingPattern = new bool[4];
    public int CurrentDifficultyIndex => currentDifficultyIndex;

    private void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;

            if (slots[i].difficultyButton != null)
            {
                slots[i].difficultyButton.onClick.AddListener(() =>
                {
                    SelectDifficulty(index);
                });
            }

            if (slots[i].enabledToggle != null)
            {
                slots[i].enabledToggle.onValueChanged.AddListener((isOn) =>
                {
                    OnToggleChanged(index, isOn);
                });
            }

            if (slots[i].levelInput != null)
            {
                slots[i].levelInput.onEndEdit.AddListener((text) =>
                {
                    ClampLevelInput(index);
                });
            }
        }

        SetAllDisabledBeforeSongLoad();
    }

    private void OnEnable()
    {
        if (songFileLoader != null)
            songFileLoader.OnSongLoadedOrUpdated += OnSongLoadedOrUpdated;
    }

    private void OnDisable()
    {
        if (songFileLoader != null)
            songFileLoader.OnSongLoadedOrUpdated -= OnSongLoadedOrUpdated;
    }

    private void OnSongLoadedOrUpdated(EditorLoadedSongData songData)
    {
        InitializeAfterSongLoaded();
    }

    private void SetAllDisabledBeforeSongLoad()
    {
        isSongLoaded = false;
        currentDifficultyIndex = -1;

        for (int i = 0; i < slots.Length; i++)
        {
            SetToggleWithoutNotify(i, false);

            if (slots[i].enabledToggle != null)
                slots[i].enabledToggle.interactable = false;

            if (slots[i].difficultyButton != null)
                slots[i].difficultyButton.interactable = false;

            if (slots[i].levelInput != null)
            {
                slots[i].levelInput.text = "0.0";
                slots[i].levelInput.interactable = false;
            }
        }

        RefreshVisual();
    }

    private void InitializeAfterSongLoaded()
    {
        isSongLoaded = true;
        currentDifficultyIndex = -1;

        editingPatterns = new Pattern[4];
        hasEditingPattern = new bool[4];

        if (measureList != null)
            measureList.ClearPattern();

        for (int i = 0; i < slots.Length; i++)
        {
            SetToggleWithoutNotify(i, false);

            if (slots[i].enabledToggle != null)
                slots[i].enabledToggle.interactable = true;

            if (slots[i].difficultyButton != null)
                slots[i].difficultyButton.interactable = false;

            if (slots[i].levelInput != null)
            {
                slots[i].levelInput.text = "0.0";
                slots[i].levelInput.interactable = false;
            }
        }

        SetDifficultyEnabled(0, true);
        SelectDifficulty(0);

        RefreshVisual();
    }

    private void OnToggleChanged(int index, bool isOn)
    {
        if (!isSongLoaded)
            return;

        if (dirtyState != null)
            dirtyState.MarkDirty();

        SetDifficultyEnabled(index, isOn);

        if (isOn)
        {
            if (currentDifficultyIndex == -1)
                SelectDifficulty(index);
        }
        else
        {
            if (currentDifficultyIndex == index)
            {
                int nextIndex = FindFirstEnabledDifficulty();
                SelectDifficulty(nextIndex);
            }
        }

        RefreshVisual();
    }

    private void SetDifficultyEnabled(int index, bool enabled)
    {
        if (!IsValidIndex(index))
            return;

        SetToggleWithoutNotify(index, enabled);

        if (slots[index].difficultyButton != null)
            slots[index].difficultyButton.interactable = enabled;

        if (slots[index].levelInput != null)
        {
            slots[index].levelInput.interactable = enabled;

            if (!enabled)
                slots[index].levelInput.text = "0.0";
        }
    }

    private void SelectDifficulty(int index)
    {
        if (!IsValidIndex(index))
        {
            currentDifficultyIndex = -1;
            RefreshVisual();
            return;
        }

        if (!IsDifficultyEnabled(index))
            return;

        SaveCurrentEditingPattern();
        currentDifficultyIndex = index;
        LoadEditingPattern(index);
        Debug.Log("Selected Difficulty : " + GetDifficultyName(index));
        RefreshVisual();
    }

    private void SaveCurrentEditingPattern()
    {
        if (!IsValidIndex(currentDifficultyIndex))
            return;
        if (measureList == null)
            return;
        editingPatterns[currentDifficultyIndex] = measureList.GetPattern();
        hasEditingPattern[currentDifficultyIndex] = true;
    }

    private void LoadEditingPattern(int index)
    {
        if (measureList == null)
            return;
        if (!IsValidIndex(index))
            return;
        if (!hasEditingPattern[index] || editingPatterns[index] == null)
        {
            measureList.ClearPattern();
            return;
        }
        measureList.LoadPattern(editingPatterns[index]);
    }
    private int FindFirstEnabledDifficulty()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (IsDifficultyEnabled(i))
                return i;
        }
        return -1;
    }

    private void ClampLevelInput(int index)
    {
        if (!IsValidIndex(index))
            return;

        if (slots[index].levelInput == null)
            return;

        if (dirtyState != null)
            dirtyState.MarkDirty();

        if (!IsDifficultyEnabled(index))
        {
            slots[index].levelInput.text = "0.0";
            return;
        }

        string text = slots[index].levelInput.text;

        if (!float.TryParse(text, out float value))
            value = 1.0f;

        value = Mathf.Clamp(value, 1.0f, 15.0f);
        slots[index].levelInput.text = value.ToString("0.0");

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            bool enabled = IsDifficultyEnabled(i);
            bool selected = i == currentDifficultyIndex;

            Image image = slots[i].buttonImage;

            if (image == null && slots[i].difficultyButton != null)
                image = slots[i].difficultyButton.image;

            if (image != null)
            {
                if (selected)
                    image.color = selectedColor;
                else if (enabled)
                    image.color = enabledColor;
                else
                    image.color = disabledColor;
            }

            if (slots[i].buttonText != null)
            {
                if (enabled)
                    slots[i].buttonText.color = Color.black;
                else
                    slots[i].buttonText.color = Color.gray;
            }
        }
    }

    private void SetToggleWithoutNotify(int index, bool isOn)
    {
        if (!IsValidIndex(index))
            return;

        if (slots[index].enabledToggle != null)
            slots[index].enabledToggle.SetIsOnWithoutNotify(isOn);
    }

    public bool IsDifficultyEnabled(int index)
    {
        if (!IsValidIndex(index))
            return false;

        if (slots[index].enabledToggle == null)
            return false;

        return slots[index].enabledToggle.isOn;
    }

    public float GetDifficultyLevel(int index)
    {
        if (!IsValidIndex(index))
            return 0.0f;

        if (slots[index].levelInput == null)
            return 0.0f;

        if (!float.TryParse(slots[index].levelInput.text, out float value))
            return 0.0f;

        return value;
    }

    public string GetDifficultyName(int index)
    {
        if (!IsValidIndex(index))
            return "Unknown";

        if (!string.IsNullOrEmpty(slots[index].displayName))
            return slots[index].displayName;

        switch (index)
        {
            case 0:
                return "Easy";
            case 1:
                return "Normal";
            case 2:
                return "Hard";
            case 3:
                return "Extream";
            default:
                return "Unknown";
        }
    }

    public bool CanSaveCurrentDifficulty()
    {
        if (!isSongLoaded)
            return false;

        if (!IsValidIndex(currentDifficultyIndex))
            return false;

        if (!IsDifficultyEnabled(currentDifficultyIndex))
            return false;

        float level = GetDifficultyLevel(currentDifficultyIndex);
        return level >= 1.0f && level <= 15.0f;
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < slots.Length;
    }
    public void SaveCurrentPatternToMemory()
    {
        SaveCurrentEditingPattern();
    }

    public void ApplyImportedDifficulty(int index, bool enabled, float level)
    {
        if (!IsValidIndex(index))
            return;

        if (slots[index].enabledToggle != null)
            slots[index].enabledToggle.interactable = true;

        SetDifficultyEnabled(index, enabled);

        if (slots[index].levelInput != null)
        {
            slots[index].levelInput.text = level.ToString("0.0");
            slots[index].levelInput.interactable = enabled;
        }

        RefreshVisual();
    }
    public void SetImportedPattern(int index, Pattern pattern)
    {
        if (!IsValidIndex(index))
            return;

        editingPatterns[index] = pattern;
        hasEditingPattern[index] = pattern != null;
    }
    public void SelectImportedDifficulty(int index)
    {
        SelectDifficulty(index);
    }
    public void SetSongLoadedForImport()
    {
        isSongLoaded = true;
        currentDifficultyIndex = -1;

        for (int i = 0; i < slots.Length; i++)
        {
            SetToggleWithoutNotify(i, false);

            if (slots[i].enabledToggle != null)
                slots[i].enabledToggle.interactable = true;

            if (slots[i].difficultyButton != null)
                slots[i].difficultyButton.interactable = false;

            if (slots[i].levelInput != null)
            {
                slots[i].levelInput.text = "0.0";
                slots[i].levelInput.interactable = false;
            }
        }

        RefreshVisual();
    }
    public void BeginImport()
    {
        isSongLoaded = true;
        currentDifficultyIndex = -1;

        editingPatterns = new Pattern[4];
        hasEditingPattern = new bool[4];

        for (int i = 0; i < slots.Length; i++)
        {
            SetToggleWithoutNotify(i, false);

            if (slots[i].enabledToggle != null)
                slots[i].enabledToggle.interactable = true;

            if (slots[i].difficultyButton != null)
                slots[i].difficultyButton.interactable = false;

            if (slots[i].levelInput != null)
            {
                slots[i].levelInput.text = "0.0";
                slots[i].levelInput.interactable = false;
            }
        }

        RefreshVisual();
    }
}