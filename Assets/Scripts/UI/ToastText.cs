using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class ToastText : MonoBehaviour
{
    TMPro.TMP_Text toastText;
    double _startTime;
    double _duration;
    void Awake()
    {
        toastText = GetComponent<TMPro.TMP_Text>();
        toastText.enabled = false;
        _startTime = 0;
        _duration = 0;
    }
    void Update()
    {
        if(InputState.currentTime - _startTime >= _duration)
        {
            HideToast();
        }
    }
    public void ShowToast(string message, float duration)
    {
        toastText.text = message;
        toastText.enabled = true;
        _startTime = InputState.currentTime;
        _duration = duration;
    }
    private void HideToast()
    {
        toastText.text = "";
        toastText.enabled = false;
    }
}
