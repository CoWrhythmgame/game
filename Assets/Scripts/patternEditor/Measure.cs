using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class Measure : MonoBehaviour
{
    #region Variables
    [Header("Objects")]
    [SerializeField] private GameObject _notePrefab;
    [SerializeField] private GameObject _noteListObject;
    //// [SerializeField] private GameObject _ConstraintObject; 더이상 사용하지 않음.
    [SerializeField] private GameObject _shadeNote;
    [SerializeField] private TMPro.TMP_Text _text;

    [Header("Lines")]
    [SerializeField] private List<GameObject> _Lines_4;
    [SerializeField] private List<GameObject> _Lines_6;
    [SerializeField] private List<GameObject> _Lines_8;
    [SerializeField] private List<GameObject> _Lines_12;
    [SerializeField] private List<GameObject> _Lines_16;

    [Header("FromToolbar")]
    [SerializeField] private int _signature = 4;
    [SerializeField] private NoteType _noteType = NoteType.single;

    [Header("SongData")]
    [SerializeField] private float _bpm = 120;
    [SerializeField] List<GameObject> _notes;

    [Header("Debug")]
    [SerializeField] private bool Debug_Increase = false;
    [SerializeField] private bool Debug_Decrease = false;
    [SerializeField] private int _MeasureIndex = 0;
    private Transform _transform;
    private Vector3 _mousePos;
    private ConstraintSource _source;
    private MeasureList _measureList;
    #endregion

    #region LifeCycle
    private void Awake()
    {
        _transform = transform.GetComponent<Transform>();
        _notes = new List<GameObject>();
        _measureList = transform.parent.parent.GetComponent<MeasureList>();

        // ! constraint 제거 더이상 사용하지 않음.
        //// _source = new ConstraintSource
        //// {
        ////     sourceTransform = _ConstraintObject.transform,
        ////     weight = 1
        //// };

    }
    public void Initialize(int index, float bpm)
    {
        _MeasureIndex = index;
        _text.text = _MeasureIndex.ToString();
        _bpm = bpm;
        OnMeasureChanged();
    }
    private void OnEnable()
    {
        OnMeasureChanged();
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
        // //HACK: 디버그용. 툴바가 만들어지면 반드시 제거할것
        // SyncWithToolbar();
    }
    #endregion

    #region Callbacks
    public void OnIndexChanged(int index)
    {
        _MeasureIndex = index;
        _text.text = _MeasureIndex.ToString();
        OnMeasureChanged();
    }
    private void OnOverMouse()
    {
        _shadeNote.SetActive(true);
        _shadeNote.transform.localPosition = GetGridVector3(_mousePos);
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            AddNote();
        }
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            DeleteNote();
        }
    }
    private void OnOffMouse()
    {
        _shadeNote.SetActive(false);
    }
    /// <summary>
    /// 확대, 축소, 삭제시 호출되는 함수(최적화를 위해 활성화되었을때 호출)
    /// </summary>
    public void OnMeasureChanged()
    {
        MeasureRePosition();
        NoteReSize();
    }
    /// <summary>
    /// MeasureList에서 저장하고 있는 툴바 정보를 가져옴
    /// </summary>
    public void OnToolbarChanged()
    {
        _signature = _measureList.GetSignature();
        _noteType = _measureList.GetNoteType();
        RenderLines(_signature);
    }
    #endregion
    #region Note Logic
    /// <summary>
    /// 노트 추가 함수
    /// </summary>
    private void AddNote()
    {
        Vector3 notePos = GetGridVector3(_mousePos);
        if(_notes.Any(note => note.transform.localPosition == notePos))return;

        GameObject note = Instantiate(_notePrefab, _noteListObject.transform);
        // note.GetComponent<ScaleConstraint>().AddSource(_source);
        note.transform.localPosition = notePos;
        note.transform.localScale = new Vector3(0.25f,1/_transform.localScale.y,1);

        note.GetComponent<EditorNote>().Initialize((int)((notePos.x+0.375f)/0.25f), _signature, (int)Mathf.Round(note.transform.localPosition.y*_signature), _noteType);
        
        _notes.Add(note);
    }
    /// <summary>
    /// 마우스 위치의 노트 삭제
    /// </summary>
    private void DeleteNote()
    {
        Debug.Log("delete");
        Vector3 notePos = GetGridVector3(_mousePos);
        if(!_notes.Any(note => note.transform.localPosition == notePos))return;
        GameObject note = _notes.FirstOrDefault(note => note.transform.localPosition == notePos);
        Debug.Log(note);
        _notes.Remove(note);
        Destroy(note);
    }
    /// <summary>
    /// 확대, 축소시 모든 노트 크기 조절
    /// </summary>
    private void NoteReSize()
    {
        foreach(GameObject note in _notes)
        {
            NoteType noteType = note.GetComponent<EditorNote>().GetNoteType();
            if(noteType == NoteType.single)
            {
                note.transform.localScale = new Vector3(0.25f,1/_transform.localScale.y,1);
            }
            //TODO: 여기에 롱노트 로직 넣어야함.
            
        }
    }
    #endregion
    #region Measure Logic
    /// <summary>
    /// 비트 값을 받아서 그 약수에 맞는 비트 가이드라인을 보여줌. 가이드라인은 필요시 더 추가할것.
    /// </summary>
    /// <param name="signature">현제 설정된 비트</param>
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
        foreach(GameObject line in _Lines_12)
        {
            line.SetActive(false);
        }
        foreach(GameObject line in _Lines_16)
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
        if(signature % 12 == 0)
        {
            foreach(GameObject line in _Lines_12)
            {
                line.SetActive(true);
            }
        }
        if(signature % 16 == 0)
        {
            foreach(GameObject line in _Lines_16)
            {
                line.SetActive(true);
            }
        }
    }
    /// <summary>
    /// 마디선 이동
    /// </summary>
    private void MeasureRePosition()
    {
        _transform.position = new Vector3(0,_MeasureIndex*_transform.localScale.y-4f,0);
    }
    /// <summary>
    /// 마우스 위치를 반환
    /// </summary>
    /// <returns>마우스 위치</returns>
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
    #endregion
    #region MouseLogic
    
    /// <summary>
    /// 벡터를 가져와서 마디선 그리드에 맞는(snap) 벡터를 반환
    /// </summary>
    /// <param name="mousePos">마우스 위치 벡터</param>
    /// <returns>그리드에 스냅된 벡터</returns>
    private Vector3 GetGridVector3(Vector3 mousePos)
    {
        float x = (MathF.Floor(mousePos.x)+1f/2)/_transform.localScale.x;
        float y;

        //offgrid
        if(_signature == 1) {
            y = (mousePos.y-_transform.position.y)/_transform.localScale.y;
        }
        else{
            y = Mathf.Floor((mousePos.y-_transform.position.y)*_signature/_transform.localScale.y)/_signature;
        }



        return new Vector3(x,y,0);
    }
    /// <summary>
    /// 마우스가 이 오브젝트 위에 있는지 여부를 반환
    /// </summary>
    /// <returns>오브젝트 위 마우스 여부</returns>
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
    #endregion
    #region Debug
    private void Debug_SizeIncrease()
    {
        _transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y*2, 0);
        OnMeasureChanged();
    }
    private void Debug_SizeDecrease()
    {
        _transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y/2, 0);
        OnMeasureChanged();
    }
    #endregion
}
