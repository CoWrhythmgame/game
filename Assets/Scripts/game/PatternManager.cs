using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

public class PatternManager : MonoBehaviour
{
    [SerializeField] private NotePoolManager notePoolManager;
    [SerializeField] private InfoPannel _infoPannel;
    [SerializeField] private float _scrollSpeed = 1f;
    private DataMaster dataMaster;
    private AudioSource _audioSource;
    private AudioClip _music;
    private GameObject judgementManager;
    private PlayOption _playOption;
    private Song _songData;
    [SerializeField]private int _difficultyIndex;
    private List<Note> _noteList;
    private Queue<Note>[] _noteQueue;
    private double _startAudioTime = 0d;
    private double _startInputTime = 0d;
    private double _tripTime = 0d;
    private float _noteoffset = 0f;
    private float _songBPM = 1;
    private bool _isLoaded=false;
    private bool _isSongBuiltin = false;
    void Start()
    {
        SetUp();

        Invoke("StartSong", 1f);
    }
    private void SetUp()
    {
        _isLoaded = false;
        notePoolManager = GetComponent<NotePoolManager>();
        _audioSource = GetComponent<AudioSource>();
        dataMaster = GameObject.FindGameObjectWithTag("DataMaster").GetComponent<DataMaster>();
        judgementManager = GameObject.FindGameObjectWithTag("JudgementManager");
        _noteQueue = new Queue<Note>[4];


        for (int i = 0; i < 4; i++)
        {
            _noteQueue[i] = new Queue<Note>();
        }
        GetDataFromMaster();

        _infoPannel.SetSongInfo(_songData);

        _scrollSpeed = _playOption.scrollSpeed;
        _noteoffset = _playOption.noteOffset;
        _songBPM = _songData.bpm;
        _tripTime = 2d/_scrollSpeed;
        _audioSource.clip = _music;
    }
    //datamaster에서 가져옴
    private void GetDataFromMaster()
    {
        _songData = dataMaster.GetSong();
        _difficultyIndex = dataMaster.GetDifficultyIndex();
        _playOption = dataMaster.GetPlayOption();
        _music = dataMaster.GetMusic();
        _isSongBuiltin = dataMaster.GetIsBuiltin();
    }
    private void StartSong()
    {
        _isLoaded = true;
        _startAudioTime = AudioSettings.dspTime;
        _startInputTime = InputState.currentTime;
        
        ReadPattern();

        transform.GetComponent<NotePosManager>().SetStartTime(_startAudioTime);
        judgementManager.GetComponent<JudgementManager>().SetStartTime(_startAudioTime, _startInputTime);
        judgementManager.GetComponent<ScoreManager>().Initialize(_noteList.Count,_noteList.Where(c=>c.noteType == NoteType.hold).ToList().Count);
    }
    private void ReadPattern()
    {
        // * 이곳에 pattern.json 읽는 함수 호출
        Pattern pattern = FileManager.LoadPattern(_songData, _difficultyIndex, _isSongBuiltin);
        _noteList = new List<Note>();

        _noteList = pattern.notes;

        // // 테스트용임. datamaster에서 값 가져오면 이거 주석처리할것.
        // _noteList = FileManager.TestPatternLoad().notes;

        // TestFillList(0, 9, NoteType.hold, 11);

        FillQueue();
    }

    private void FillQueue()
    {
        _noteList = _noteList.OrderBy(s => s.time).ToList();
        for(int i = 0; i < _noteList.Count; i++)
        {
            _noteList[i].time += 2;
            _noteList[i].releaseTime += 2;

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
            if(playdata == null) return;
            Debug.Log(JsonUtility.ToJson(playdata, true));
            dataMaster.SetPlayData(playdata);


            // ingame 작업중에는 song값을 불러오지 않으므로 비활성화
            Record newrecord = UpdateRecord(playdata);
            dataMaster.SetRecord(newrecord, newrecord != _songData.record[_difficultyIndex]);
            // HACK: 나중에 조건 확인하기
            // if(마무리 조건(노래가 끝났을 때 같은거))
            Invoke("OnChangeScene",2f);
        }
    }

    private void OnChangeScene()
    {
        if (dataMaster.GetIsTestPlay())
        {
            dataMaster.SetIsTestPlay(false);
            SceneManager.LoadScene("EditorScene");
        }
        else{
            SceneManager.LoadScene("ResultScene");
        }
    }
    private void OnEnable() // 이벤트 받는 부분
    {
        PauseManager.OnGamePaused += PauseMusic;
        PauseManager.OnGameResumed += ResumeMusic;
    }
    private void OnDisable()
    {
        PauseManager.OnGamePaused -= PauseMusic;
        PauseManager.OnGameResumed -= ResumeMusic;
    }
    private void PauseMusic()
    {
        if (_audioSource != null && _audioSource.isPlaying)
            _audioSource.Pause();
    }
    private void ResumeMusic()
    {
        if (_audioSource != null)
            _audioSource.UnPause();
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
        if (PauseManager.IsPaused)
            return;
        double currentTime = AudioSettings.dspTime- PauseManager.TotalPausedDspTime - _startAudioTime;
        if(_isLoaded)
        {
            if(currentTime >= 2 && !_audioSource.isPlaying)
            {
                Debug.Log("current time: " + currentTime);
                _audioSource.Play();
            }
        }
        for(int i = 0; i < 4; i++)
        {
            if(_noteQueue[i].Count > 0){
                Note note = _noteQueue[i].Peek();
                if(currentTime >= note.time - _tripTime*note.bpm/_songBPM/Mathf.Clamp(_scrollSpeed, float.MinValue, 1f)/Mathf.Clamp(NotePosManager._bpmFactor, float.MinValue, 1f)+Mathf.Clamp(_noteoffset, float.MinValue, 0))
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
