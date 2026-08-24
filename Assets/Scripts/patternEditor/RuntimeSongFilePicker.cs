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
        if (!File.Exists(path))
        {
            Debug.LogWarning("������ song_info.json ������ �������� �ʽ��ϴ�: " + path);
            return;
        }

        string fileName = Path.GetFileName(path);

        if (fileName != "song_info.json")
        {
            Debug.LogWarning("song_info.json ������ �����ؾ� �մϴ�: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        EditorLoadedSongData songData = JsonUtility.FromJson<EditorLoadedSongData>(json);

        if (songData == null || string.IsNullOrWhiteSpace(songData.songName))
        {
            Debug.LogWarning("song_info.json���� �� �̸��� ���� ���߽��ϴ�: " + path);
            return;
        }

        if (patternImporter == null)
        {
            Debug.LogWarning("EditorPatternImporter�� ������� �ʾҽ��ϴ�.");
            return;
        }

        bool isbuiltin = FileManager.ChackBuiltIn(path);

        patternImporter.ImportPatternBySongName(songData.songName, isbuiltin);
    }
}