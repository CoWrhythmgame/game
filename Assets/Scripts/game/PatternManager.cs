using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PatternManager : MonoBehaviour
{
    [SerializeField] private NotePoolManager notePoolManager;
    [SerializeField] private float _scrollSpeed = 1f;
    [SerializeField] private float _noteOffset = 0f;
    private GameObject dataMaster;
    private GameObject judgementManager;
    private PlayOption _playOption;
    private Song _songData;
    [SerializeField]private int _difficultyIndex;
    private List<Note> _noteList;
    private Queue<Note>[] _noteQueue;
    private double _startAudioTime = 0d;
    private double _startInputTime = 0d;
    private double _tripTime = 0d;
    private float _songBPM = 1;
    void Start()
    {
        SetUp();

        Invoke("StartSong", 1f);
    }
    private void SetUp()
    {
        notePoolManager = transform.GetComponent<NotePoolManager>();
        dataMaster = GameObject.FindGameObjectWithTag("DataMaster");
        judgementManager = GameObject.FindGameObjectWithTag("JudgementManager");
        _noteQueue = new Queue<Note>[4];
        for (int i = 0; i < 4; i++)
        {
            _noteQueue[i] = new Queue<Note>();
        }
        GetDataFromMaster();

        // _scrolSpeed = _playOption.scrollSpeed;
        // _noteOffset = _playOption.noteOffset;
        // _songBPM = _songData.bpm;
        _tripTime = 2d/_scrollSpeed;
    }
    //datamaster에서 가져옴
    private void GetDataFromMaster()
    {
        _songData = dataMaster.GetComponent<DataMaster>().GetSong();
        _difficultyIndex = dataMaster.GetComponent<DataMaster>().GetDifficultyIndex();
        _playOption = dataMaster.GetComponent<DataMaster>().GetPlayOption();
    }
    private void StartSong()
    {
        _startAudioTime = AudioSettings.dspTime;
        _startInputTime = InputState.currentTime;
        
        ReadPattern();


        judgementManager.GetComponent<JudgementManager>().SetStartTime(_startAudioTime, _startInputTime);
        judgementManager.GetComponent<ScoreManager>().Initialize(_noteList.Count,_noteList.Where(c=>c.noteType == NoteType.hold).ToList().Count);
    }
    private void ReadPattern()
    {
        //이곳에 pattern.json 읽는 함수 호출
        // Pattern pattern = FileManager.LoadPattern(_songData, _difficultyIndex);
        _noteList = new List<Note>();

        // _noteList = pattern.notes;

        _noteList = FileManager.TestPatternLoad().notes;

        TestFillList(0, 9, NoteType.hold, 11);

        FillQueue();
    }

    private void FillQueue()
    {
        _noteList = _noteList.OrderBy(s => s.time).ToList();
        for(int i = 0; i < _noteList.Count; i++)
        {
            _noteQueue[_noteList[i].lane].Enqueue(_noteList[i]);
        }
    }
    public void CheckPatternEnd()
    {
        int notecount = 0;
        for(int i = 0; i < 4; i++)
        {
            notecount += _noteQueue[i].Count;
        }
        if(notecount == 0)
        {
            PlayData playdata = judgementManager.GetComponent<JudgementManager>().OnPatternEnd();
            Debug.Log(JsonUtility.ToJson(playdata, true));
            dataMaster.GetComponent<DataMaster>().SetPlayData(playdata);


            // ingame 작업중에는 song값을 불러오지 않으므로 비활성화
            // Record newrecord = UpdateRecord(playdata);
            // dataMaster.GetComponent<DataMaster>().SetRecord(newrecord, newrecord != _songData.record[_difficultyIndex]);
        }
    }

    private Record UpdateRecord(PlayData playData)
    {
        Record record = _songData.record[_difficultyIndex];
        if(playData.maxcombo > record.maxcombo)
        {
            record.maxcombo = playData.maxcombo;
        }
        if(playData.score > record.score)
        {
            record.score = playData.score;
        }
        if(playData.prate > record.prate)
        {
            record.prate = playData.prate;
        }
        if(record.score >= 1000000)
        {
            record.comboResult = ComboResult.allperfact;
        }
        else if(_songData.patternInfo[_difficultyIndex].totalNoteCount == record.maxcombo)
        {
            record.comboResult = ComboResult.fullcombo;
        }
        return record;
    }

    // Update is called once per frame
    void Update()
    {
        double currentTime = AudioSettings.dspTime-_startAudioTime;
        for(int i = 0; i < 4; i++)
        {
            if(_noteQueue[i].Count > 0){
                Note note = _noteQueue[i].Peek();
                
                if(currentTime >= note.time - _tripTime*note.bpm/_songBPM)
                {
                    notePoolManager.SpawnNote(note, _scrollSpeed, note.bpm/_songBPM);
                    _noteQueue[i].Dequeue();
                }
                //여기에 롱노트 관련 삽입
            }
        }
    }
    #region 테스트용
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
    #endregion
}
