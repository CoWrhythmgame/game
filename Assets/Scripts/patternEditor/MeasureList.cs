using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeasureList : MonoBehaviour
{
    #region Variables
    [Header("Objects")]
    [SerializeField] private GameObject _measurePrefab;
    [SerializeField] private GameObject _addButton;
    [SerializeField] private GameObject _camera;

    [Header("MeasureData")]
    [SerializeField] private int _scale = 1;
    [SerializeField] private int _measureIndex = 0;
    [SerializeField] private int _signature = 4;
    [SerializeField] private NoteType _noteType = NoteType.single;

    [Header("CameraData")]
    [SerializeField] private int _currentMeasure = 0;
    [SerializeField] private float _cameraMaxLength = 0;

    [Header("SongData")]
    [SerializeField] private float _bpm = 120;
    

    [Header("Debug")]
    [SerializeField] private bool Debug_Increase = false;
    [SerializeField] private bool Debug_Decrease = false;
    [SerializeField] private bool Debug_Remove = false;

    private List<GameObject> _measures;
    #endregion

    #region LifeCycle
    void Awake()
    {
        _measures = new List<GameObject>();
        _camera = Camera.main.gameObject;
        AddMeasure();
    }
    void Update()
    {
        
        float scrollData = Mouse.current.scroll.ReadValue().y;
        if(scrollData != 0)
        {
            OnScroll(scrollData);
        }
        if (Debug_Increase)
        {
            ScaleIncrease();
            Debug_Increase = false;
        }
        if (Debug_Decrease)
        {
            ScaleDecrease();
            Debug_Decrease = false;
        }
        if (Debug_Remove)
        {
            DeleteMeasure(1);
            Debug_Remove = false;
        }
    }
    #endregion
    #region Callbacks
    private void OnScroll(float scrollData)
    {
        float scrollvalue = 2;

        if(scrollData < 0) scrollvalue *= -1;
        if(scrollvalue < 0 && _camera.transform.position.y <= 0) return;
        if(scrollvalue > 0 && _camera.transform.position.y >= _cameraMaxLength) return;


        _camera.transform.Translate(0,scrollvalue,0);

        RenderMeasure();
    }
    /// <summary>
    /// 확대, 축소, 삭제시 호출
    /// </summary>
    private void OnMeasureChanged()
    {
        CurrentChange();
        CheckMaxLength();
        RePositionAddButton();
        RePositionCamera();
    }
    public void OnToolbarChanged()
    {
        CurrentSync();
    }
    #endregion
    #region Core Logic
    /// <summary>
    /// 마디선 추가 함수
    /// </summary>
    public void AddMeasure()
    {
        GameObject measurePrefab = Instantiate(_measurePrefab, transform);
        measurePrefab.transform.position = Vector3.zero;
        GameObject measure = measurePrefab.transform.GetChild(0).gameObject;
        measure.transform.localScale = new Vector3(4,4f*_scale, 1);
        measure.GetComponent<Measure>().Initialize(_measureIndex, _bpm);
        _measures.Add(measure);

        _measureIndex++;
        CheckMaxLength();
        RePositionAddButton();
    }
    /// <summary>
    /// 마디선 삭제 함수
    /// </summary>
    /// <param name="index">삭제할 마디선 인덱스</param>
    public void DeleteMeasure(int index)
    {
        if(_measureIndex < index) return;
        if(_measureIndex <= 1) return;
        if(_measureIndex-1 == _currentMeasure) _currentMeasure--;
        Destroy(_measures[index].transform.parent.gameObject);
        _measures.RemoveAt(index);
        for(int i = index; i < _measures.Count; i++)
        {
            _measures[i].GetComponent<Measure>().OnIndexChanged(i);
        }
        _measureIndex--;
        if(Mathf.Abs(_currentMeasure-index) <= 2) {
            RenderMeasure();
        }
        OnMeasureChanged();
    }
    /// <summary>
    /// 렌더된 measure만 OnMeasureChanged() 호출
    /// </summary>
    private void CurrentChange()
    {
        for(int i = -2; i < 3; i++)
        {
            _measures[Mathf.Clamp(_currentMeasure+i,0,_measureIndex-1)].GetComponent<Measure>().OnMeasureChanged();
        }
    }
    /// <summary>
    /// 렌더된 measure만 OnToolbarChanged() 호출
    /// </summary>
    private void CurrentSync()
    {
        for(int i = -2; i < 3; i++)
        {
            _measures[Mathf.Clamp(_currentMeasure+i,0,_measureIndex-1)].GetComponent<Measure>().OnToolbarChanged();
        }
    }
    /// <summary>
    /// 현제 카메라 메인 마디선 주위 상하 2개의 마디선만 active함
    /// </summary>
    private void RenderMeasure()
    {
        _currentMeasure = (int)Mathf.Clamp(MathF.Floor((_camera.transform.position.y+4f)/(4f*_scale)),0,_measureIndex-1);
        foreach(GameObject measure in _measures)
        {
            measure.SetActive(false);
        }
        for(int i = -2; i < 3; i++)
        {
            _measures[Mathf.Clamp(_currentMeasure+i,0,_measureIndex-1)].SetActive(true);
        }
    }
    /// <summary>
    /// 마디선 크기 증가
    /// </summary>
    public void ScaleIncrease()
    {
        _scale++;
        ResizeMeasures();
    }
    /// <summary>
    /// 마디선 크기 감소, 최소 1
    /// </summary>
    public void ScaleDecrease()
    {
        if(_scale <= 1) return;
        _scale--;
        ResizeMeasures();
    }
    /// <summary>
    /// _scale값을 이용해 모든 마디선의 크기를 조정함
    /// </summary>
    private void ResizeMeasures()
    {
        foreach(GameObject measure in _measures)
        {
            measure.transform.localScale = new Vector3(4,4*_scale,1);
        }
        CheckMaxLength();
        OnMeasureChanged();
    }
    /// <summary>
    /// 확대, 축소, 삭제시 카메라 위치를 이전에 보고있었던 마디선의 위치로 이동시킴
    /// </summary>
    private void RePositionCamera()
    {
        //-0.01f 해준 이유는 스크롤 길이가 1일때 render 돌때 _currentMeasure가 자동으로 1씩 증가했기 때문(이 함수를 돌았을때 floor 내부 값이 1이 되는 문제)
        _camera.transform.position = new Vector3(0,4*_scale*_currentMeasure-0.01f, _camera.transform.position.z);
    }
    /// <summary>
    /// 마디선 추가, 삭제시 보면의 길이를 찾음
    /// </summary>
    private void CheckMaxLength()
    {
        _cameraMaxLength = 4*_scale*_measureIndex;
        
    }
    /// <summary>
    /// 마디선 추가/삭제시 마디선 추가 버튼을 재배치
    /// </summary>
    private void RePositionAddButton()
    {
        _addButton.transform.position = new Vector3(0,_measureIndex*4*_scale-2f,0);
    }
    #endregion
    #region Getter/Setter
    // 툴바에서 정보를 가져올때 사용하는 함수
    public void SetSignature(int Signature)
    {
        _signature = Signature;
    }
    public void SetNoteType(NoteType type)
    {
        _noteType = type;
    }
    // 툴바 정보에서 가져온 정보를 measure에 전달할때 사용되는 함수들
    public int GetSignature()
    {
        return _signature;
    }
    public NoteType GetNoteType()
    {
        return _noteType;
    }
    
    #endregion
}
