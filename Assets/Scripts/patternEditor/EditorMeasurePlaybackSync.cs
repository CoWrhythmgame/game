using UnityEngine;

public class EditorMeasurePlaybackSync : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EditorSongAudioController audioController;
    [SerializeField] private EditorSongFileLoader songFileLoader;
    [SerializeField] private MeasureList measureList;
    [SerializeField] private BeatmapTimer beatmapTimer;
    [SerializeField] private EditorJudgeLine judgeLine;

    [Header("Settings")]
    [SerializeField] private bool syncOnlyWhilePlaying = true;

    private void Start()
    {
        if (songFileLoader != null)
            songFileLoader.OnSongLoadedOrUpdated += OnSongLoadedOrUpdated;
    }

    private void OnDestroy()
    {
        if (songFileLoader != null)
            songFileLoader.OnSongLoadedOrUpdated -= OnSongLoadedOrUpdated;
    }

    private void OnSongLoadedOrUpdated(EditorLoadedSongData songData)
    {
        if (beatmapTimer == null)
            return;

        if (songData == null)
            return;

        beatmapTimer.SetSingleBpm(songData.bpm);
    }

    private void Update()
    {
        if (audioController == null)
            return;

        if (songFileLoader == null)
            return;

        if (measureList == null)
            return;

        if (beatmapTimer == null)
            return;

        if (judgeLine == null)
            return;

        if (!songFileLoader.IsSongLoaded())
            return;

        if (syncOnlyWhilePlaying && !audioController.IsPlaying)
            return;

        double songTime = audioController.CurrentTime;
        double measureProgress = beatmapTimer.GetMeasureProgressByTime(songTime);

        measureList.SyncCameraToMeasureProgress(
            measureProgress,
            judgeLine.GetLocalYOffset()
        );
    }
}