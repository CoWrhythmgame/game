using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorInfoPanelToggle : MonoBehaviour
{
    [Header("Target Panel")]
    [SerializeField] private GameObject infoPanel;

    [Header("Toggle Button")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private TextMeshProUGUI toggleButtonText;

    [Header("Button Text")]
    [SerializeField] private string openedText = "<";
    [SerializeField] private string closedText = ">";

    private bool isOpen = true;

    private void Awake()
    {
        if (toggleButton == null)
            toggleButton = GetComponent<Button>();

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(TogglePanel);
    }

    public void TogglePanel()
    {
        isOpen = !isOpen;
        RefreshUI();
    }

    public void OpenPanel()
    {
        isOpen = true;
        RefreshUI();
    }

    public void ClosePanel()
    {
        isOpen = false;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (infoPanel != null)
            infoPanel.SetActive(isOpen);

        if (toggleButtonText != null)
            toggleButtonText.text = isOpen ? openedText : closedText;
    }
}