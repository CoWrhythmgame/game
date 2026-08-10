using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class Measure : MonoBehaviour
{
    #region Variables
    [Header("Connect Area")]
    [SerializeField] private GameObject _notePrefab;
    [SerializeField] private GameObject _noteListObject;
    //// [SerializeField] private GameObject _ConstraintObject; 더이상 사용하지 않음.
    [SerializeField] private GameObject _shadeNote;
    [SerializeField] private GameObject _inspectorButton;
    [SerializeField] private TMPro.TMP_Text _text;
    [SerializeField] private Inspector _inspector;

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
    [SerializeField] private float _BPM = 120;
    [SerializeField] List<GameObject> _notes;

    [Header("MeasureData")]
    [SerializeField] private Color _originalColor;
    [SerializeField] private Color _noticeColor;
    [SerializeField] private bool _isHold = false;
    [SerializeField] private bool _isMouseOverNote = false;
    [SerializeField] private bool _isBpmAdjusted = false;
    [SerializeField] private float _holdMaxLangth = 0;
    [SerializeField] private int _MeasureIndex = 0;
    [Header("Debug")]
    // ! 툴바가 완성되었으므로 사용하지 않음
    //// [SerializeField] private bool Debug_Increase = false;
    //// [SerializeField] private bool Debug_Decrease = false;
    private Transform _transform;
    private Transform _shadenoteTransform;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _mousePos;
    private Vector3 _longNotePos;
    private Vector3 _notesize;
    //// private ConstraintSource _source;
    private MeasureList _measureList;
    private bool _isQuitting = false;
    #endregion

    #region LifeCycle
    private void Awake()
    {
        _transform = transform.GetComponent<Transform>();
        _spriteRenderer = transform.GetComponent<SpriteRenderer>();
        _shadenoteTransform = _shadeNote.transform;
        _notes = new List<GameObject>();
        _measureList = transform.parent.parent.GetComponent<MeasureList>();

        _inspector = GameObject.FindGameObjectWithTag("Inspector").GetComponent<Inspector>();

        _notesize = _notePrefab.GetComponent<SpriteRenderer>().bounds.size;

        _inspectorButton.GetComponent<ObjectButton>().OnButtonClicked += ShowInspector;

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
        _BPM = bpm;
        _isBpmAdjusted = false;
        _spriteRenderer.color = _originalColor;
        OnMeasureChanged();
    }
    private void OnDisable()
    {
        //게임 종료, 씬 전환시 바로 리턴
        if(_isQuitting || !gameObject.scene.isLoaded) return;
        if(_inspectorButton != null)_inspectorButton.SetActive(false);
    }
    private void OnEnable()
    {
        if(_inspectorButton != null)_inspectorButton.SetActive(true);
        OnMeasureChanged();
        OnToolbarChanged();
    }
    
    private void Update()
    {
        _mousePos = GetMouseWorldPosition();
        if (IsMouseOn() || _isHold)
        {
            OnOverMouse();
        }
        else
        {
            OnOffMouse();
        }
        // ! 툴바가 만들어졌으므로 사용하지 않음.
        // if (Debug_Increase)
        // {
        //     Debug_SizeIncrease();
        //     Debug_Increase=false;
        // }
        // if (Debug_Decrease)
        // {
        //     Debug_SizeDecrease();
        //     Debug_Decrease=false;
        // }
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }
    #endregion

    #region Callbacks
    public void OnIndexChanged(int index)
    {
        _MeasureIndex = index;
        _text.text = _MeasureIndex.ToString();
        OnMeasureChanged();
    }
    public void OnInspectorBpmChanged(float BPM)
    {
        _BPM = BPM;
        _isBpmAdjusted = true;
        _spriteRenderer.color = _noticeColor;
    }
    private void OnOverMouse()
    {
        if(!_isMouseOverNote)_shadeNote.SetActive(true);
        if(!_isHold) _shadenoteTransform.localPosition = GetMouseGridVector3(_mousePos);
        if (Mouse.current.leftButton.wasPressedThisFrame && _noteType == NoteType.single)
        {
            AddNote();
        }
        //롱노트 로직
        else if(Mouse.current.leftButton.wasPressedThisFrame && _noteType == NoteType.hold)
        {
            _longNotePos = _shadenoteTransform.localPosition;
            _isHold = true;
        }
        if (Mouse.current.leftButton.isPressed && _isHold == true)
        {
            SetShadenoteHoldSize();
        }
        else if(Mouse.current.leftButton.wasReleasedThisFrame && _isHold == true)
        {
            AddNote();
        }
    }
    private void OnOffMouse()
    {
        _shadeNote.SetActive(false);
    }
    private void OnMouseOverNote()
    {
        _shadeNote.SetActive(false);
        _isMouseOverNote = true;
    }
    private void OnMouseOffNote()
    {
        _shadeNote.SetActive(true);
        _isMouseOverNote = false;
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
    /// 렌더링되지 않은 마디선까지 모든 마디선이 정보를 갱신할 때 호출되는 함수
    /// </summary>
    public void OnEveryMeasrueChanged()
    {
        SyncWithSongBPM();
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
    public void ShowInspector()
    {
        _inspector.Show(this, _MeasureIndex,_BPM);
    }
    #endregion
    #region Note Logic
    /// <summary>
    /// 노트 추가 함수
    /// </summary>
    private void AddNote()
    {
        Vector3 notePos = GetMouseGridVector3(_mousePos);
        if(_isMouseOverNote) return;
        if(_notes.Any(note => note.transform.localPosition == notePos))return;


        GameObject note = Instantiate(_notePrefab, _noteListObject.transform);
        //// note.GetComponent<ScaleConstraint>().AddSource(_source);
        note.transform.localPosition = notePos;
        if(_noteType == NoteType.single){
            note.transform.localScale = new Vector3(0.25f,1/_transform.localScale.y,1);
            note.GetComponent<SpriteRenderer>().sortingLayerName = "Note_Single";
            note.GetComponent<EditorNote>().Initialize((int)((notePos.x+0.375f)/0.25f), _signature, (int)Mathf.Round(note.transform.localPosition.y*_signature), _noteType);
        }
        else if(_noteType == NoteType.hold)
        {
            float length = Mathf.Clamp(GetGridY(_mousePos.y-_shadenoteTransform.position.y)*4f, 0.25f, float.MaxValue)/_transform.localScale.y;
            Debug.Log(length);
            note.transform.localScale = new Vector3(0.25f, Mathf.Clamp(GetGridY(_mousePos.y-_shadenoteTransform.position.y)*4f, 0.25f, float.MaxValue), 1f);
            note.transform.localPosition = _longNotePos;

            if(_holdMaxLangth < length)
            {
                _holdMaxLangth = length;
            }
            
            note.GetComponent<SpriteRenderer>().sortingLayerName = "Note_Hold";
            note.GetComponent<EditorNote>().Initialize((int)((notePos.x+0.375f)/0.25f), _signature, (int)Mathf.Round(note.transform.localPosition.y*_signature), _noteType, length);
            _isHold = false;
            _shadenoteTransform.localScale = new Vector3(0.25f,1/_transform.localScale.y,1);
        }

        note.GetComponent<EditorNote>().OnNoteDelete += DeleteNote;
        note.GetComponent<EditorNote>().OnMouseOverNote += OnMouseOverNote;
        note.GetComponent<EditorNote>().OnMouseOffNote += OnMouseOffNote;
        _notes.Add(note);
    }
    /// <summary>
    /// 마우스 위치의 노트 삭제
    /// </summary>
    private void DeleteNote(GameObject note)
    {
        _notes.Remove(note);
        Destroy(note);
    }
    /// <summary>
    /// 롱노트를 놓을 때 놓일 노트의 길이를 미리보기 위해 shadenote의 길이를 늘리는 함수
    /// </summary>
    private void SetShadenoteHoldSize()
    {
        _shadenoteTransform.localScale = new Vector3(0.25f, Mathf.Clamp(GetGridY(_mousePos.y-_shadenoteTransform.position.y)*4f, 0.25f, float.MaxValue), 1f);
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
            // * 롱노트는 마디 크기 비율 그대로 따라감
            
        }
        _shadenoteTransform.localScale = new Vector3(0.25f,1/_transform.localScale.y,1);
    }
    #endregion
    #region Measure Logic
    /// <summary>
    /// 마디선 BPM을 노래 BPM에 맞춤
    /// </summary>
    /// <param name="ignoreAdjusted">마디의 bpm이 변경되었음을 무시하고 초기화</param>
    public void SyncWithSongBPM(bool ignoreAdjusted = false)
    {
        if(ignoreAdjusted) {
            _isBpmAdjusted = false;
            _spriteRenderer.color = _originalColor;
            }


        if(!_isBpmAdjusted) _BPM = _measureList.GetBpm();
    }
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
    
    #endregion
    #region MouseLogic
    
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
    /// <summary>
    /// 벡터를 가져와서 마디선 그리드에 맞는(snap) 벡터를 반환
    /// </summary>
    /// <param name="mousePos">마우스 위치 벡터</param>
    /// <returns>그리드에 스냅된 벡터</returns>
    private Vector3 GetMouseGridVector3(Vector3 mousePos)
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
    /// 직접 y값을 그리드에 맞추는 함수
    /// </summary>
    /// <param name="posY">y값(마디선에 맞추지 않으므로 직접 벡터 시작점 잡아줄 필요 있음.)</param>
    /// <returns>마디선 기준으로 그리드에 대응되는 Y값</returns>
    private float GetGridY(float posY)
    {
        float retY;

        //offgrid
        if(_signature == 1) {
            retY = posY/_transform.localScale.y;
        }
        else{
            retY = Mathf.Floor(posY*_signature/_transform.localScale.y)/_signature;
        }


        return retY;
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
    #region getter/setter
    /// <summary>
    /// 렌더링을 위하여 롱노트 최대 길이를 반환하는 함수
    /// </summary>
    /// <returns>롱노트 최대 길이</returns>
    public float GetHoldMaxLength()
    {
        return _holdMaxLangth;
    }
    /// <summary>
    /// 롱노트를 놓는 중인지 반환하는 함수
    /// </summary>
    /// <returns>롱노트 놓는지 여부</returns>
    public bool GetIsHold()
    {
        return _isHold;
    }
    public int GetIndex(){
        return _MeasureIndex;
    }
    public double Getbpm()
    {
        return _BPM;
    }
    public List<Note> GetNotes()
    {
        List<Note> notes = new List<Note>();
        foreach(GameObject note in _notes)
        {
            Note notedata = note.GetComponent<EditorNote>().GetNoteData();
            notedata.time = notedata.time/_BPM*60;
            notedata.releaseTime = notedata.releaseTime/_BPM*60*4+notedata.time;
            notedata.bpm = _BPM;
            notes.Add(notedata);
        }
        return notes;
    }
    #endregion
    #region Debug
    // ! 툴바 완성으로 인해 사용하지 않음
    // private void Debug_SizeIncrease()
    // {
    //     _transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y*2, 0);
    //     OnMeasureChanged();
    // }
    // private void Debug_SizeDecrease()
    // {
    //     _transform.localScale = new Vector3(_transform.localScale.x, _transform.localScale.y/2, 0);
    //     OnMeasureChanged();
    // }
    #endregion
}
