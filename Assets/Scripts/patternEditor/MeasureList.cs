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

    [Header("CameraData")]
    [SerializeField] private int _currentMeasure = 0;
    [SerializeField] private float _cameraMaxLength = 0;

    [Header("SongData")]
    [SerializeField] private float _bpm = 120;

    [Header("Debug")]
    [SerializeField] private bool Debug_Increase = false;
    [SerializeField] private bool Debug_Decrease = false;

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
    /// 확대, 축소, 삭제시 호출
    /// </summary>
    private void SyncList()
    {
        CurrentSync();
        RePositionAddButton();
        RePositionCamera();
    }
    /// <summary>
    /// 렌더된 measure만 SyncMeasure() 호출
    /// </summary>
    private void CurrentSync()
    {
        for(int i = -2; i < 3; i++)
        {
            _measures[Mathf.Clamp(_currentMeasure+i,0,_measureIndex-1)].GetComponent<Measure>().SyncMeasure();
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
    private void ScaleIncrease()
    {
        _scale++;
        ResizeMeasures();
    }
    /// <summary>
    /// 마디선 크기 감소, 최소 1
    /// </summary>
    private void ScaleDecrease()
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
        SyncList();
    }
    /// <summary>
    /// 확대, 축소, 삭제시 카메라 위치를 이전에 보고있었던 마디선의 위치로 이동시킴
    /// </summary>
    private void RePositionCamera()
    {
        _camera.transform.position = new Vector3(0,4*_scale*_currentMeasure, _camera.transform.position.z);
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
}
