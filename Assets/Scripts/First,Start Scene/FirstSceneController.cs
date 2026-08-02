using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstSceneController : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "StartScene";

    public void GoToNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}