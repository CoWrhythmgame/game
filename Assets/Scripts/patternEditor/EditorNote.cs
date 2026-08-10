using System;
using UnityEngine;

public class EditorNote : MonoBehaviour
{
    [Header("NoteData")]
    [SerializeField]private int _laneIndex = 0;
    [SerializeField]private int _signature = 1;
    [SerializeField]private int _currentBeat = 1;
    [SerializeField]private NoteType _noteType = NoteType.single;
    [SerializeField]private float _longNoteLength = 1f;
    public Action<GameObject> OnNoteDelete;
    public Action OnMouseOverNote;
    public Action OnMouseOffNote;
    #region LifeCycle
    public void Initialize(int laneIndex, int signature, int currentBeat, NoteType noteType)
    {
        _laneIndex = laneIndex;
        _signature = signature;
        _currentBeat = currentBeat;
        _noteType = noteType;
    }
    public void Initialize(int laneIndex, int signature, int currentBeat, NoteType noteType, float longNoteLength)
    {
        _laneIndex = laneIndex;
        _signature = signature;
        _currentBeat = currentBeat;
        _noteType = noteType;
        _longNoteLength = longNoteLength;
    }
    public void DeleteNote()
    {
        OnMouseOffNote.Invoke();
        OnNoteDelete.Invoke(gameObject);
    }
    #endregion
    #region Callback
    public void OnOverMouse()
    {
        Color color = new Color(0.5f,1f,0.5f,0.75f);
        transform.GetComponent<SpriteRenderer>().color = color;
        OnMouseOverNote.Invoke();
    }
    public void OnOffMouse()
    {
        Color color = new Color(1,1,1,1);
        transform.GetComponent<SpriteRenderer>().color = color;
        OnMouseOffNote();
    }
    #endregion
    public NoteType GetNoteType()
    {
        return _noteType;
    }
    public Note GetNoteData()
    {
        Note note = new Note
        {
            lane = _laneIndex,
            //time = (double)_currentBeat/ _signature * 4,
            time = transform.localPosition.y * 4.0,
            noteType = _noteType,
        };
        if(_noteType == NoteType.single)
        {
            note.releaseTime = 0;
        }else if(_noteType == NoteType.hold)
        {
            note.releaseTime = _longNoteLength;
        }
        return note;
    }

}
