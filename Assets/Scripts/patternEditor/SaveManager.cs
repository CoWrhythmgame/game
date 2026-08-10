using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    [SerializeField] MeasureList _measureList;
    [SerializeField] EditorSongFileLoader _editorSongFileLoader;
    [SerializeField] EditorSongInfoUI _editorSongInfoUI;
    [SerializeField] private EditorDifficultyUI editorDifficultyUI;
    [SerializeField] private DirtyState dirtyState;
    public bool SavePattern()
    {
        try{
            EditorLoadedSongData editorLoadedSongData = _editorSongFileLoader.GetCurrentSongData();
            Pattern pattern = _measureList.GetPattern();
            Song song = new Song
            {
                songname = editorLoadedSongData.songName,
                bpm = editorLoadedSongData.bpm,
                artist = editorLoadedSongData.artistName
            };
            PatternInfo patternInfo = new PatternInfo
            {
                difficulty = 2,
                totalNoteCount = pattern.notes.Count
            };
            FileManager.Editor_SavePattern(song, patternInfo, pattern, _editorSongInfoUI.GetDifficultyIndex());
        }
        catch
        {
            Debug.LogWarning("노래 정보에 문제가 생겼습니다!");
        }
    }
    public bool TrySavePattern()
    {
        DataMaster dataMaster = GameObject.FindGameObjectWithTag("DataMaster").GetComponent<DataMaster>();
        try{
            EditorLoadedSongData editorLoadedSongData = _editorSongFileLoader.GetCurrentSongData();
            Pattern pattern = _measureList.GetPattern();
            Song song = new Song
            {
                songname = editorLoadedSongData.songName,
                bpm = editorLoadedSongData.bpm,
                artist = editorLoadedSongData.artistName
            };
            PatternInfo patternInfo = new PatternInfo
            {
                difficulty = 2,
                totalNoteCount = pattern.notes.Count
            };
            FileManager.Editor_SavePattern(song, patternInfo, pattern, _editorSongInfoUI.GetDifficultyIndex());
            
            //FIXME: 파일 저장 경로 싹 바꿀것
            //HACK: AssetDataBase는 UnityEditor 클래스의 메서드라서 에디터에서만 사용 가능함.
            AssetDatabase.Refresh();

            song = FileManager.LoadSong().Where(x => x.songname == song.songname).ToArray()[0];
            
            dataMaster.SetSongData(song, editorLoadedSongData.selectedDifficultyIndex);
            dataMaster.SetIsTestPlay(true);
            return true;
        }
        catch
        {
            Debug.LogWarning("노래 정보에 문제가 생겼습니다!");
            return false;
        }
    }
    public void TestPlay()
    {
        if(TrySavePattern()){
            SceneManager.LoadScene("InGameScene");
        }
    }
    public void SavePatternOld()
    {
        if (_editorSongFileLoader == null)
        {
            Debug.LogWarning("EditorSongFileLoader가 연결되지 않았습니다..");
            return false;
        }

        if (_measureList == null)
        {
            Debug.LogWarning("MeasureList가 연결되지 않았습니다.");
            return false;
        }

        if (editorDifficultyUI == null)
        {
            Debug.LogWarning("EditorDifficultyUI가 연결되지 않았습니다.");
            return false;
        }

        if (!editorDifficultyUI.CanSaveCurrentDifficulty())
        {
            Debug.LogWarning("현재 난이도는 저장할 수 없습니다. 체크 여부와 난이도 값을 확인하세요.");
            return false;
        }

        EditorLoadedSongData editorLoadedSongData = _editorSongFileLoader.GetCurrentSongData();

        if (editorLoadedSongData == null)
        {
            Debug.LogWarning("로드된 곡 정보가 없습니다.");
            return false;
        }

        editorDifficultyUI.SaveCurrentPatternToMemory();

        int difficultyIndex = editorDifficultyUI.CurrentDifficultyIndex;
        float difficultyLevel = editorDifficultyUI.GetDifficultyLevel(difficultyIndex);

        Pattern pattern = _measureList.GetPattern();

        Song song = new Song
        {
            songname = editorLoadedSongData.songName,
            bpm = editorLoadedSongData.bpm,
            artist = editorLoadedSongData.artistName
        };

        PatternInfo patternInfo = new PatternInfo
        {
            difficulty = Mathf.RoundToInt(difficultyLevel),
            totalNoteCount = pattern.notes.Count
        };

        FileManager.Editor_SavePattern(song, patternInfo, pattern, difficultyIndex);

        if (dirtyState != null)
            dirtyState.MarkSaved();

        Debug.Log("Pattern saved. Difficulty: " + editorDifficultyUI.GetDifficultyName(difficultyIndex));

        return true;
    }
}
