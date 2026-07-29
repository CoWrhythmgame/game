using UnityEngine;

public class TextPannel : MonoBehaviour
{
    [Header("ConnectedObject")]
    [SerializeField]private TMPro.TMP_Text _constText; 
    [SerializeField]private TMPro.TMP_Text _variableText; 

    void Awake()
    {
        _constText = transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
        _variableText = transform.GetChild(1).GetComponent<TMPro.TMP_Text>();
    }

    /// <summary>
    /// variableText에 들어갈 텍스트를 설정합니다.
    /// </summary>
    /// <param name="content">텍스트 내용</param>
    public void SetContent(string content)
    {
        _variableText.text = content;
    }
    /// <summary>
    /// VariableText의 텍스트를 반환합니다.
    /// </summary>
    /// <returns>VariableText에 쓰여진 string값</returns>
    public string GetContent()
    {
        return _variableText.text;
    }
}
