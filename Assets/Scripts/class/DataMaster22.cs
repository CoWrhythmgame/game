using UnityEngine;

public class DataMaster : MonoBehaviour
{
    public static DataMaster Instance { get; private set; }

    public Song CurrentSong { get; private set; }
    public int CurrentPatternIndex { get; private set; }
    public PlayData CurrentPlayData { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayResult(Song song, int patternIndex, PlayData playData)
    {
        CurrentSong = song;
        CurrentPatternIndex = patternIndex;
        CurrentPlayData = playData;
    }

    public void ClearPlayResult()
    {
        CurrentSong = null;
        CurrentPatternIndex = 0;
        CurrentPlayData = null;
    }
}