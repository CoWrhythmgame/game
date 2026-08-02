using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeasureList : MonoBehaviour
{
    #region Variables
    [Header("Connect Area")]
    [SerializeField] private GameObject _measurePrefab;
    [SerializeField] private GameObject _addButton;
    [SerializeField] private GameObject _camera;
    [SerializeField] private Inspector _inspector;

    [Header("MeasureData")]
    [SerializeField] private int _scale = 1;
    [SerializeField] private int _measureIndex = 0;
    [SerializeField] private int _signature = 4;
    [SerializeField] private NoteType _noteType = NoteType.single;
    [SerializeField] private List<GameObject> _currentMeasures = new List<GameObject>();

    [Header("CameraData")]
    [SerializeField] private int _currentMeasureIndex = 0;
    [SerializeField] private float _cameraMaxLength = 0;

    [Header("SongData")]
    [SerializeField] private float _bpm = 120;
    

    [Header("Debug")]
    [SerializeField] private bool Debug_Pattern = false;
    // [SerializeField] private bool Debug_Increase = false;
    // [SerializeField] private bool Debug_Decrease = false;
    // [SerializeField] private bool Debug_Remove = false;

    private List<GameObject> _measures;
    private BeatmapTimer _beatMapTimer;
    #endregion

    #region LifeCycle
    void Awake()
    {
        _measures = new List<GameObject>();
        _camera = Camera.main.gameObject;
        
        _beatMapTimer = transform.GetComponent<BeatmapTimer>(); 

        _addButton.GetComponent<ObjectButton>().OnButtonClicked += AddMeasure;
        _inspector.OnDeleteButtonClicked += DeleteMeasure;

        AddMeasure();
    }
    void Update()
    {
        
        float scrollData = Mouse.current.scroll.ReadValue().y;
        if(scrollData != 0)
        {
            OnScroll(scrollData);
        }
        //! 툴바가 완성되었으므로 사용하지 않음
        // if (Debug_Increase)
        // {
        //     ScaleIncrease();
        //     Debug_Increase = false;
        // }
        // if (Debug_Decrease)
        // {
        //     ScaleDecrease();
        //     Debug_Decrease = false;
        // }
        // if (Debug_Remove)
        // {
        //     DeleteMeasure(1);
        //     Debug_Remove = false;
        // }
        if (Debug_Pattern)
        {
            GetPattern();
            Debug_Pattern = false;
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
        _currentMeasures.Add(measure);

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
        if(_measureIndex-1 == _currentMeasureIndex) _currentMeasureIndex--;
        _currentMeasures.Remove(_measures[index]);
        Destroy(_measures[index].transform.parent.gameObject);
        _measures.RemoveAt(index);
        for(int i = index; i < _measures.Count; i++)
        {
            _measures[i].GetComponent<Measure>().OnIndexChanged(i);
        }
        _measureIndex--;
        if(Mathf.Abs(_currentMeasureIndex-index) <= 2) {
            RenderMeasure();
        }
        OnMeasureChanged();
    }
    /// <summary>
    /// 렌더된 measure만 OnMeasureChanged() 호출
    /// </summary>
    private void CurrentChange()
    {
        foreach(GameObject measure in _currentMeasures)
        {
            measure.GetComponent<Measure>().OnMeasureChanged();
        }
    }
    /// <summary>
    /// 렌더된 measure만 OnToolbarChanged() 호출
    /// </summary>
    private void CurrentSync()
    {
        foreach(GameObject measure in _currentMeasures)
        {
            measure.GetComponent<Measure>().OnToolbarChanged();
        }
    }
    /// <summary>
    /// 현제 카메라 메인 마디선 주위 상하 2개의 마디선만 active함
    /// </summary>
    private void RenderMeasure()
    {
        _currentMeasureIndex = (int)Mathf.Clamp(MathF.Floor((_camera.transform.position.y+4f)/(4f*_scale)),0,_measureIndex-1);
        _currentMeasures.Clear();
        for(int i = 0; i < _measures.Count; i++)
        {
            //롱노트가 보이는 위치에 있으면 해당 롱노트를 가진 마디도 렌더링 해줘야함.
            //롱노트를 놓는 중이면 보여야함.
            if(_currentMeasureIndex > i){
                if(Mathf.Abs(_currentMeasureIndex-i)>2)
                {
                    if(i + _measures[i].GetComponent<Measure>().GetHoldMaxLength() - (_currentMeasureIndex - 2) > 0 || _measures[i].GetComponent<Measure>().GetIsHold())
                    {
                        _currentMeasures.Add(_measures[i]);
                    }
                }
            }
            _measures[i].SetActive(false);

        }
        for(int i = -2; i < 3; i++)
        {
            _currentMeasures.Add(_measures[Mathf.Clamp(_currentMeasureIndex+i,0,_measureIndex-1)]);
        }
        foreach(GameObject measure in _currentMeasures)
        {
            measure.SetActive(true);
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
        _camera.transform.position = new Vector3(0,4*_scale*_currentMeasureIndex-0.01f, _camera.transform.position.z);
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
    public Pattern GetPattern()
    {
        _beatMapTimer.SetTimingPoints(_measures);
        Pattern pattern = new Pattern
        {
            notes = new List<Note>()
        };
        foreach (GameObject measure in _measures)
        {
            List<Note> notes = measure.GetComponent<Measure>().GetNotes();
            double startTime = _beatMapTimer.GetMeasureTime(measure.GetComponent<Measure>().GetIndex());
            foreach(Note note in notes)
            {
                note.time += startTime;
                note.releaseTime += startTime;
            }
            pattern.notes = pattern.notes.Concat(notes).ToList();
        }
        Debug.Log(JsonUtility.ToJson(pattern));
        return pattern;
    }
    #endregion
}
