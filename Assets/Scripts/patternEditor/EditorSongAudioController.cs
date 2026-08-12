using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;
public class EditorSongAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeasureList _measureList;
    [SerializeField] private SEManager _SEManager;
    [SerializeField] private EditorSongFileLoader songFileLoader;
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup musicMixerGroup;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button frontButton;
    [SerializeField] private Button saveButton;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI playButtonText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Settings")]
    [SerializeField] private float seekSeconds = 5f;

    private Queue<Note> _currentPattern;
    private bool isLoadingClip = false;
    

    public AudioSource AudioSource => audioSource;

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    public float CurrentTime
    {
        get
        {
            if (audioSource == null)
                return 0f;

            return audioSource.time;
        }
    }
    private void Awake()
    {
        SetupAudioSource();
        RegisterButtons();

        SetControlButtonsActive(false);
        SetPlayButtonText("Play");
        RefreshTimeText();
    }

    private void OnEnable()
    {
        if (songFileLoader != null)
            songFileLoader.OnSongLoadedOrUpdated += OnSongLoadedOrUpdated;
    }

    private void OnDisable()
    {
        if (songFileLoader != null)
            songFileLoader.OnSongLoadedOrUpdated -= OnSongLoadedOrUpdated;
    }

    private void OnDestroy()
    {
        UnregisterButtons();
    }

    private void Update()
    {
        RefreshTimeText();

        if (!HasAudioClip())
            return;

        if (!audioSource.isPlaying && audioSource.time >= audioSource.clip.length)
        {
            StopSongInternal();
        }
        //* 키음 재생
        if (audioSource.isPlaying)
        {
            if(_currentPattern.Count != 0){
                if(_currentPattern.Peek().time <= audioSource.time)
                {
                    _currentPattern.Dequeue();
                    _SEManager.PlayHitSound();  
                }
            }
        }
    }

    private void SetupAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        if (musicMixerGroup != null)
            audioSource.outputAudioMixerGroup = musicMixerGroup;

        ApplySavedAudioOptions();
    }

    private void RegisterButtons()
    {
        if (playButton != null)
            playButton.onClick.AddListener(TogglePlayPause);

        if (stopButton != null)
            stopButton.onClick.AddListener(StopSong);

        if (backButton != null)
            backButton.onClick.AddListener(Back5Seconds);

        if (frontButton != null)
            frontButton.onClick.AddListener(Forward5Seconds);

        //if (saveButton != null)
        //    saveButton.onClick.AddListener(SaveChartPlaceholder);
    }

    private void UnregisterButtons()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(TogglePlayPause);

        if (stopButton != null)
            stopButton.onClick.RemoveListener(StopSong);

        if (backButton != null)
            backButton.onClick.RemoveListener(Back5Seconds);

        if (frontButton != null)
            frontButton.onClick.RemoveListener(Forward5Seconds);

        //if (saveButton != null)
        //    saveButton.onClick.RemoveListener(SaveChartPlaceholder);
    }

    private void OnSongLoadedOrUpdated(EditorLoadedSongData songData)
    {
        if (songData == null)
            return;

        if (string.IsNullOrEmpty(songData.audioLocalPath))
            return;

        StopSongInternal();

        SetControlButtonsActive(false);
        SetPlayButtonText("Loading...");
        RefreshTimeText();

        StartCoroutine(LoadAudioClip(songData.audioLocalPath));
    }

    private IEnumerator LoadAudioClip(string audioPath)
    {
        if (isLoadingClip)
            yield break;

        isLoadingClip = true;

        string uri = GetFileUri(audioPath);
        AudioType audioType = GetAudioType(audioPath);

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Failed to load audio clip: " + request.error);

                if (audioSource != null)
                    audioSource.clip = null;

                SetControlButtonsActive(false);
                SetPlayButtonText("Play");
                RefreshTimeText();

                isLoadingClip = false;
                yield break;
            }

            AudioClip loadedClip = DownloadHandlerAudioClip.GetContent(request);

            audioSource.clip = loadedClip;
            audioSource.time = 0f;

            SetControlButtonsActive(true);
            SetPlayButtonText("Play");
            RefreshTimeText();

            Debug.Log("Audio clip loaded: " + audioPath);
        }

        isLoadingClip = false;
    }

    private void TogglePlayPause()
    {
        if (!HasAudioClip())
        {
            Debug.LogWarning("No audio clip loaded.");
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            SetPlayButtonText("Play");
            return;
        }

        if (audioSource.time >= audioSource.clip.length)
            audioSource.time = 0f;

        _currentPattern = new Queue<Note>(_measureList.GetPattern().notes.OrderBy(x => x.time));

        audioSource.Play();
        SetPlayButtonText("Pause");
    }

    private void StopSong()
    {
        if (!HasAudioClip())
            return;

        StopSongInternal();
    }

    private void StopSongInternal()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
        audioSource.time = 0f;

        SetPlayButtonText("Play");
        RefreshTimeText();
    }

    private void Back5Seconds()
    {
        if (!HasAudioClip())
            return;

        audioSource.time = Mathf.Max(0f, audioSource.time - seekSeconds);
        RefreshTimeText();
    }

    private void Forward5Seconds()
    {
        if (!HasAudioClip())
            return;

        audioSource.time = Mathf.Min(audioSource.clip.length, audioSource.time + seekSeconds);

        if (audioSource.time >= audioSource.clip.length)
        {
            StopSongInternal();
            return;
        }

        RefreshTimeText();
    }

    /*private void SaveChartPlaceholder()
    {
        Debug.Log("Save button clicked. Chart saving is not implemented yet.");
    }*/

    private bool HasAudioClip()
    {
        return audioSource != null && audioSource.clip != null;
    }

    private void SetControlButtonsActive(bool isActive)
    {
        if (playButton != null)
            playButton.interactable = isActive;

        if (stopButton != null)
            stopButton.interactable = isActive;

        if (backButton != null)
            backButton.interactable = isActive;

        if (frontButton != null)
            frontButton.interactable = isActive;

        if (saveButton != null)
            saveButton.interactable = isActive;
    }

    private void SetPlayButtonText(string text)
    {
        if (playButtonText != null)
            playButtonText.text = text;
    }

    private void RefreshTimeText()
    {
        if (timeText == null)
            return;

        if (!HasAudioClip())
        {
            timeText.text = "00:00 / 00:00";
            return;
        }

        float currentTime = audioSource.time;
        float totalTime = audioSource.clip.length;

        timeText.text = FormatTime(currentTime) + " / " + FormatTime(totalTime);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private string GetFileUri(string path)
    {
        return "file:///" + path.Replace("\\", "/");
    }

    private AudioType GetAudioType(string path)
    {
        string extension = Path.GetExtension(path).ToLower();

        switch (extension)
        {
            case ".wav":
                return AudioType.WAV;

            case ".ogg":
                return AudioType.OGGVORBIS;

            case ".mp3":
                return AudioType.MPEG;

            default:
                return AudioType.UNKNOWN;
        }
    }

    private void ApplySavedAudioOptions()
    {
        int masterVolume = PlayerPrefs.GetInt("Option_MasterVolume", 100);
        int musicVolume = PlayerPrefs.GetInt("Option_MusicVolume", 100);
        int sfxVolume = PlayerPrefs.GetInt("Option_SoundEffectVolume", 100);
        int keyVolume = PlayerPrefs.GetInt("Option_KeyVolume", 100);

        if (audioMixer != null)
        {
            SetMixerVolume("MasterVolume", masterVolume);
            SetMixerVolume("MusicVolume", musicVolume);
            SetMixerVolume("SFXVolume", sfxVolume);
            SetMixerVolume("KeySoundVolume", keyVolume);
        }
        else
        {
            // Mixer가 연결 안 됐을 때 최소한 MusicVolume만 AudioSource에 적용
            if (audioSource != null)
                audioSource.volume = Mathf.Clamp01(musicVolume / 100f);
        }
    }

    private void SetMixerVolume(string parameterName, int volume)
    {
        if (audioMixer == null)
            return;

        float dbValue = VolumeToDb(volume);
        bool success = audioMixer.SetFloat(parameterName, dbValue);

        if (!success)
            Debug.LogWarning("AudioMixer parameter not found: " + parameterName);
    }

    private float VolumeToDb(int volume)
    {
        if (volume <= 0)
            return -80f;

        return Mathf.Log10(volume / 100f) * 20f;
    }
}