using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField] MeasureList _measureList;
    [SerializeField] EditorSongFileLoader _editorSongFileLoader;
    [SerializeField] EditorSongInfoUI _editorSongInfoUI;

    public void SavePattern()
    {
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
}
