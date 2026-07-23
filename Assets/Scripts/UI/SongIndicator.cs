using UnityEngine;
using UnityEngine.UI;

public class SongIndicator : MonoBehaviour
{
    public Text songname;
    public Text artist;
    public Text BPM;
    public Text difficulty;
    public Text TotalNote;
    public Text Score;
    public Text Combo;
    public Text Rate;
    public Text RecordComboResult;
    public void SetIndicator(Song song, int difficultyindex)
    {
        songname.text = song.songname;
        artist.text = song.artist;
        difficulty.text = song.patternInfo[difficultyindex].difficulty.ToString();
        TotalNote.text = song.patternInfo[difficultyindex].totalNoteCount.ToString();
        BPM.text = song.bpm.ToString();
        Score.text = song.record[difficultyindex].score.ToString();
        Combo.text = song.record[difficultyindex].maxcombo.ToString();
        Rate.text = song.record[difficultyindex].prate.ToString();
        Color newcolor = new Color32(255,255,255,0);
        // Debug.Log("songname: " + song.songname);
        // Debug.Log("artist: " + song.artist);
        // Debug.Log("difficulty: " + song.pattern[difficultyindex].difficulty);
        // Debug.Log("totalNoteCount: " + song.pattern[difficultyindex].totalNoteCount);
        // Debug.Log("score: " + song.record[difficultyindex].score);
        // Debug.Log("maxcombo: " + song.record[difficultyindex].maxcombo);
        // Debug.Log("comboResult: " + song.record[difficultyindex].comboResult);

        if(song.record[difficultyindex].comboResult == ComboResult.none)
        {
            RecordComboResult.text = "";
        }
        else if(song.record[difficultyindex].comboResult == ComboResult.fullcombo)
        {
            newcolor = new Color32(20,190,160,255);
            RecordComboResult.text = "FC";
        }
        else if(song.record[difficultyindex].comboResult == ComboResult.allperfact)
        {
            newcolor = new Color32(190,20,160,255);
            RecordComboResult.text = "AP";
        }
            RecordComboResult.color = newcolor;
    }
    
}
