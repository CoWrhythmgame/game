using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SFB;

public class RuntimeSongFilePicker : MonoBehaviour
{
    [Header("Load Song")]
    [SerializeField] private EditorSongFileLoader songFileLoader;

    [Header("Import Saved Editor Song")]
    [SerializeField] private Button importButton;
    [SerializeField] private EditorPatternImporter patternImporter;

    private void OnEnable()
    {
        if (songFileLoader != null)
            songFileLoader.OnSongFileOpenRequested += OpenSongAudioFileBrowser;

        if (importButton != null)
            importButton.onClick.AddListener(OpenSavedSongInfoFileBrowser);

    }

    private void OnDisable()
    {
        if (songFileLoader != null)
            songFileLoader.OnSongFileOpenRequested -= OpenSongAudioFileBrowser;

        if (importButton != null)
            importButton.onClick.RemoveListener(OpenSavedSongInfoFileBrowser);
    }

    private void OpenSongAudioFileBrowser()
    {
        ExtensionFilter[] extensions =
        {
            new ExtensionFilter("Audio Files", "mp3", "wav", "ogg"),
            new ExtensionFilter("All Files", "*")
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel(
            "Load Song Audio",
            "",
            extensions,
            false
        );

        if (paths == null || paths.Length == 0)
        {
            Debug.Log("Song loading canceled.");
            return;
        }

        string path = paths[0];

        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.Log("Song loading canceled.");
            return;
        }

        if (songFileLoader == null)
        {
            Debug.LogWarning("EditorSongFileLoader�� ������� �ʾҽ��ϴ�.");
            return;
        }

        songFileLoader.LoadSongFromPath(path);
    }

    private void OpenSavedSongInfoFileBrowser()
    {
        ExtensionFilter[] extensions =
        {
            new ExtensionFilter("Song Info Json", "json"),
            new ExtensionFilter("All Files", "*")
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel(
            "Import Editor Song Info",
            "",
            extensions,
            false
        );

        if (paths == null || paths.Length == 0)
        {
            Debug.Log("Import song info canceled.");
            return;
        }

        string path = paths[0];

        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.Log("Import song info canceled.");
            return;
        }

        ImportSongInfoFromPath(path);
    }

    private void ImportSongInfoFromPath(string path)
    {
        // 선택한 파일 자체가 존재하지 않음
        if (!File.Exists(path))
        {
            Debug.LogWarning("Selected file does not exist: " + path);

            if (songFileLoader != null)
            {
                songFileLoader.ShowLoadError(
                    "The selected file could not be found."
                );
            }

            return;
        }

        string fileName = Path.GetFileName(path);

        // song_info.json이 아닌 다른 JSON 선택
        if (fileName != "song_info.json")
        {
            Debug.LogWarning("Please select song_info.json: " + path);

            if (songFileLoader != null)
            {
                songFileLoader.ShowLoadError(
                    "Please select a song_info.json file."
                );
            }

            return;
        }

        EditorLoadedSongData songData;

        // JSON 읽기 / 파싱
        try
        {
            string json = File.ReadAllText(path);

            songData =
                JsonUtility.FromJson<EditorLoadedSongData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);

            if (songFileLoader != null)
            {
                songFileLoader.ShowLoadError(
                    "The selected song_info.json file is invalid."
                );
            }

            return;
        }

        // JSON은 열렸지만 필요한 곡 정보가 없음
        if (songData == null ||
            string.IsNullOrWhiteSpace(songData.songName))
        {
            Debug.LogWarning(
                "Invalid song_info.json: " + path
            );

            if (songFileLoader != null)
            {
                songFileLoader.ShowLoadError(
                    "The selected song_info.json file is invalid."
                );
            }

            return;
        }

        if (patternImporter == null)
        {
            Debug.LogWarning(
                "EditorPatternImporter is not connected."
            );

            if (songFileLoader != null)
            {
                songFileLoader.ShowLoadError(
                    "Pattern importer is not available."
                );
            }

            return;
        }

        // 정상 파일이면 Import 진행
        patternImporter.ImportPatternBySongName(songData.songName, false);
    }
}