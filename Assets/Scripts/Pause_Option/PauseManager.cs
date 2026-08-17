using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.LowLevel;
public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Menu Texts")]
    [SerializeField] private TextMeshProUGUI[] menuTexts;

    [Header("Menu Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("Scene")]
    [SerializeField] private string songSelectSceneName = "TestSongSelectScene";

    private int currentIndex = 0;
    private float previousTimeScale = 1f;
    private static double pauseStartDspTime;
    private static double pauseStartInputTime;
    public static event System.Action OnGamePaused;
    public static event System.Action OnGameResumed;
    public static double TotalPausedDspTime { get; private set; }
    public static double TotalPausedInputTime { get; private set; }
    private void Awake()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        TotalPausedDspTime = 0d;
        TotalPausedInputTime = 0d;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        RefreshMenuUI();
    }

    private void OnEnable()
    {
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.performed += OnPauseInput;
            pauseAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.performed -= OnPauseInput;
            pauseAction.action.Disable();
        }
    }

    private void Update()
    {
        if (!IsPaused)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            MoveCursor(-1);
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            MoveCursor(1);
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            ExecuteCurrentMenu();
        }
    }

    private void OnPauseInput(InputAction.CallbackContext context)
    {
        // ESC ����
        // ���� ���̸� �Ͻ�����
        // �Ͻ����� ���̸� Continue�� �����ϰ� �簳
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused)
            return;

        IsPaused = true;
        currentIndex = 0;

        pauseStartDspTime = AudioSettings.dspTime;
        pauseStartInputTime = InputState.currentTime;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        RefreshMenuUI();
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (!IsPaused)
            return;

        TotalPausedDspTime += AudioSettings.dspTime - pauseStartDspTime;
        TotalPausedInputTime += InputState.currentTime - pauseStartInputTime;

        IsPaused = false;
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;

        if (pausePanel != null)
            pausePanel.SetActive(false);
        OnGameResumed?.Invoke();
    }

    public void RestartGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ExitToSongSelect()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        DataMaster dataMaster = GameObject.FindGameObjectWithTag("DataMaster").GetComponent<DataMaster>();
        if (dataMaster.GetIsTestPlay())
        {
            dataMaster.SetIsTestPlay(false);
            SceneManager.LoadScene("EditorScene");
        }
        else
        {
            SceneManager.LoadScene(songSelectSceneName);
        }

    }

    private void MoveCursor(int direction)
    {
        if (menuTexts == null || menuTexts.Length == 0)
            return;

        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = menuTexts.Length - 1;
        else if (currentIndex >= menuTexts.Length)
            currentIndex = 0;

        RefreshMenuUI();
    }

    private void ExecuteCurrentMenu()
    {
        switch (currentIndex)
        {
            case 0:
                ResumeGame();
                break;

            case 1:
                RestartGame();
                break;

            case 2:
                ExitToSongSelect();
                break;
        }
    }

    private void RefreshMenuUI()
    {
        if (menuTexts == null)
            return;

        for (int i = 0; i < menuTexts.Length; i++)
        {
            if (menuTexts[i] == null)
                continue;

            menuTexts[i].color = i == currentIndex ? selectedColor : normalColor;
        }
    }
}