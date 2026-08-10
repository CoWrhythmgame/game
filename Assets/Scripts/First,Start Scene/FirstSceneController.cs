using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstSceneController : MonoBehaviour
{
    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "StartScene";

    [Header("Boot Duration")]
    [SerializeField] private float waitSeconds = 0.05f;

    private static bool hasPassedBootScene = false;

    private void Start()
    {
        if (hasPassedBootScene)
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        hasPassedBootScene = true;
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        if (waitSeconds > 0f)
        {
            yield return new WaitForSeconds(waitSeconds);
        }
        else
        {
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}