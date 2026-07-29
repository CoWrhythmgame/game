using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// * none이면 제작중(WIP)
public enum ToolbarType
{
    None,
    Signature,
    NoteType,
    ScaleUp,
    ScaleDown,
}
public class ToolBarButton : MonoBehaviour
{
    [Header("ButtonSetting")]
    [SerializeField] private MeasureList _measureList;
    [SerializeField] private ToolbarType _toolbarType = ToolbarType.None;
    [SerializeField] private TMPro.TMP_Text _buttonText;
    [SerializeField] private Image _buttonSprite;
    [Header("Data")]
    [SerializeField] private List<int> _signatureList = new List<int>(){4,6,8,12,16,1};
    [SerializeField] private List<Sprite> _sprites = new List<Sprite>();
    [SerializeField] private int _ListIndex = 0;
    
    private Button _button;
    void Start()
    {
        _button = transform.GetComponent<Button>();
        switch (_toolbarType)
        {
            case ToolbarType.Signature:
                _button.onClick.AddListener(ChangeSignature);
                break;
            case ToolbarType.NoteType:
                _button.onClick.AddListener(ChangeNoteType);
                break;
            case ToolbarType.ScaleUp:
                _button.onClick.AddListener(_measureList.ScaleIncrease);
                break;
            case ToolbarType.ScaleDown:
                _button.onClick.AddListener(_measureList.ScaleDecrease);
                break;

        }
        _button.onClick.AddListener(_measureList.OnToolbarChanged);
    }

    private void ChangeSignature()
    {
        _ListIndex++;
        if(_ListIndex == _signatureList.Count)
        {
            _ListIndex = 0;
        }
        _measureList.SetSignature(_signatureList[_ListIndex]);
        _buttonText.text = _signatureList[_ListIndex].ToString();
    }
    private void ChangeNoteType()
    {
        ChangeSprite();
        if(_ListIndex == 0)
        {
            _measureList.SetNoteType(NoteType.single);
        }else if(_ListIndex == 1){
            _measureList.SetNoteType(NoteType.hold);
        }
        else
        {
            Debug.LogWarning("NoteType:해당 분기가 없음");
        }
    }
    private void ChangeSprite()
    {
        _ListIndex++;
        if(_ListIndex == _sprites.Count) _ListIndex = 0;
        _buttonSprite.sprite = _sprites[_ListIndex];
    }

}
