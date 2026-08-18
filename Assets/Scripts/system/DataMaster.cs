using UnityEngine;

public class DataMaster : MonoBehaviour
{
    // 어디서나 접근할 수 있는 전역 인스턴스
    public static DataMaster Instance { get; private set; }
    private PlayData _playData = new PlayData();
    private PlayOption _playOption = new PlayOption();
    private Song _song = new Song();
    private AudioClip _music;
    private bool _isNewRecord = false;
    private bool _isTestPlay = false;
    private bool _isBuiltin = false;
    private int _difficultyIndex = 0;
    void Awake()
    {
        // 1. 이미 인스턴스가 존재하는데, 그게 내가 아니라면? (중복 생성된 경우)
        if (Instance != null && Instance != this)
        {
            // 나 자신을 파괴하여 중복 생성을 막음
            Destroy(gameObject);
            return;
        }

        // 2. 내가 최초로 생성된 매니저라면 인스턴스로 등록
        Instance = this;

        // 3. 씬이 바뀌어도 파괴되지 않도록 설정 (최상위 오브젝트여야 작동함)
        DontDestroyOnLoad(gameObject);
    }
    #region SongSelectScene
    //옵션이 바뀌었을 때 함수 호출
    public void SetOption(PlayOption option)
    {
        _playOption = option;

    }
    //게임 시작 시(곡 선택시) 호출
    public void SetSongData(Song song, int difficultyIndex)
    {
        _song = song;
        _difficultyIndex = difficultyIndex;
    }
    public void SetIsBuiltin(bool isBuiltin)
    {
        _isBuiltin = isBuiltin;
    }
    public void SetMusic(AudioClip music)
    {
        _music = music;
    }
    #endregion
    #region InGameScene
    //인게임 씬을 로드할 때 호출
    public void OnLoadIngameScene()
    {
        _isNewRecord = false;
    }

    //곡 정보를 불러올 때 호출
    public Song GetSong()
    {
        return _song;
    }
    //어떤 패턴(easy, normal...)인지 확인용
    public int GetDifficultyIndex()
    {
        return _difficultyIndex;
    }
    public PlayOption GetPlayOption()
    {
        return _playOption;
    }
    public AudioClip GetMusic()
    {
        return _music;
    }
    public bool GetIsBuiltin()
    {
        return _isBuiltin;
    }
    //게임 끝나고 플레이 데이터 저장할때 호출
    public void SetPlayData(PlayData playData)
    {
        this._playData = playData;
    }
    public void SetRecord(Record record, bool isNewRecord = false)
    {
        _song.record[_difficultyIndex] = record;
        this._isNewRecord = isNewRecord;
    }
    public bool GetIsTestPlay()
    {
        return _isTestPlay;
    }
    #endregion
    #region ResultScene
    //결과창에서 표기할 데이터 전달할 때 호출
    public PlayData GetPlayData()
    {
        return _playData;
    }
    public Record GetRecord()
    {
        return _song.record[_difficultyIndex];
    }
    public bool GetIsNewRecord()
    {
        return _isNewRecord;
    }
    #endregion
    #region EditorScene
    public void SetIsTestPlay(bool istest)
    {
        _isTestPlay = istest;
    }
    #endregion
}
