using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuUI : MonoBehaviour
{
    [Header("Panel Images")]
    [SerializeField] private Image inGamePanelImage;
    [SerializeField] private Image anotherPanelImage;

    [Header("Menu Texts")]
    [SerializeField] private TextMeshProUGUI inGameText;
    [SerializeField] private TextMeshProUGUI anotherText;

    [Header("Scene Names")]
    [SerializeField] private string inGameSceneName = "TestSongSelectScene";
    [SerializeField] private string anotherSceneName = "";

    [Header("Text Colors")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color selectedTextColor = Color.yellow;

    [Header("Panel Colors")]
    [SerializeField] private Color normalPanelColor = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color selectedPanelColor = new Color(1f, 1f, 1f, 0.45f);

    private int currentIndex = 0;
    private const int menuCount = 2;

    private void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            MoveCursor(-1);
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            MoveCursor(1);
        }
        else if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            SelectCurrentMenu();
        }
    }

    private void MoveCursor(int direction)
    {
        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = menuCount - 1;
        else if (currentIndex >= menuCount)
            currentIndex = 0;

        RefreshUI();
    }

    private void SelectCurrentMenu()
    {
        switch (currentIndex)
        {
            case 0:
                LoadScene(inGameSceneName);
                break;

            case 1:
                if (string.IsNullOrEmpty(anotherSceneName))
                {
                    Debug.Log("Another scene is not assigned yet.");
                    return;
                }

                LoadScene(anotherSceneName);
                break;
        }
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Scene name is empty.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void RefreshUI()
    {
        bool isInGameSelected = currentIndex == 0;
        bool isAnotherSelected = currentIndex == 1;

        if (inGameText != null)
            inGameText.color = isInGameSelected ? selectedTextColor : normalTextColor;

        if (anotherText != null)
            anotherText.color = isAnotherSelected ? selectedTextColor : normalTextColor;

        if (inGamePanelImage != null)
            inGamePanelImage.color = isInGameSelected ? selectedPanelColor : normalPanelColor;

        if (anotherPanelImage != null)
            anotherPanelImage.color = isAnotherSelected ? selectedPanelColor : normalPanelColor;
    }
}