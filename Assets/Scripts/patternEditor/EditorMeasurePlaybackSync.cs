using UnityEngine;

public class EditorMeasurePlaybackSync : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EditorSongAudioController audioController;
    [SerializeField] private EditorSongFileLoader songFileLoader;
    [SerializeField] private MeasureList measureList;

    [Header("Settings")]
    [SerializeField] private bool syncOnlyWhilePlaying = true;

    private void Update()
    {
        if (audioController == null)
            return;

        if (songFileLoader == null)
            return;

        if (measureList == null)
            return;

        if (!songFileLoader.HasLoadedSong)
            return;

        if (syncOnlyWhilePlaying && !audioController.IsPlaying)
            return;


        float bpm = songFileLoader.GetCurrentBpm();
        float songTime = audioController.CurrentTime;

        measureList.SyncCameraToSongTime(songTime, bpm);
    }
}