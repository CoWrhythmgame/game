using UnityEngine;
using UnityEngine.UI;

public class SongIndicator : MonoBehaviour
{
    [SerializeField]private Text songname;
    [SerializeField]private Text artist;
    [SerializeField]private Text BPM;
    [SerializeField]private Text difficulty;
    [SerializeField]private Text TotalNote;
    [SerializeField]private Text Score;
    [SerializeField]private Text Combo;
    [SerializeField]private Text Rate;
    [SerializeField]private Text RecordComboResult;
    [SerializeField]private Image jacketImage;

    [SerializeField]private Sprite DefaultJaket;
    public void SetIndicator(Song song, int difficultyindex, Sprite jacketSprite)
    {
        if(jacketSprite != null)
        {
            jacketImage.sprite = jacketSprite;
        }
        else
        {
            Debug.LogWarning("자켓 이미지가 null입니다. 기본 자켓 이미지를 사용합니다.");
            jacketImage.sprite = DefaultJaket;
        }
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
