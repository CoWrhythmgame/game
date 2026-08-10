using UnityEngine;

public class EditorPatternImporter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EditorSongInfoUI editorSongInfoUI;
    [SerializeField] private EditorDifficultyUI editorDifficultyUI;
    [SerializeField] private MeasureList measureList;
    [SerializeField] private BeatmapTimer beatmapTimer;
    [SerializeField] private DirtyState dirtyState;
    [SerializeField] private EditorSongFileLoader editorSongFileLoader;

    [Header("Temporary Test")]
    [SerializeField] private string importSongName = "";

    public void ImportPattern()
    {
        if (string.IsNullOrWhiteSpace(importSongName))
        {
            Debug.LogWarning("Import Song Name이 비어 있습니다.");
            return;
        }

        if (!FileManager.Editor_TryLoadSongInfo(importSongName, out Song song))
        {
            Debug.LogWarning("곡 정보를 불러오지 못했습니다: " + importSongName);
            return;
        }

        if (editorDifficultyUI == null)
        {
            Debug.LogWarning("EditorDifficultyUI가 연결되지 않았습니다.");
            return;
        }

        if (measureList == null)
        {
            Debug.LogWarning("MeasureList가 연결되지 않았습니다.");
            return;
        }

        if (editorSongInfoUI != null)
        {
            editorSongInfoUI.SetSongInfo(song.songname, song.artist, song.bpm);
        }

        if (editorSongFileLoader != null)
        {
            editorSongFileLoader.SetCurrentSongDataFromImport(song);
        }

        if (beatmapTimer != null)
        {
            beatmapTimer.SetSingleBpm(song.bpm);
        }

        if (dirtyState != null)
            dirtyState.ClearDirty();

        editorDifficultyUI.BeginImport();

        int firstEnabledIndex = -1;

        for (int i = 0; i < 4; i++)
        {
            bool hasPatternInfo = FileManager.Editor_TryLoadPatternInfo(
                importSongName,
                i,
                out PatternInfo patternInfo
            );

            bool hasPattern = FileManager.Editor_TryLoadPattern(
                importSongName,
                i,
                out Pattern pattern
            );

            bool enabled = hasPatternInfo && hasPattern;

            float level = 0.0f;

            if (hasPatternInfo && patternInfo != null)
                level = patternInfo.difficulty;

            editorDifficultyUI.ApplyImportedDifficulty(i, enabled, level);

            if (enabled)
            {
                editorDifficultyUI.SetImportedPattern(i, pattern);

                if (firstEnabledIndex == -1)
                    firstEnabledIndex = i;
            }
        }

        if (firstEnabledIndex == -1)
        {
            measureList.ClearPattern();
            Debug.LogWarning("불러올 수 있는 채보가 없습니다: " + importSongName);
            return;
        }

        editorDifficultyUI.SelectImportedDifficulty(firstEnabledIndex);

        Debug.Log("Imported pattern song: " + importSongName);
    }
}