using UnityEngine;
using UnityEngine.UI;

public class SongSelector : MonoBehaviour
{
    private Song song;
    public Text songname;
    public Text artist;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Setup(Song song)
    {
        this.song = song;
        songname.text = song.songname;
        artist.text = song.artist;
    }
    public void OnCursor()
    {
        GetComponent<Image>().color = Color.white;
    }
    public void OffCursor()
    {
        GetComponent<Image>().color = Color.gray;
    }
}
