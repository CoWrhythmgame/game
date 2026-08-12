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

        Song song = null;

        bool hasSavedSongInfo = FileManager.Editor_TryLoadSongInfo(importSongName, out song);

        if (!hasSavedSongInfo)
        {
            Debug.LogWarning("StreamingAssets 곡 정보가 없습니다. EditorSongs에서 곡을 불러옵니다: " + importSongName);

            if (editorSongFileLoader == null)
            {
                Debug.LogWarning("EditorSongFileLoader가 연결되지 않았습니다.");
                return;
            }

            if (!editorSongFileLoader.TryLoadExistingEditorSongByName(importSongName, out EditorLoadedSongData loadedData))
            {
                Debug.LogWarning("EditorSongs에서도 곡을 불러오지 못했습니다: " + importSongName);
                return;
            }

            song = new Song
            {
                songname = loadedData.songName,
                artist = loadedData.artistName,
                bpm = loadedData.bpm
            };
        }
        else
        {
            if (editorSongFileLoader != null)
            {
                editorSongFileLoader.SetCurrentSongDataFromImport(song);
            }
        }

        if (editorSongInfoUI != null)
        {
            editorSongInfoUI.SetSongInfo(song.songname, song.artist, song.bpm);
        }

        if (beatmapTimer != null)
        {
            beatmapTimer.SetSingleBpm(song.bpm);
        }

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

            editorDifficultyUI.ApplyImportedDifficulty(0, true, 1);
            editorDifficultyUI.SelectImportedDifficulty(0);

            Debug.LogWarning("저장된 채보가 없어 음악만 불러오고 빈 Easy 패턴으로 시작합니다: " + importSongName);
        }
        else
        {
            editorDifficultyUI.SelectImportedDifficulty(firstEnabledIndex);
        }

        if (dirtyState != null)
            dirtyState.ClearDirty();

        Debug.Log("Imported editor song: " + importSongName);
    }

    public void ImportPatternBySongName(string songName)
    {
        importSongName = songName;
        ImportPattern();
    }
}