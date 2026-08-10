using System;
using UnityEngine;

public class InputPannel : MonoBehaviour
{
    [Header("ConnectedObject")]
    [SerializeField]private TMPro.TMP_Text _constText; 
    [SerializeField]private TMPro.TMP_InputField _variableInputField; 

    /// <summary>
    /// InputField가 작성 완료되었을때 발생하는 이벤트
    /// </summary>
    public Action<string> OnInputSubmitted;

    void Awake()
    {
        // _constText = transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
        // _variableInputField = transform.GetChild(1).GetComponent<TMPro.TMP_InputField>();
        _variableInputField.onSubmit.AddListener(InputSubmitHandler);
        // OnInputSubmitted += x=>Debug.Log(x);
    }
    /// <summary>
    /// OnInputSubmitted에 구독된 함수를 호출합니다.
    /// </summary>
    /// <param name="input">InputField 값</param>
    private void InputSubmitHandler(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            OnInputSubmitted?.Invoke(input);
        }
    }

    public void SetContent(string content)
    {
        SetInputField(content);
    }
    private void SetInputField(string content)
    {
        _variableInputField.text = content;
    }
    public string GetContent()
    {
        return _variableInputField.text;
    }

}
