using UnityEngine;

public class EditorNote : MonoBehaviour
{
    [Header("NoteData")]
    [SerializeField]private int _laneIndex = 0;
    [SerializeField]private int _signature = 1;
    [SerializeField]private int _currentBeat = 1;
    [SerializeField]private NoteType _noteType = NoteType.single;
    [SerializeField]private float _longNoteLength = 1f;
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
    public NoteType GetNoteType()
    {
        return _noteType;
    }

}
