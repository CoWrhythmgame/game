using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PatternManager : MonoBehaviour
{
    [SerializeField] private NotePoolManager notePoolManager;
    [SerializeField] private float _scrollSpeed = 1f;
    private PlayOption _playOption;
    private Song _songData;
    [SerializeField]private int _difficultyIndex;
    private List<Note> _noteList;
    private Queue<Note>[] _noteQueue;
    private double _startAudioTime = 0d;
    private double _startInputTime = 0d;
    private double _tripTime = 0d;
    void Start()
    {
        notePoolManager = transform.GetComponent<NotePoolManager>();
        _noteQueue = new Queue<Note>[4];
        for (int i = 0; i < 4; i++)
        {
            _noteQueue[i] = new Queue<Note>();
        }
        
        _songData = GameObject.FindGameObjectWithTag("DataMaster").GetComponent<DataMaster>().GetSong();
        _difficultyIndex = GameObject.FindGameObjectWithTag("DataMaster").GetComponent<DataMaster>().GetDifficultyIndex();
        _playOption = GameObject.FindGameObjectWithTag("DataMaster").GetComponent<DataMaster>().GetPlayOption();

        // _scrolSpeed = _playOption.scrollSpeed;
        _tripTime = 2d/_scrollSpeed;
        Invoke("StartSong", 1f);
    }
    private void StartSong()
    {
        _startAudioTime = AudioSettings.dspTime;
        _startInputTime = InputState.currentTime;
        

        GameObject.FindGameObjectWithTag("JudgementManager").GetComponent<JudgementManager>().SetStartTime(_startAudioTime, _startInputTime);

        ReadPattern();
    }
    private void ReadPattern()
    {
        //이곳에 pattern.json 읽는 함수 호출
        _noteList = new List<Note>();
        TestFillList(0, 5);
        TestFillList(1, 5);
        TestFillList(2, 5);
        
        TestFillList(3, 7);
        TestFillList(2, 7);
        TestFillList(1, 7);

        FillQueue();
    }

    private void TestFillList(int lane, double time, NoteType noteType = NoteType.single, double releaseTime = 0)
    {
        _noteList.Add(new Note()
        {
            lane = lane,
            time = time,
            noteType = noteType,
            releaseTime = releaseTime
        });
    }
    private void FillQueue()
    {
        _noteList = _noteList.OrderBy(s => s.time).ToList();
        for(int i = 0; i < _noteList.Count; i++)
        {
            _noteQueue[_noteList[i].lane].Enqueue(_noteList[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        double currentTime = AudioSettings.dspTime-_startAudioTime;
        for(int i = 0; i < 4; i++)
        {
            if(_noteQueue[i].Count > 0){
                Note temp = _noteQueue[i].Peek();
                if(currentTime >= temp.time - _tripTime)
                {
                    notePoolManager.SpawnNote(temp.lane, temp.time, _scrollSpeed);
                    _noteQueue[i].Dequeue();
                }
                //여기에 롱노트 관련 삽입
            }
        }
    }
}
