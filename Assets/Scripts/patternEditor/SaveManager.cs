using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    [SerializeField] MeasureList _measureList;
    [SerializeField] EditorSongFileLoader _editorSongFileLoader;
    [SerializeField] EditorSongInfoUI _editorSongInfoUI;

    public void SavePattern()
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
            song = FileManager.LoadSong().Where(x => x.songname == song.songname).ToArray()[0];
            dataMaster.SetSongData(song, editorLoadedSongData.selectedDifficultyIndex);
            dataMaster.SetIsTestPlay(true);
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
}
