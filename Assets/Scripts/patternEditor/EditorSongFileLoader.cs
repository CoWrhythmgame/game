using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorSongFileLoader : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private EditorSongInfoUI songInfoUI;
    [SerializeField] private Button loadSongButton;

    [Header("Song Meta Input Panel")]
    [SerializeField] private GameObject songMetaInputPanel;
    [SerializeField] private TMP_InputField songNameInput;
    [SerializeField] private TMP_InputField artistInput;
    [SerializeField] private TMP_InputField bpmInput;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Default Values")]
    [SerializeField] private string defaultArtistName = "Unknown Artist";

    private string pendingSourcePath = "";
    private EditorLoadedSongData currentSongData;

    public EditorLoadedSongData CurrentSongData => currentSongData;

    private void Awake()
    {
        if (loadSongButton != null)
            loadSongButton.onClick.AddListener(OpenSongFile);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmSongInfo);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelSongInfoInput);

        HideSongMetaInputPanel();
    }

    private void OnDestroy()
    {
        if (loadSongButton != null)
            loadSongButton.onClick.RemoveListener(OpenSongFile);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(ConfirmSongInfo);

        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(CancelSongInfoInput);
    }

    public void OpenSongFile()
    {
#if UNITY_EDITOR
        string sourcePath = EditorUtility.OpenFilePanel(
            "Load Song Audio",
            "",
            "mp3,wav,ogg"
        );

        if (string.IsNullOrEmpty(sourcePath))
        {
            Debug.Log("Song loading canceled.");
            return;
        }

        if (!IsSupportedAudioFile(sourcePath))
        {
            Debug.LogWarning("Unsupported audio file. Only mp3, wav, and ogg files are allowed.");
            return;
        }

        pendingSourcePath = sourcePath;
        ShowSongMetaInputPanel(sourcePath);
#else
        Debug.LogWarning("This file loading method only works in the Unity Editor.");
#endif
    }

    private void ShowSongMetaInputPanel(string sourcePath)
    {
        if (songMetaInputPanel != null)
            songMetaInputPanel.SetActive(true);

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);

        if (songNameInput != null)
            songNameInput.text = fileNameWithoutExtension;

        if (artistInput != null)
            artistInput.text = defaultArtistName;

        if (bpmInput != null)
            bpmInput.text = "";

        SetError("");
    }

    private void HideSongMetaInputPanel()
    {
        if (songMetaInputPanel != null)
            songMetaInputPanel.SetActive(false);

        SetError("");
    }

    private void ConfirmSongInfo()
    {
        if (string.IsNullOrEmpty(pendingSourcePath))
        {
            SetError("No audio file selected.");
            return;
        }

        string songName = songNameInput != null ? songNameInput.text.Trim() : "";
        string artistName = artistInput != null ? artistInput.text.Trim() : "";
        string bpmString = bpmInput != null ? bpmInput.text.Trim() : "";

        if (string.IsNullOrEmpty(songName))
        {
            SetError("Please enter a song name.");
            return;
        }

        if (string.IsNullOrEmpty(artistName))
        {
            artistName = defaultArtistName;
        }

        if (!float.TryParse(bpmString, out float bpm))
        {
            SetError("Please enter BPM as a number.");
            return;
        }

        if (bpm <= 0f)
        {
            SetError("BPM must be greater than 0.");
            return;
        }

        ImportSongFile(pendingSourcePath, songName, artistName, bpm);

        pendingSourcePath = "";
        HideSongMetaInputPanel();
    }

    private void CancelSongInfoInput()
    {
        pendingSourcePath = "";
        HideSongMetaInputPanel();

        Debug.Log("Song info input canceled.");
    }

    private void ImportSongFile(string sourcePath, string songName, string artistName, float bpm)
    {
        string safeSongName = MakeSafeFileName(songName);
        string extension = Path.GetExtension(sourcePath).ToLower();

        string rootPath = GetEditorSongRootPath();
        string songFolderPath = Path.Combine(rootPath, safeSongName);

        Directory.CreateDirectory(songFolderPath);

        string audioFileName = safeSongName + extension;
        string savedAudioPath = Path.Combine(songFolderPath, audioFileName);

        File.Copy(sourcePath, savedAudioPath, true);

        currentSongData = new EditorLoadedSongData
        {
            songName = songName,
            artistName = artistName,
            bpm = bpm,
            audioFileName = audioFileName,
            audioLocalPath = savedAudioPath,
            selectedDifficultyIndex = songInfoUI != null ? songInfoUI.CurrentDifficultyIndex : 0,
            selectedDifficultyName = songInfoUI != null ? songInfoUI.CurrentDifficultyName : "Easy",
            importedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        SaveSongInfoJson(songFolderPath, currentSongData);

        if (songInfoUI != null)
        {
            songInfoUI.SetSongInfo(
                currentSongData.songName,
                currentSongData.artistName,
                currentSongData.bpm
            );
        }

        Debug.Log("Song loaded: " + savedAudioPath);
    }

    private void SaveSongInfoJson(string songFolderPath, EditorLoadedSongData songData)
    {
        string jsonPath = Path.Combine(songFolderPath, "song_info.json");
        string json = JsonUtility.ToJson(songData, true);

        File.WriteAllText(jsonPath, json);

        Debug.Log("Song info saved: " + jsonPath);
    }

    private bool IsSupportedAudioFile(string path)
    {
        string extension = Path.GetExtension(path).ToLower();

        return extension == ".mp3" ||
               extension == ".wav" ||
               extension == ".ogg";
    }

    private string GetEditorSongRootPath()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        return Path.Combine(
            documentsPath,
            "My Games",
            "RythmGame",
            "EditorSongs"
        );
    }

    private string MakeSafeFileName(string fileName)
    {
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidChar, '_');
        }

        fileName = fileName.Trim();

        if (string.IsNullOrEmpty(fileName))
            fileName = "NewSong";

        return fileName;
    }

    private void SetError(string message)
    {
        if (errorText != null)
            errorText.text = message;
    }
}

[Serializable]
public class EditorLoadedSongData
{
    public string songName;
    public string artistName;
    public float bpm;

    public string audioFileName;
    public string audioLocalPath;

    public int selectedDifficultyIndex;
    public string selectedDifficultyName;

    public string importedAt;
}