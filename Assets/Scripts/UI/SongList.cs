using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SongList : MonoBehaviour
{
    public int songIndex = 0;//곡 커서 위치
    public GameObject currentSelector;// 커서 위치한 songselector
    public List<GameObject> SongSelectors;
    public GameObject SSprefab;
    public GameObject contentPannel;
    public GameObject songIndicator;
    public int difficultyIndex = 0;
    // public KeySetting keySetting;
    InputSystem_Actions inputSystem_Actions;
    public InputAction cursorAction;
    private Vector2 tragetPos = new Vector2(0, 0);
    RectTransform contentRect;
    List<Song> songs = new List<Song>();
    float SSheight;
    void Awake()
    {
        TestMakeSong("test", "artist", 100, 12, 1, 1002, ComboResult.none, 80);
        TestMakeSong("test2", "me", 100, 11, 1, 108, ComboResult.allperfact, 81);

        SSheight = SSprefab.GetComponent<RectTransform>().rect.height;

        inputSystem_Actions = new InputSystem_Actions();
        cursorAction = inputSystem_Actions.UI.Move;
        cursorAction.Enable();
        contentRect = contentPannel.GetComponent<RectTransform>();
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
        contentRect.anchoredPosition = Vector2.Lerp(contentRect.anchoredPosition, tragetPos, 0.1f);
    }
    public void Setup(List<Song> songs)
    {
        foreach (GameObject obj in SongSelectors){
            Destroy(obj);
        }
        SongSelectors.Clear();
        
    }
    public void TestMakeSong(string songname, string artist, float bpm, float difficulty, int totalnotecount, float score, ComboResult comboResult, float prate)
    {
            songs.Add(new Song(){
            songname = songname,
            artist = artist,
            bpm = bpm,
            songPath = "Assets/Resources/Songs/test/test.mp3",
            previewPath = "Assets/Resources/Songs/test/test_preview.mp3",
            pattern = new List<Pattern>(){
                new Pattern(){
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
    public void MakeSelectors(List<Song> songs)
    {
        Vector3 pos = new Vector3(0, 0, 0);
        foreach(Song song in songs)
        {
            GameObject selector = Instantiate(SSprefab);
            selector.transform.SetParent(contentPannel.transform, false);
            selector.GetComponent<RectTransform>().anchoredPosition = pos;
            selector.GetComponent<SongSelector>().Setup(song);
            SongSelectors.Add(selector);
            pos += new Vector3(0, -(SSheight+10), 0);
        }
        currentSelector = SongSelectors[songIndex];
        EnableSelector(songIndex);
    }

    public void EnableSelector(int index)
    {
        currentSelector.GetComponent<SongSelector>().OffCursor();
        songIndex = index;
        currentSelector = SongSelectors[songIndex];
        currentSelector.GetComponent<SongSelector>().OnCursor();
        songIndicator.GetComponent<SongIndicator>().SetIndicator(currentSelector.GetComponent<SongSelector>().GetSong(), difficultyIndex);
    }
    void CursorMove(Vector2 input)
    {
        Debug.Log("input: " + input);
        if (input.y > 0)
        {
            if (songIndex > 0)
            {
                EnableSelector(songIndex - 1);
                Debug.Log("up");
            }
        }
        else if (input.y < 0)
        {
            if (songIndex < SongSelectors.Count - 1)
            {
                EnableSelector(songIndex + 1);
                Debug.Log("down");
            }
        }
        tragetPos = new Vector2(0, (SSheight+10) * songIndex);
    }
}
