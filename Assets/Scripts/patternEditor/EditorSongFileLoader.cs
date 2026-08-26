using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorSongFileLoader : MonoBehaviour
{
    private enum SongMetaInputMode
    {
        None,
        ImportNewSong,
        EditCurrentSong
    }

    [Header("Main UI")]
    [SerializeField] private EditorSongInfoUI songInfoUI;
    [SerializeField] private Button loadSongButton;
    [SerializeField] private Button editInfoButton;

    [Header("Song Meta Input Panel")]
    [SerializeField] private GameObject songMetaInputPanel;
    [SerializeField] private TMP_InputField songNameInput;
    [SerializeField] private TMP_InputField artistInput;
    [SerializeField] private TMP_InputField bpmInput;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Image _jacketImage;
    [SerializeField] private Button _jacketLoadButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Default Values")]
    [SerializeField] private string defaultArtistName = "Unknown Artist";
    [SerializeField] private Sprite _defaultJacket;

    private string pendingSourcePath = "";
    private SongMetaInputMode inputMode = SongMetaInputMode.None;

    private EditorLoadedSongData currentSongData;
    private byte[] _jacketImageData;
    
    public bool IsSongLoaded()
    {
        return currentSongData != null;
    }

    public EditorLoadedSongData GetCurrentSongData()
    {
        return currentSongData;
    }

    public bool TryGetCurrentSongData(out EditorLoadedSongData songData)
    {
        songData = currentSongData;
        return songData != null;
    }

    public float GetCurrentBpm()
    {
        if (currentSongData == null)
            return 0f;

        return currentSongData.bpm;
    }

    public string GetCurrentSongName()
    {
        if (currentSongData == null)
            return "";

        return currentSongData.songName;
    }

    public string GetCurrentArtistName()
    {
        if (currentSongData == null)
            return "";

        return currentSongData.artistName;
    }

    public string GetCurrentAudioPath()
    {
        if (currentSongData == null)
            return "";

        return currentSongData.audioLocalPath;
    }
    public byte[] GetJacketImageData()
    {
        return _jacketImageData;
    }
    public bool HasLoadedSong => currentSongData != null;

    public event Action<EditorLoadedSongData> OnSongLoadedOrUpdated;
    public event Action<EditorLoadedSongData> OnSongMetaUpdated;
    private void Awake()
    {
        if (loadSongButton != null)
            loadSongButton.onClick.AddListener(RequestOpenSongFile);

        if (editInfoButton != null)
            editInfoButton.onClick.AddListener(OpenEditSongInfoPanel);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmSongInfo);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelSongInfoInput);
        if(_jacketLoadButton != null)
            _jacketLoadButton.onClick.AddListener(RequestJacketFile);

        HideSongMetaInputPanel();
        SetEditButtonActive(false);
    }

    private void OnDestroy()
    {
        if (loadSongButton != null)
            loadSongButton.onClick.RemoveListener(RequestOpenSongFile);

        if (editInfoButton != null)
            editInfoButton.onClick.RemoveListener(OpenEditSongInfoPanel);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(ConfirmSongInfo);

        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(CancelSongInfoInput);
    }

    public event Action OnSongFileOpenRequested;
    public void RequestOpenSongFile()
    {
        OnSongFileOpenRequested?.Invoke();
    }
    public void LoadSongFromPath(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            Debug.LogWarning("Song loading canceled or path is empty.");
            return;
        }

        if (!File.Exists(sourcePath))
        {
            Debug.LogWarning("Audio file does not exist: " + sourcePath);
            return;
        }

        if (!IsSupportedAudioFile(sourcePath))
        {
            Debug.LogWarning("Unsupported audio file. Only mp3, wav, and ogg files are allowed.");
            return;
        }

        pendingSourcePath = sourcePath;
        inputMode = SongMetaInputMode.ImportNewSong;

        ShowSongMetaInputPanelForImport(sourcePath);
    }
    public void OpenEditSongInfoPanel()
    {
        if (currentSongData == null)
        {
            SetError("No song loaded.");
            return;
        }

        pendingSourcePath = "";
        inputMode = SongMetaInputMode.EditCurrentSong;


        ShowSongMetaInputPanelForEdit();
    }

    private void ShowSongMetaInputPanelForImport(string sourcePath)
    {
        if (songMetaInputPanel != null)
            songMetaInputPanel.SetActive(true);

        EditorInputBlocker.SetBlocked(true);
        
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);
        SetJacketImage(null);
        _jacketImageData = null;

        if (songNameInput != null)
            songNameInput.text = fileNameWithoutExtension;

        if (artistInput != null)
            artistInput.text = defaultArtistName;

        if (bpmInput != null)
            bpmInput.text = "";

        SetError("");
    }

    private void ShowSongMetaInputPanelForEdit()
    {
        if (songMetaInputPanel != null)
            songMetaInputPanel.SetActive(true);

        EditorInputBlocker.SetBlocked(true);
        Sprite jaket = FileManager.LoadJacket(currentSongData.songName, false);
        _jacketImageData = FileManager.LoadJacketData(currentSongData.songName, false);
        SetJacketImage(jaket);

        if (songNameInput != null)
            songNameInput.text = currentSongData.songName;

        if (artistInput != null)
            artistInput.text = currentSongData.artistName;

        if (bpmInput != null)
            bpmInput.text = currentSongData.bpm.ToString("0.##");

        SetError("");
    }

    private void HideSongMetaInputPanel()
    {
        if (songMetaInputPanel != null)
            songMetaInputPanel.SetActive(false);

        EditorInputBlocker.SetBlocked(false);

        SetError("");
    }

    private void ConfirmSongInfo()
    {
        string songName = songNameInput != null ? songNameInput.text.Trim() : "";
        string artistName = artistInput != null ? artistInput.text.Trim() : "";
        string bpmString = bpmInput != null ? bpmInput.text.Trim() : "";

        if (!ValidateSongMetaInput(songName, artistName, bpmString, out float bpm))
            return;

        if (string.IsNullOrEmpty(artistName))
            artistName = defaultArtistName;

        switch (inputMode)
        {
            case SongMetaInputMode.ImportNewSong:
                ConfirmImportSong(songName, artistName, bpm);
                break;

            case SongMetaInputMode.EditCurrentSong:
                ConfirmEditSongInfo(songName, artistName, bpm);
                break;

            default:
                SetError("Invalid input mode.");
                break;
        }
    }

    private bool ValidateSongMetaInput(string songName, string artistName, string bpmString, out float bpm)
    {
        bpm = 0f;

        if (string.IsNullOrEmpty(songName))
        {
            SetError("Please enter a song name.");
            return false;
        }

        if (!float.TryParse(bpmString, out bpm))
        {
            SetError("Please enter BPM as a number.");
            return false;
        }

        if (bpm <= 0f)
        {
            SetError("BPM must be greater than 0.");
            return false;
        }

        return true;
    }

    private void ConfirmImportSong(string songName, string artistName, float bpm)
    {
        if (string.IsNullOrEmpty(pendingSourcePath))
        {
            SetError("No audio file selected.");
            return;
        }

        ImportSongFile(pendingSourcePath, songName, artistName, bpm);

        pendingSourcePath = "";
        inputMode = SongMetaInputMode.None;
        HideSongMetaInputPanel();
    }

    private void ConfirmEditSongInfo(string songName, string artistName, float bpm)
    {
        if (currentSongData == null)
        {
            SetError("No song loaded.");
            return;
        }

        currentSongData.songName = songName;
        currentSongData.artistName = artistName;
        currentSongData.bpm = bpm;
        currentSongData.selectedDifficultyIndex = songInfoUI != null ? songInfoUI.CurrentDifficultyIndex : 0;
        currentSongData.selectedDifficultyName = songInfoUI != null ? songInfoUI.CurrentDifficultyName : "Easy";

        SaveCurrentSongInfoJson();
        FileManager.SaveJacket(songName, false, _jacketImageData);

        if (songInfoUI != null)
        {
            songInfoUI.SetSongInfo(
                currentSongData.songName,
                currentSongData.artistName,
                currentSongData.bpm
            );
        }

        OnSongMetaUpdated?.Invoke(currentSongData);

        inputMode = SongMetaInputMode.None;
        HideSongMetaInputPanel();

        Debug.Log("Song info updated.");
    }

    private void CancelSongInfoInput()
    {
        pendingSourcePath = "";
        inputMode = SongMetaInputMode.None;

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
        FileManager.SaveJacket(songName, false, _jacketImageData);

        currentSongData = new EditorLoadedSongData
        {
            songName = songName,
            artistName = artistName,
            bpm = bpm,

            audioFileName = audioFileName,
            audioLocalPath = savedAudioPath,
            songFolderPath = songFolderPath,

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

        SetEditButtonActive(true);

        OnSongLoadedOrUpdated?.Invoke(currentSongData);
        Debug.Log("Song loaded: " + savedAudioPath);
    }

    private void SaveCurrentSongInfoJson()
    {
        if (currentSongData == null)
            return;

        string folderPath = currentSongData.songFolderPath;

        if (string.IsNullOrEmpty(folderPath))
            folderPath = Path.GetDirectoryName(currentSongData.audioLocalPath);

        SaveSongInfoJson(folderPath, currentSongData);
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
            "Songs"
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

    private void SetEditButtonActive(bool isActive)
    {
        if (editInfoButton != null)
            editInfoButton.interactable = isActive;
    }

    private void SetError(string message)
    {
        if (errorText != null)
            errorText.text = message;
    }

    public void SetCurrentSongDataFromImport(Song song)
    {
        if (song == null)
        {
            Debug.LogWarning("Import song data is null.");
            return;
        }

        if (TryLoadExistingEditorSongByName(song.songname, out EditorLoadedSongData loadedData))
        {
            currentSongData.songName = song.songname;
            currentSongData.artistName = song.artist;
            currentSongData.bpm = song.bpm;

            if (songInfoUI != null)
            {
                songInfoUI.SetSongInfo(
                    currentSongData.songName,
                    currentSongData.artistName,
                    currentSongData.bpm
                );
            }

            OnSongLoadedOrUpdated?.Invoke(currentSongData);

            Debug.Log("Current song data set from import with audio: " + currentSongData.songName);
            return;
        }

        currentSongData = new EditorLoadedSongData
        {
            songName = song.songname,
            artistName = song.artist,
            bpm = song.bpm,

            selectedDifficultyIndex = songInfoUI != null ? songInfoUI.CurrentDifficultyIndex : 0,
            selectedDifficultyName = songInfoUI != null ? songInfoUI.CurrentDifficultyName : "Easy",

            importedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        SetEditButtonActive(true);

        OnSongLoadedOrUpdated?.Invoke(currentSongData);

        Debug.LogWarning("Current song data set from import, but audio path was not found: " + currentSongData.songName);
    }
    public bool TryLoadExistingEditorSongByName(string songName, out EditorLoadedSongData loadedData)
    {
        loadedData = null;

        if (string.IsNullOrWhiteSpace(songName))
        {
            Debug.LogWarning("Song name is empty.");
            return false;
        }

        string safeSongName = MakeSafeFileName(songName);
        string songFolderPath = Path.Combine(GetEditorSongRootPath(), safeSongName);
        string jsonPath = Path.Combine(songFolderPath, "song_info.json");

        if (!File.Exists(jsonPath))
        {
            Debug.LogWarning("Editor song_info.json�� ã�� ���߽��ϴ�: " + jsonPath);
            return false;
        }

        string json = File.ReadAllText(jsonPath);
        loadedData = JsonUtility.FromJson<EditorLoadedSongData>(json);

        if (loadedData == null)
        {
            Debug.LogWarning("Editor song_info.json �Ľ� ����: " + jsonPath);
            return false;
        }

        loadedData.songFolderPath = songFolderPath;

        if (string.IsNullOrEmpty(loadedData.audioLocalPath))
        {
            if (!string.IsNullOrEmpty(loadedData.audioFileName))
            {
                loadedData.audioLocalPath = Path.Combine(songFolderPath, loadedData.audioFileName);
            }
            else
            {
                string[] audioFiles = Directory.GetFiles(songFolderPath);

                foreach (string file in audioFiles)
                {
                    if (IsSupportedAudioFile(file))
                    {
                        loadedData.audioLocalPath = file;
                        loadedData.audioFileName = Path.GetFileName(file);
                        break;
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(loadedData.audioLocalPath) || !File.Exists(loadedData.audioLocalPath))
        {
            Debug.LogWarning("����� ������ ã�� ���߽��ϴ�: " + songFolderPath);
            return false;
        }

        currentSongData = loadedData;

        if (songInfoUI != null)
        {
            songInfoUI.SetSongInfo(
                currentSongData.songName,
                currentSongData.artistName,
                currentSongData.bpm
            );
        }

        SetEditButtonActive(true);

        OnSongLoadedOrUpdated?.Invoke(currentSongData);

        Debug.Log("Existing editor song loaded: " + currentSongData.songName);
        return true;
    }
    private void RequestJacketFile()
    {
        string path = FileManager.OpenJaketFileBrowser();
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Jacket image loading canceled.");
            return;
        }
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            _jacketImage.sprite = null;
            _jacketImage.color = Color.clear;
            return;
        }

        byte[] imageData = File.ReadAllBytes(path);
        _jacketImageData = imageData;
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        SetJacketImage(sprite);
    }
    private void SetJacketImage(Sprite sprite)
    {
        if (_jacketImage == null)
            return;
        if (sprite == null)
        {
            _jacketImage.sprite = _defaultJacket;
        }
        else{
            _jacketImage.sprite = sprite;
        }
        _jacketImage.color = Color.white;
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
    public string songFolderPath;

    public int selectedDifficultyIndex;
    public string selectedDifficultyName;

    public string importedAt;
}