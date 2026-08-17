using UnityEngine;
using UnityEngine.UI;

public class SongSelector : MonoBehaviour
{
    [SerializeField] private Text _songname;
    [SerializeField] private Text _artist;
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private Image _arrowImage;
    [SerializeField] private Sprite _normalArrowImage;
    [SerializeField] private Sprite _selectedArrowImage;
    private Song _song;
    private Image _image;



    public void Setup(Song song)
    {
        _song = song;
        _songname.text = _song.songname;
        _artist.text = _song.artist;

        _image = GetComponent<Image>();
        _image.sprite = _normalSprite;
        _arrowImage.sprite = _normalArrowImage;
        OffCursor();
    }
    public Song GetSong()
    {
        return _song;
    }
    public void OnCursor()
    {
        _image.sprite = _selectedSprite;
        _arrowImage.sprite = _selectedArrowImage;
        _image.color = Color.white;
        _arrowImage.color = Color.white;
    }
    public void OffCursor()
    {
        _image.sprite = _normalSprite;
        _arrowImage.sprite = _normalArrowImage;
        _image.color = Color.gray;
        _arrowImage.color = Color.gray;
    }
}
