using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Measure : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject _notePrefab;
    [SerializeField] private GameObject _shadeNote;
    [SerializeField] private TMPro.TMP_Text _text;
    [Header("Lines")]
    [SerializeField] private List<GameObject> _Lines_4;
    [SerializeField] private List<GameObject> _Lines_6;
    [SerializeField] private List<GameObject> _Lines_8;
    [Header("FromToolbar")]
    [SerializeField] private int _signature = 4;
    [Header("SongData")]
    [SerializeField] private float _bpm = 120;
    [Header("Debug")]
    [SerializeField] private bool Debug_Increase = false;
    [SerializeField] private bool Debug_Decrease = false;
    private int _MeasureIndex = 0;
    private Transform _transform;
    private Vector3 _mousePos;
    private void Awake()
    {
        _transform = transform.GetComponent<Transform>();
    }
    public void Initialize(int index, float bpm)
    {
        _MeasureIndex = index;
        _text.text = _MeasureIndex.ToString();
        _bpm = bpm;
        SyncMeasure();
    }
    private void OnEnable()
    {
        SyncMeasure();
        OnToolbarChanged();
    }
    
    private void Update()
    {
        _mousePos = GetMouseWorldPosition();
        if (IsMouseOn())
        {
            OnOverMouse();
        }
        else
        {
            OnOffMouse();
        }
        if (Debug_Increase)
        {
            Debug_SizeIncrease();
            Debug_Increase=false;
        }
        if (Debug_Decrease)
        {
            Debug_SizeDecrease();
            Debug_Decrease=false;
        }
        OnToolbarChanged();//디버그용. 확인가능한 수단이 만들어지면 반드시 제거할것
    }
    private void OnOverMouse()
    {
        _shadeNote.SetActive(true);
        _shadeNote.transform.localPosition = GetGridVector3(_mousePos);
    }
    private void OnOffMouse()
    {
        _shadeNote.SetActive(false);
    }
    //툴바 조작 시 호출
    private void OnToolbarChanged()
    {
        
        // _signature = (박자 설정에서 값 가져옴)
        RenderLines(_signature);
    }
    private void RenderLines(int signature)
    {
    
        foreach(GameObject line in _Lines_4)
        {
            line.SetActive(false);
        }
        foreach(GameObject line in _Lines_6)
        {
            line.SetActive(false);
        }
        foreach(GameObject line in _Lines_8)
        {
            line.SetActive(false);
        }
        if(signature % 4 == 0)
        {
            foreach(GameObject line in _Lines_4)
            {
                line.SetActive(true);
            }
        }
        if(signature % 6 == 0)
        {
            foreach(GameObject line in _Lines_6)
            {
                line.SetActive(true);
            }
        }
        if(signature % 8 == 0)
        {
            foreach(GameObject line in _Lines_8)
            {
                line.SetActive(true);
            }
        }
    }
    //확대, 축소, 삭제시 어긋타는걸 막기위한 함수(최적화를 위해 활성화되었을때 호출)
    public void SyncMeasure()
    {
        MeasureRePosition();
    }
    private Vector3 GetMouseWorldPosition()
    {   
        // 1. 화면 픽셀 좌표 가져오기
        Vector2 screenPos = Mouse.current.position.ReadValue();
        
        // 2. 메인 카메라를 이용해 월드 좌표로 변환
        // Z값이 카메라 위치로 고정되므로, 2D 평면인 Z=0으로 맞춰줍니다.
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        
        return worldPos;
    }
    private Vector3 GetGridVector3(Vector3 vector3)
    {
        float x = (MathF.Floor(vector3.x)+1f/2)/_transform.localScale.x;
        float y = Mathf.Floor(vector3.y*_signature/_transform.localScale.y)/_signature;
        return new Vector3(x,y,0)-_transform.position/_transform.localScale.y;
    }
    private bool IsMouseOn()
    {
        if(Mathf.Abs(_transform.position.x-_mousePos.x) < _transform.localScale.x / 2)
        {
            if(_mousePos.y - _transform.position.y < _transform.localScale.y && 0 <_mousePos.y - _transform.position.y)
            {
                return true;
            }
        }
        return false;
    }
    private void MeasureRePosition()
    {
        _transform.position = new Vector3(0,_MeasureIndex*_transform.localScale.y-4f,0);
    }
    #region Debug
    private void Debug_SizeIncrease()
    {
        _transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y*2, 0);
        SyncMeasure();
    }
    private void Debug_SizeDecrease()
    {
        _transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y/2, 0);
        SyncMeasure();
    }
    #endregion
}
