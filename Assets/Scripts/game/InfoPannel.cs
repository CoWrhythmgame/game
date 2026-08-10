using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPannel : MonoBehaviour
{
    #region VariableArea
    [Header("SongName")]
    [SerializeField] private Image _jaketImage;
    [SerializeField] private TMPro.TMP_Text _songNameText;
    [SerializeField] private TMPro.TMP_Text _artistText;
    [SerializeField] private TMPro.TMP_Text _bpmText;
    [SerializeField] private List<TextPannel> _judgePannels;
    [SerializeField] private TextPannel _scorePannel;
    [SerializeField] private TextPannel _percentPannel;
    #endregion

    public void SetSongInfo(Song songData)
    {
        _songNameText.text = songData.songname;
        _artistText.text = songData.artist;
        _bpmText.text = "BPM "+songData.bpm.ToString();
    }

    public void SetJudgeCount(int[] judgeCount)
    {
        for(int i = 0; i < 4; i++)
        {
            _judgePannels[i].SetContent(judgeCount[i].ToString());
        }
    }
    public void SetScore(long score, double prate)
    {
        _scorePannel.SetContent(score.ToString());
        _percentPannel.SetContent(prate.ToString("0.00"));
    }
    
}
