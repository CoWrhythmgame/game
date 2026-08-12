using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitConfirmController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private DirtyState dirtyState;
    [SerializeField] private SaveManager saveManager;

    [Header("Scene")]
    [SerializeField] private string targetSceneName = "FirstScene";

    private void Awake()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    public void RequestExit()
    {
        if (dirtyState == null)
        {
            LoadTargetScene();
            return;
        }

        if (!dirtyState.IsDirty)
        {
            LoadTargetScene();
            return;
        }

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
            EditorInputBlocker.SetBlocked(true);
        }
    }

    public void SaveAndExit()
    {
        if (saveManager == null)
        {
            Debug.LogWarning("SaveManager가 연결되지 않았습니다.");
            return;
        }

        bool saved = saveManager.SavePattern();

        if (!saved)
        {
            Debug.LogWarning("저장에 실패해서 나가기를 중단합니다.");
            return;
        }

        EditorInputBlocker.SetBlocked(false);
        LoadTargetScene();
    }

    public void ExitWithoutSave()
    {
        EditorInputBlocker.SetBlocked(false);
        LoadTargetScene();
    }

    public void CancelExit()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        EditorInputBlocker.SetBlocked(false);
    }

    private void LoadTargetScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}