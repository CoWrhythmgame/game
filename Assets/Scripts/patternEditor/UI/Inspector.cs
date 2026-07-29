using System;
using UnityEngine;
using UnityEngine.UI;

public class Inspector : MonoBehaviour
{
    #region Variables
    [Header("Connect Area")]
    [SerializeField] private GameObject _contentPannel;
    [SerializeField] private Measure _currentMeasure;
    [SerializeField] private TextPannel _measureIndexTextPannel;
    [SerializeField] private InputPannel _BPMInputPannel;
    [SerializeField] private Button _deleteButton;
    [Header("Data Area")]
    [SerializeField] private int _measureIndex = -1;
    [SerializeField] private float _BPM = -1;

    /// <summary>
    /// index를 매개변수로 호출하는 이벤트
    /// </summary>
    public Action<int> OnDeleteButtonClicked;
    #endregion


    #region LifeCycle
    void Start()
    {
        _deleteButton.onClick.AddListener(DeleteButtonHandler);
        _BPMInputPannel.OnInputSubmitted += SetBpmFromInputField;
        Hide();
    }
    #endregion
    #region CallBack
    /// <summary>
    /// 인스펙터의 delete버튼을 누르면 호출되는 함수
    /// </summary>
    private void DeleteButtonHandler()
    {
        OnDeleteButtonClicked?.Invoke(_measureIndex);
        Hide();
    }
    private void SetBpmFromInputField(string BPM)
    {
        float tempBPM = _BPM;
        if(float.TryParse(BPM, out _BPM))
        {
            _currentMeasure.OnInspectorBpmChanged(_BPM);
        }
        else
        {
            _BPM = tempBPM;
            _BPMInputPannel.SetContent(_BPM.ToString());
        }
    }
    #endregion
    #region Activate Area
    /// <summary>
    /// 인스펙터를 띄우고 값을 불러와 넣음
    /// </summary>
    /// <param name="index">마디선의 번호</param>
    /// <param name="BPM">마디선에 적용된 BPM</param>
    public void Show(Measure measure, int index, float BPM)
    {
        _currentMeasure = measure;
        _contentPannel.SetActive(true);
        _measureIndex = index;
        _BPM = BPM;
        _measureIndexTextPannel.SetContent(_measureIndex.ToString());
        _BPMInputPannel.SetContent(_BPM.ToString());
    }
    /// <summary>
    /// 인스펙터를 초기화하고 비활성화 상태로 만듬(숨김).
    /// </summary>
    public void Hide()
    {
        //디버깅용 초기화.
        //활성화되었는데 이 값이 -1이라면 데이터 전달이 잘 되었는지 확인할것.
        _currentMeasure = null;
        _measureIndex = -1;
        _BPM = -1;
        _contentPannel.SetActive(false);
    }

    #endregion




}
