using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SongList : MonoBehaviour
{
    public int songIndex = 0;//곡 커서 위치
    public GameObject currentSelector;// 커서 위치한 songselector
    public List<GameObject> SongSelectors;
    public GameObject SSprefab;
    public GameObject songContentPannel;
    public GameObject songIndicatorobj;
    [SerializeField] private RectTransform _builtinFolder;
    [SerializeField] private RectTransform _customFolder;
    public int difficultyIndex = 0;
    public GameObject DifficultyContentPannel;
    public GameObject DifficultyL;
    public GameObject DifficultyR;

    [SerializeField] private DataMaster _dataMaster;
    [SerializeField] private OptionMenuUI _optionMenuUI;
    // public KeySetting keySetting;
    InputSystem_Actions inputSystem_Actions;
    InputAction cursorAction;
    InputAction _cursorSummit;
    private Vector2 songtragetPos = new Vector2(0, 0);
    private Vector2 difficultytragetPos = new Vector2(0, 0);
    private SongIndicator songIndicator;
    RectTransform songcontentRect;
    RectTransform difficultycontentRect;
    List<Song> songs = new List<Song>();
    Song currentSongData;
    PlayOption _currentPlayOption;
    float SSheight;
    float SIwidth;
    private int _builtinSongCount = 0;

    void Awake()
    {
        songs = FileManager.LoadSong(true);
        _builtinSongCount = songs.Count;
        songs.AddRange(FileManager.LoadSong(false));
        Debug.Log(JsonUtility.ToJson(songs[0],true));
        songIndicator = songIndicatorobj.GetComponent<SongIndicator>();
        SSheight = SSprefab.GetComponent<RectTransform>().rect.height;
        SIwidth = songIndicatorobj.GetComponent<RectTransform>().rect.width;
        _customFolder.anchoredPosition = new Vector3(-15, _builtinSongCount * -SSheight-30, 0);
        Song temp = songs[2];
        // temp.record.Clear();
        // temp.pattern.Clear();
        Debug.Log(JsonUtility.ToJson(temp,true));
        Debug.Log(Application.streamingAssetsPath);
        inputSystem_Actions = new InputSystem_Actions();

        _dataMaster = GameObject.FindGameObjectWithTag("DataMaster").GetComponent<DataMaster>();
        

        songcontentRect = songContentPannel.GetComponent<RectTransform>();
        difficultycontentRect = DifficultyContentPannel.GetComponent<RectTransform>();
        MakeSelectors(songs);
    }
    void OnEnable()
    {
        
        cursorAction = inputSystem_Actions.UI.Move;
        _cursorSummit = inputSystem_Actions.UI.Summit;
        cursorAction.Enable();
        _cursorSummit.Enable();
    }
    void OnDisable()
    {
        
        cursorAction.Disable();
        _cursorSummit.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (OptionMenuUI.IsOptionOpen) // 추가한 코드 : 옵션 메뉴가 열려있으면 방향키 입력을 무시하도록 함
        {
            return;
        }
        Vector2 input = cursorAction.ReadValue<Vector2>();
        if (cursorAction.WasPressedThisFrame())
        {
            CursorMove(input);
        }
        if (_cursorSummit.WasCompletedThisFrame())
        {
            CursorSummit();
        }
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene("StartScene");
        }
    }
    void FixedUpdate()
    {
        songcontentRect.anchoredPosition = Vector2.Lerp(songcontentRect.anchoredPosition, songtragetPos, 0.1f);
        difficultycontentRect.anchoredPosition = Vector2.Lerp(difficultycontentRect.anchoredPosition, difficultytragetPos, 0.1f);
    }

    #region songselector 생성
    //songs값에 따라 초기화 후 songselector를 만드는 함수
    public void Setup(List<Song> songs)
    {
        foreach (GameObject obj in SongSelectors){
            Destroy(obj);
        }
        SongSelectors.Clear();
        MakeSelectors(songs);
        
    }
    //songSelector를 만드는 함수
    public void MakeSelectors(List<Song> songs)
    {
        Vector3 pos = new Vector3(0, 0, 0);
        for (int i = 0; i < songs.Count; i++)
        {
            GameObject selector = Instantiate(SSprefab);
            selector.transform.SetParent(songContentPannel.transform, false);
            if(i < _builtinSongCount)
            {
                selector.GetComponent<RectTransform>().anchoredPosition = pos;
            }
            else
            {
                selector.GetComponent<RectTransform>().anchoredPosition = pos + new Vector3(0, -130, 0);
            }
            selector.GetComponent<SongSelector>().Setup(songs[i]);
            SongSelectors.Add(selector);
            pos += new Vector3(0, -(SSheight+10), 0);
        }
        currentSelector = SongSelectors[songIndex];
        EnableSelector(songIndex);
    }
    #endregion
    #region 커서 관련
    //songselect커서 이동에 대한 함수 index로 이동
    public void EnableSelector(int index)
    {
        if(index < 0 || index >= SongSelectors.Count)
        {
            return;
        }
        currentSelector.GetComponent<SongSelector>().OffCursor();
        songIndex = index;
        currentSelector = SongSelectors[songIndex];
        currentSelector.GetComponent<SongSelector>().OnCursor();
        currentSongData = currentSelector.GetComponent<SongSelector>().GetSong();
        if(difficultyIndex >= currentSongData.patternInfo.Count)
        {
            SetDifficulty(currentSongData.patternInfo.Count-1);
        }else
        {
            SetDifficulty(difficultyIndex);
        }
        songIndicator.SetIndicator(currentSongData, difficultyIndex);
    }
    //
    public void SetDifficulty(int index)
    {
        int dcount = currentSongData.patternInfo.Count;
        if(dcount <= index || index < 0)
        {
            return;
        }
        if(index == 0)
        {
            DifficultyL.SetActive(false);
        }
        else DifficultyL.SetActive(true);

        if(index == dcount - 1)
        {
            DifficultyR.SetActive(false);
        }
        else DifficultyR.SetActive(true);

        difficultyIndex = index;
        difficultytragetPos = new Vector2(-SIwidth*index, 0);
        songIndicator.SetIndicator(currentSongData, difficultyIndex);
    }
    //커서 이동 판별 함수
    void CursorMove(Vector2 input)
    {
        Debug.Log("input: " + input);
        if (input.y > 0)
        {
            EnableSelector(songIndex - 1);
        }
        else if (input.y < 0)
        {
            EnableSelector(songIndex + 1);
        }
        if(input.x > 0)
        {
            SetDifficulty(difficultyIndex + 1);
        }
        else if(input.x < 0)
        {
            SetDifficulty(difficultyIndex - 1);
        }
        songtragetPos = new Vector2(0, (SSheight+10) * songIndex);
        if(songIndex >= _builtinSongCount)
        {
            songtragetPos += new Vector2(0, 130);
        }
    }
    private async void CursorSummit()
    {
        _dataMaster.SetSongData(currentSongData, difficultyIndex);
        _dataMaster.SetOption(_optionMenuUI.GetCurrentPlayOption());
        if(_builtinSongCount > songIndex)
        {
            _dataMaster.SetIsBuiltin(true);
        }
        else
        {
            _dataMaster.SetIsBuiltin(false);
        }
        AudioClip musicClip = await FileManager.LoadMusic(currentSongData, false);
        _dataMaster.SetMusic(musicClip);
        SceneManager.LoadScene("InGameScene");
    }
    #endregion
    #region 테스트용 함수
    public void TestMakeSong(string songname, string artist, float bpm, float difficulty, int totalnotecount, float score, ComboResult comboResult, float prate)
    {
            songs.Add(new Song(){
            songname = songname,
            artist = artist,
            bpm = bpm,
            patternInfo = new List<PatternInfo>(){
                new PatternInfo(){
                    difficulty = difficulty,
                    totalNoteCount = totalnotecount
                }
            },
            record = new List<Record>(){
                new Record(){
                    score = score,
                    maxcombo = 100,
                    comboResult = comboResult,
                    prate = prate
                }
            }
        });
    }
    public void TestMakeSong(string songname, string artist, float bpm, List<PatternInfo> patterns, List<Record> records)
    {
            songs.Add(new Song(){
            songname = songname,
            artist = artist,
            bpm = bpm,
            patternInfo = patterns,
            record = records
        });
    }
    #endregion
}
