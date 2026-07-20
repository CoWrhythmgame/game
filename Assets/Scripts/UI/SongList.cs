using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SongList : MonoBehaviour
{
    public int songIndex = 0;//곡 커서 위치
    public GameObject currentSelector;// 커서 위치한 songselector
    public List<GameObject> SongSelectors;
    public GameObject SSprefab;
    public GameObject songContentPannel;
    public GameObject songIndicatorobj;
    public int difficultyIndex = 0;
    public GameObject DifficultyContentPannel;
    public GameObject DifficultyL;
    public GameObject DifficultyR;

    // public KeySetting keySetting;
    InputSystem_Actions inputSystem_Actions;
    InputAction cursorAction;
    private Vector2 songtragetPos = new Vector2(0, 0);
    private Vector2 difficultytragetPos = new Vector2(0, 0);
    private SongIndicator songIndicator;
    RectTransform songcontentRect;
    RectTransform difficultycontentRect;
    List<Song> songs = new List<Song>();
    Song currentSongData;
    float SSheight;
    float SIwidth;

    void Awake()
    {
        songs = FileManager.LoadSong();
        Debug.Log(JsonUtility.ToJson(songs[0],true));
            TestMakeSong("test", "artist", 100, 12, 1, 1002, ComboResult.none, 80);
            TestMakeSong("test2", "me", 100, 11, 1, 108, ComboResult.allperfact, 81);
            TestMakeSong("TeSt3", "AAA", 123, new List<PatternInfo>()
            {
                new PatternInfo()
                {
                    patternPath = "Assets/Resources/Songs/test/test_pattern.json",
                    difficulty = 1,
                    totalNoteCount = 1
                },
                new PatternInfo()
                {
                    patternPath = "Assets/Resources/Songs/test/test_pattern.json",
                    difficulty = 2,
                    totalNoteCount = 2
                },
                new PatternInfo()
                {
                    patternPath = "Assets/Resources/Songs/test/test_pattern.json",
                    difficulty = 3,
                    totalNoteCount = 3
                },
                new PatternInfo()
                {
                    patternPath = "Assets/Resources/Songs/test/test_pattern.json",
                    difficulty = 4,
                    totalNoteCount = 4
                }
            }, 
            new List<Record>()
            {
                new Record()
                {
                    score = 1,
                    maxcombo = 0,
                    comboResult = ComboResult.none,
                    prate = 0
                },
                new Record()
                {
                    score = 2,
                    maxcombo = 2,
                    comboResult = ComboResult.fullcombo,
                    prate = 90
                },
                new Record()
                {
                    score = 3,
                    maxcombo = 3,
                    comboResult = ComboResult.allperfact,
                    prate = 100
                },
                new Record()
                {
                    score = 4,
                    maxcombo = 2,
                    comboResult = ComboResult.none,
                    prate = 50
                },
            });
        songIndicator = songIndicatorobj.GetComponent<SongIndicator>();
        SSheight = SSprefab.GetComponent<RectTransform>().rect.height;
        SIwidth = songIndicatorobj.GetComponent<RectTransform>().rect.width;
        Song temp = songs[2];
        // temp.record.Clear();
        // temp.pattern.Clear();
        Debug.Log(JsonUtility.ToJson(temp,true));
        Debug.Log(Application.streamingAssetsPath);
        inputSystem_Actions = new InputSystem_Actions();
        cursorAction = inputSystem_Actions.UI.Move;
        cursorAction.Enable();
        songcontentRect = songContentPannel.GetComponent<RectTransform>();
        difficultycontentRect = DifficultyContentPannel.GetComponent<RectTransform>();
        MakeSelectors(songs);
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
        foreach(Song song in songs)
        {
            GameObject selector = Instantiate(SSprefab);
            selector.transform.SetParent(songContentPannel.transform, false);
            selector.GetComponent<RectTransform>().anchoredPosition = pos;
            selector.GetComponent<SongSelector>().Setup(song);
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
    }
    #endregion
    #region 테스트용 함수
    public void TestMakeSong(string songname, string artist, float bpm, float difficulty, int totalnotecount, float score, ComboResult comboResult, float prate)
    {
            songs.Add(new Song(){
            songname = songname,
            artist = artist,
            bpm = bpm,
            songPath = "Assets/Resources/Songs/test/test.mp3",
            previewPath = "Assets/Resources/Songs/test/test_preview.mp3",
            patternInfo = new List<PatternInfo>(){
                new PatternInfo(){
                    patternPath = "Assets/Resources/Songs/test/test_pattern.json",
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
            songPath = "Assets/Resources/Songs/test/test.mp3",
            previewPath = "Assets/Resources/Songs/test/test_preview.mp3",
            patternInfo = patterns,
            record = records
        });
    }
    #endregion
}
