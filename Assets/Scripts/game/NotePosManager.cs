using System.Collections.Generic;
using UnityEngine;

public class NotePosManager : MonoBehaviour
{
    [SerializeField]private List<Transform> _activeNoteList = new List<Transform>();
    private Dictionary<Transform, NoteObject> _noteObjDict = new Dictionary<Transform, NoteObject>();
    private float _traveldistance;
    private float _judgeY;
    private double _startTime;

    private void Update()
    {
        double currentTime = AudioSettings.dspTime - _startTime;
        //HACK: 총 정지 시간을 확인할 필요가 있음

        for(int i = _activeNoteList.Count - 1; i >= 0; i--)
        {
            if (_noteObjDict.TryGetValue(_activeNoteList[i], out NoteObject noteObject))
            {
                double timeToHit = noteObject.GetTargetTime() - currentTime;
                if (noteObject.GetIsLong() && noteObject.GetIsHolding())
                {
                    
                    _activeNoteList[i].localScale = new Vector3(1, Mathf.Clamp((float)(noteObject.GetReleaseTime() - currentTime) * noteObject.GetNoteSpeed() * 32 / _traveldistance, 0, float.MaxValue), 1);
                }
                else
                {
                    float newY = (float)(timeToHit * noteObject.GetNoteSpeed());
                    Vector3 newPosition = new Vector3(_activeNoteList[i].position.x, newY + _judgeY, _activeNoteList[i].position.z);
                    _activeNoteList[i].position = newPosition;
                }
            }
        }
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
