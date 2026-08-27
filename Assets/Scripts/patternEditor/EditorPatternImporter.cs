using UnityEngine;

public class EditorPatternImporter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EditorSongInfoUI editorSongInfoUI;
    [SerializeField] private EditorDifficultyUI editorDifficultyUI;
    [SerializeField] private MeasureList measureList;
    [SerializeField] private BeatmapTimer beatmapTimer;
    [SerializeField] private EditorSongFileLoader editorSongFileLoader;

    [Header("Temporary Test")]
    [SerializeField] private string importSongName = "";

    public void ImportPattern(bool isbuiltin)
    {
        if (string.IsNullOrWhiteSpace(importSongName))
        {
            Debug.LogWarning("Import Song Name�� ��� �ֽ��ϴ�.");
            return;
        }

        if (editorDifficultyUI == null)
        {
            Debug.LogWarning("EditorDifficultyUI�� ������� �ʾҽ��ϴ�.");
            return;
        }

        if (measureList == null)
        {
            Debug.LogWarning("MeasureList�� ������� �ʾҽ��ϴ�.");
            return;
        }

        Song song = null;

        bool hasSavedSongInfo = FileManager.Editor_TryLoadSongInfo(importSongName, isbuiltin, out song);

        if (!hasSavedSongInfo)
        {
            Debug.LogWarning("StreamingAssets �� ������ �����ϴ�. EditorSongs���� ���� �ҷ��ɴϴ�: " + importSongName);

            if (editorSongFileLoader == null)
            {
                Debug.LogWarning("EditorSongFileLoader�� ������� �ʾҽ��ϴ�.");
                return;
            }

            if (!editorSongFileLoader.TryLoadExistingEditorSongByName(importSongName, out EditorLoadedSongData loadedData))
            {
                Debug.LogWarning("EditorSongs������ ���� �ҷ����� ���߽��ϴ�: " + importSongName);
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
                isbuiltin,
                out PatternInfo patternInfo
            );

            bool hasPattern = FileManager.Editor_TryLoadPattern(
                importSongName,
                i,
                isbuiltin,
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

            Debug.LogWarning("����� ä���� ���� ���Ǹ� �ҷ����� �� Easy �������� �����մϴ�: " + importSongName);
        }
        else
        {
            editorDifficultyUI.SelectImportedDifficulty(firstEnabledIndex);
        }
        Debug.Log("Imported editor song: " + importSongName);
    }

    public void ImportPatternBySongName(string songName, bool isbuiltin)
    {
        importSongName = songName;
        ImportPattern(isbuiltin);
    }
    private void Start()
    {
        TryRestoreFromTestPlay();
    }

    private void TryRestoreFromTestPlay()
    {
        if (!EditorReturnContext.ShouldRestore)
            return;

        string songName = EditorReturnContext.SongName;
        int difficultyIndex = EditorReturnContext.DifficultyIndex;

        EditorReturnContext.Clear();
        bool isbuiltin = DataMaster.Instance.GetIsBuiltin();
        ImportPatternBySongName(songName, isbuiltin);

        if (editorDifficultyUI != null)
            editorDifficultyUI.SelectImportedDifficulty(difficultyIndex);
    }
}