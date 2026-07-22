using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [System.Serializable]
    public class ResultMenuItem
    {
        public GameObject menuObject;
        public Image panelImage;
        public TextMeshProUGUI menuText;
    }

    [Header("Song Info")]
    [SerializeField] private TextMeshProUGUI songNameText;
    [SerializeField] private TextMeshProUGUI artistText;
    [SerializeField] private TextMeshProUGUI difficultyText;

    [Header("Judge Count")]
    [SerializeField] private TextMeshProUGUI perfectCountText;
    [SerializeField] private TextMeshProUGUI greatCountText;
    [SerializeField] private TextMeshProUGUI goodCountText;
    [SerializeField] private TextMeshProUGUI missCountText;

    [Header("Play Result")]
    [SerializeField] private TextMeshProUGUI maxComboCountText;
    [SerializeField] private TextMeshProUGUI fastCountText;
    [SerializeField] private TextMeshProUGUI lateCountText;

    [Header("Score / Rate")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rateText;

    [Header("Menu Items")]
    [SerializeField] private ResultMenuItem[] menuItems;

    [Header("Menu Colors")]
    [SerializeField] private Color normalPanelColor = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color selectedPanelColor = new Color(1f, 1f, 0f, 0.45f);
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color selectedTextColor = Color.yellow;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference leftAction;
    [SerializeField] private InputActionReference rightAction;
    [SerializeField] private InputActionReference submitAction;

    [Header("Scene Names")]
    [SerializeField] private string inGameSceneName = "InGameScene";
    [SerializeField] private string songSelectSceneName = "TestSongSelectScene";

    private int currentIndex = 0;

    private void Awake()
    {
        Time.timeScale = 1f;
        AutoFindMenuComponents();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPause();
        }

        if (DataMaster.Instance != null &&
            DataMaster.Instance.CurrentPlayData != null)
        {
            Song song = DataMaster.Instance.CurrentSong;
            int patternIndex = DataMaster.Instance.CurrentPatternIndex;
            PlayData playData = DataMaster.Instance.CurrentPlayData;

            ShowResult(song, patternIndex, playData);

            if (song != null)
            {
                FileManager.UpdateRecord(song.songname, patternIndex, playData);
            }
            else
            {
                Debug.LogWarning("ResultUI: CurrentSong이 없어서 Record 저장을 건너뜁니다.");
            }
        }
        else
        {
            Debug.LogWarning("ResultUI: 전달받은 PlayData가 없습니다. 더미 결과를 표시합니다.");
            ShowDummyResult();
        }

        RefreshMenuUI();
    }

    private void OnEnable()
    {
        RegisterAction(leftAction, OnLeftInput);
        RegisterAction(rightAction, OnRightInput);
        RegisterAction(submitAction, OnSubmitInput);
    }

    private void OnDisable()
    {
        UnregisterAction(leftAction, OnLeftInput);
        UnregisterAction(rightAction, OnRightInput);
        UnregisterAction(submitAction, OnSubmitInput);
    }

    private void RegisterAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null || actionReference.action == null)
            return;

        actionReference.action.performed += callback;
        actionReference.action.Enable();
    }

    private void UnregisterAction(InputActionReference actionReference, System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null || actionReference.action == null)
            return;

        actionReference.action.performed -= callback;
        actionReference.action.Disable();
    }

    private void OnLeftInput(InputAction.CallbackContext context)
    {
        MoveCursor(-1);
    }

    private void OnRightInput(InputAction.CallbackContext context)
    {
        MoveCursor(1);
    }

    private void OnSubmitInput(InputAction.CallbackContext context)
    {
        ExecuteCurrentMenu();
    }

    private void ShowDummyResult()
    {
        SetText(songNameText, "SongName");
        SetText(artistText, "Artist");
        SetText(difficultyText, "Difficulty");

        SetText(perfectCountText, "1000");
        SetText(greatCountText, "100");
        SetText(goodCountText, "10");
        SetText(missCountText, "1");

        SetText(maxComboCountText, "1000");
        SetText(fastCountText, "100");
        SetText(lateCountText, "100");

        SetText(scoreText, "1000000");
        SetText(rateText, "100.00%");
    }

    private void MoveCursor(int direction)
    {
        if (menuItems == null || menuItems.Length == 0)
            return;

        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = menuItems.Length - 1;
        else if (currentIndex >= menuItems.Length)
            currentIndex = 0;

        RefreshMenuUI();
    }

    private void ExecuteCurrentMenu()
    {
        switch (currentIndex)
        {
            case 0:
                RestartGame();
                break;

            case 1:
                ExitToSongSelect();
                break;
        }
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(inGameSceneName);
    }

    private void ExitToSongSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(songSelectSceneName);
    }

    private void RefreshMenuUI()
    {
        if (menuItems == null)
            return;

        for (int i = 0; i < menuItems.Length; i++)
        {
            ResultMenuItem item = menuItems[i];

            if (item == null)
                continue;

            bool isSelected = i == currentIndex;

            if (item.panelImage != null)
                item.panelImage.color = isSelected ? selectedPanelColor : normalPanelColor;

            if (item.menuText != null)
                item.menuText.color = isSelected ? selectedTextColor : normalTextColor;
        }
    }

    private void AutoFindMenuComponents()
    {
        if (menuItems == null)
            return;

        foreach (ResultMenuItem item in menuItems)
        {
            if (item == null || item.menuObject == null)
                continue;

            if (item.panelImage == null)
                item.panelImage = item.menuObject.GetComponent<Image>();

            if (item.menuText == null)
                item.menuText = item.menuObject.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
            text.text = value;
    }
    private void ShowResult(Song song, int patternIndex, PlayData playData)
    {
        string songName = song != null ? song.songname : "Unknown Song";
        string artist = song != null ? song.artist : "Unknown Artist";
        string difficulty = GetDifficultyName(patternIndex);

        SetText(songNameText, songName);
        SetText(artistText, artist);
        SetText(difficultyText, difficulty);

        SetText(scoreText, Mathf.RoundToInt(playData.score).ToString());
        SetText(rateText, playData.prate.ToString("0.00") + "%");

        SetText(maxComboCountText, playData.maxcombo.ToString());

        SetText(perfectCountText, GetArrayValue(playData.noteCount, 0).ToString());
        SetText(greatCountText, GetArrayValue(playData.noteCount, 1).ToString());
        SetText(goodCountText, GetArrayValue(playData.noteCount, 2).ToString());
        SetText(missCountText, GetArrayValue(playData.noteCount, 3).ToString());

        SetText(fastCountText, GetArrayValue(playData.fscount, 0).ToString());
        SetText(lateCountText, GetArrayValue(playData.fscount, 1).ToString());
    }

    private int GetArrayValue(int[] array, int index)
    {
        if (array == null)
        {
            return 0;
        }

        if (index < 0 || index >= array.Length)
        {
            return 0;
        }

        return array[index];
    }

    private string GetDifficultyName(int patternIndex)
    {
        switch (patternIndex)
        {
            case 0:
                return "1-Easy";
            case 1:
                return "2-Normal";
            case 2:
                return "3-Hard";
            case 3:
                return "4-Extreme";
            default:
                return "Unknown";
        }
    }
}