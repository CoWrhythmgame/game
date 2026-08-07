using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorSongInfoUI : MonoBehaviour
{
    [Header("Song Info Texts")]
    [SerializeField] private TextMeshProUGUI songNameText;
    [SerializeField] private TextMeshProUGUI artistNameText;
    [SerializeField] private TextMeshProUGUI bpmText;

    [Header("Difficulty Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button extreamButton;

    [Header("Difficulty Texts")]
    [SerializeField] private TextMeshProUGUI easyText;
    [SerializeField] private TextMeshProUGUI normalText;
    [SerializeField] private TextMeshProUGUI hardText;
    [SerializeField] private TextMeshProUGUI extreamText;

    [Header("Difficulty Colors")]
    [SerializeField] private Color selectedButtonColor = new Color(1f, 0.9f, 0.2f, 1f);
    [SerializeField] private Color normalButtonColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color selectedTextColor = Color.black;
    [SerializeField] private Color normalTextColor = new Color(0.25f, 0.25f, 0.25f, 1f);

    private readonly string[] difficultyNames =
    {
        "Easy",
        "Normal",
        "Hard",
        "Extream"
    };

    private int currentDifficultyIndex = 0;

    public int CurrentDifficultyIndex => currentDifficultyIndex;
    public string CurrentDifficultyName => difficultyNames[currentDifficultyIndex];

    private void Awake()
    {
        RegisterDifficultyButtons();
    }

    private void Start()
    {
        SetDefaultSongInfo();
        SelectDifficulty(0);
    }

    private void RegisterDifficultyButtons()
    {
        if (easyButton != null)
            easyButton.onClick.AddListener(() => SelectDifficulty(0));

        if (normalButton != null)
            normalButton.onClick.AddListener(() => SelectDifficulty(1));

        if (hardButton != null)
            hardButton.onClick.AddListener(() => SelectDifficulty(2));

        if (extreamButton != null)
            extreamButton.onClick.AddListener(() => SelectDifficulty(3));
    }

    public void SetSongInfo(string songName, string artistName, float bpm)
    {
        SetText(songNameText, songName);
        SetText(artistNameText, artistName);
        SetText(bpmText, "BPM " + bpm.ToString("0.##"));
    }

    public void SetDefaultSongInfo()
    {
        SetText(songNameText, "Song Name");
        SetText(artistNameText, "Artist Name");
        SetText(bpmText, "BPM");
    }

    public void SelectDifficulty(int difficultyIndex)
    {
        if (difficultyIndex < 0 || difficultyIndex >= difficultyNames.Length)
            return;

        currentDifficultyIndex = difficultyIndex;
        RefreshDifficultyUI();

        Debug.Log("Selected Difficulty : " + CurrentDifficultyName);

        // ���߿� ���̵� ���� �� ä�� �ҷ������ ���� ����
        // LoadPatternByDifficulty(currentDifficultyIndex);
    }

    private void RefreshDifficultyUI()
    {
        RefreshOneDifficulty(0, easyButton, easyText);
        RefreshOneDifficulty(1, normalButton, normalText);
        RefreshOneDifficulty(2, hardButton, hardText);
        RefreshOneDifficulty(3, extreamButton, extreamText);
    }

    private void RefreshOneDifficulty(int index, Button button, TextMeshProUGUI text)
    {
        bool isSelected = index == currentDifficultyIndex;

        if (button != null)
        {
            Image buttonImage = button.GetComponent<Image>();

            if (buttonImage != null)
                buttonImage.color = isSelected ? selectedButtonColor : normalButtonColor;
        }

        if (text != null)
        {
            text.text = difficultyNames[index];
            text.color = isSelected ? selectedTextColor : normalTextColor;
        }
    }

    private void SetText(TextMeshProUGUI text, string value)
    {
        if (text == null)
            return;

        text.text = value;
    }
    public int GetDifficultyIndex()
    {
        return currentDifficultyIndex;
    }
}