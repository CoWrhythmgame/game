using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NotePosManager : MonoBehaviour
{
    public static float _bpmFactor = 1.0f;
    [SerializeField]private List<Transform> _activeNoteList = new List<Transform>();
    [SerializeField]private bool _direct_Geo = false;
    [SerializeField]private float _factorViewer = 1f;
    private Dictionary<Transform, NoteObject> _noteObjDict = new Dictionary<Transform, NoteObject>();
    private float _traveldistance;
    private float _judgeY;
    private float _directFactor = 1f;
    private float _count = 0;
    private double _startTime;


    void Awake()
    {
        _bpmFactor = 1.0f;
        _directFactor = 1f;
    }
    private void Update()
    {
        _factorViewer = _bpmFactor;
        double currentTime = AudioSettings.dspTime -PauseManager.TotalPausedDspTime - _startTime;
        //HACK: 총 정지 시간을 확인할 필요가 있음

        for(int i = _activeNoteList.Count - 1; i >= 0; i--)
        {
            if (_noteObjDict.TryGetValue(_activeNoteList[i], out NoteObject noteObject))
            {
                double timeToHit = noteObject.GetTargetTime() - currentTime;
                if (noteObject.GetIsLong() && noteObject.GetIsHolding())
                {
                    
                    _activeNoteList[i].localScale = new Vector3(1, Mathf.Clamp((float)(noteObject.GetReleaseTime() - currentTime) * noteObject.GetNoteSpeed() * 36 / _traveldistance*_bpmFactor, 0, float.MaxValue), 1);
                }
                else
                {
                    float newY = (float)(timeToHit * noteObject.GetNoteSpeed())*_bpmFactor;
                    Vector3 newPosition = new Vector3(_activeNoteList[i].position.x, newY + _judgeY, _activeNoteList[i].position.z);
                    _activeNoteList[i].position = newPosition;
                }
            }
        }
        if(_direct_Geo ||Keyboard.current.bKey.wasPressedThisFrame)
        {
            _directFactor = 0.001f;
            _count=1;
            _direct_Geo = false;
        }
        if(_directFactor < 1f)
        {
            _directFactor += (_count/1000)*(_count/1000);
            _count++;
        }else if(_directFactor > 1f)
        {
            _directFactor = 1f;
        }
        _bpmFactor = _directFactor;
    }

    public void AddNote(Transform note)
    {
        _activeNoteList.Add(note);
    }
    public void RemoveNote(Transform note)
    {
        _activeNoteList.Remove(note);
    }
    public void RegisterNoteObject(Transform note, NoteObject noteObject)
    {
        _noteObjDict[note] = noteObject;
    } 
    public void SetTravelDistance(float distance)
    {
        _traveldistance = distance;
    }
    public void SetStartTime(double startTime)
    {
        _startTime = startTime;
    }
    public void SetJudgeY(float judgeY)
    {
        _judgeY = judgeY;
    }
}
