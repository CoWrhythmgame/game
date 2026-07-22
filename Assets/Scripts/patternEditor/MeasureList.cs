using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeasureList : MonoBehaviour
{
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
    private void OnScroll(float scrollData)
    {
        float scrollvalue = 2;

        if(scrollData < 0) scrollvalue *= -1;
        if(scrollvalue < 0 && _camera.transform.position.y <= 0) return;
        if(scrollvalue > 0 && _camera.transform.position.y >= _cameraMaxLength) return;


        _camera.transform.Translate(0,scrollvalue,0);

        RenderMeasure();
    }
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
    //확대, 축소, 삭제시 호출
    private void SyncList()
    {
        CurrentSync();
        RePositionAddButton();
        RePositionCamera();
    }
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
    //렌더된 measure만 syncmeasure 호출
    private void CurrentSync()
    {
        for(int i = -2; i < 3; i++)
        {
            _measures[Mathf.Clamp(_currentMeasure+i,0,_measureIndex-1)].GetComponent<Measure>().SyncMeasure();
        }
    }
    private void ScaleIncrease()
    {
        _scale++;
        ResizeMeasures();
    }
    private void ScaleDecrease()
    {
        if(_scale <= 1) return;
        _scale--;
        ResizeMeasures();
    }
    private void ResizeMeasures()
    {
        foreach(GameObject measure in _measures)
        {
            measure.transform.localScale = new Vector3(4,4*_scale,1);
        }
        CheckMaxLength();
        SyncList();
    }
    private void RePositionCamera()
    {
        _camera.transform.position = new Vector3(0,4*_scale*_currentMeasure, _camera.transform.position.z);
    }

    private void CheckMaxLength()
    {
        _cameraMaxLength = 4*_scale*_measureIndex;
        
    }
    private void RePositionAddButton()
    {
        _addButton.transform.position = new Vector3(0,_measureIndex*4*_scale-2f,0);
    }
}
