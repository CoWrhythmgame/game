using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
public class OptionMenuUI : MonoBehaviour
{
    public static bool IsOptionOpen { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject optionPanel;

    [Header("Option Name Texts")]
    [SerializeField] private TextMeshProUGUI[] nameTexts;

    [Header("Option Value Texts")]
    [SerializeField] private TextMeshProUGUI[] valueTexts;

    [Header("Selected Option Colors")]
    [SerializeField] private Color normalOptionTextColor = Color.white;
    [SerializeField] private Color selectedOptionTextColor = Color.yellow;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private int currentIndex = 0;

    private readonly string[] optionNames =
    {
        "Full Scene",
        "Frame",
        "Master Volume",
        "Music Volume",
        "Sound Effect",
        "Key Volume",
        "Scroll Speed",
        "Note Offset"
    };

    private bool isFullScreen = true;

    private int frameIndex = 1;
    private readonly int[] frameValues = { 30, 60, 120, 144, -1 };
    private readonly string[] frameTexts = { "30", "60", "120", "144", "Unlimited" };

    private int masterVolume = 100;
    private int musicVolume = 100;
    private int soundEffectVolume = 100;
    private int keyVolume = 100;

    private float scrollSpeed = 1.0f;
    private float noteOffset = 0.0f;
    private float VolumeToDb(int volume)
    {
        if (volume <= 0)
            return -80f;

        return Mathf.Log10(volume / 100f) * 20f;
    }
    private void Awake()
    {
        LoadOptions();
        ApplyOptions();
    }

    private void Start()
    {
        CloseOption();
        RefreshUI();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            ToggleOption();
            return;
        }

        if (!IsOptionOpen)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseOption();
            return;
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            MoveCursor(-1);
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            MoveCursor(1);
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            ChangeValue(-1);
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            ChangeValue(1);
        }
    }

    private void ToggleOption()
    {
        if (IsOptionOpen)
            CloseOption();
        else
            OpenOption();
    }

    private void OpenOption()
    {
        IsOptionOpen = true;
        currentIndex = 0;

        if (optionPanel != null)
            optionPanel.SetActive(true);

        RefreshUI();
    }

    private void CloseOption()
    {
        IsOptionOpen = false;

        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    private void MoveCursor(int direction)
    {
        if (nameTexts == null || nameTexts.Length == 0)
            return;

        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = nameTexts.Length - 1;
        else if (currentIndex >= nameTexts.Length)
            currentIndex = 0;

        RefreshUI();
    }

    private void ChangeValue(int direction)
    {
        switch (currentIndex)
        {
            case 0: // Full Scene
                isFullScreen = !isFullScreen;
                break;

            case 1: // Frame
                frameIndex += direction;

                if (frameIndex < 0)
                    frameIndex = frameValues.Length - 1;
                else if (frameIndex >= frameValues.Length)
                    frameIndex = 0;

                break;

            case 2: // Master Volume
                masterVolume = Mathf.Clamp(masterVolume + direction * 5, 0, 100);
                break;

            case 3: // Music Volume
                musicVolume = Mathf.Clamp(musicVolume + direction * 5, 0, 100);
                break;

            case 4: // Sound Effect
                soundEffectVolume = Mathf.Clamp(soundEffectVolume + direction * 5, 0, 100);
                break;

            case 5: // Key Volume
                keyVolume = Mathf.Clamp(keyVolume + direction * 5, 0, 100);
                break;

            case 6: // Scroll Speed
                scrollSpeed += direction * 0.1f;
                scrollSpeed = Mathf.Clamp(scrollSpeed, 0.1f, 10.0f);
                scrollSpeed = Mathf.Round(scrollSpeed * 10f) / 10f;
                break;

            case 7: // Note Offset
                noteOffset += direction * 0.1f;
                noteOffset = Mathf.Clamp(noteOffset, -5.0f, 5.0f);
                noteOffset = Mathf.Round(noteOffset * 10f) / 10f;
                break;
        }

        ApplyOptions();
        SaveOptions();
        RefreshUI();
    }
    private void SetMixerVolume(string parameterName, int volume)
    {
        float dbValue = VolumeToDb(volume);
        bool success = audioMixer.SetFloat(parameterName, dbValue);

        if (!success)
        {
            Debug.LogWarning("AudioMixer parameter not found: " + parameterName);
            return;
        }

        Debug.Log(parameterName + " = " + volume + " / " + dbValue + " dB");
    }
    private void ApplyOptions()
    {
        Screen.fullScreen = isFullScreen;

        if (frameIndex >= 0 && frameIndex < frameValues.Length)
        {
            Application.targetFrameRate = frameValues[frameIndex];
        }
        if (audioMixer == null)
        {
            Debug.LogWarning("OptionMenuUI: AudioMixer가 연결되지 않았습니다.");
            return;
        }
        SetMixerVolume("MasterVolume", masterVolume);
        SetMixerVolume("MusicVolume", musicVolume);
        SetMixerVolume("SFXVolume", soundEffectVolume);
        SetMixerVolume("KeySoundVolume", keyVolume);
    }

    private void SaveOptions()
    {
        PlayerPrefs.SetInt("Option_IsFullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.SetInt("Option_FrameIndex", frameIndex);

        PlayerPrefs.SetInt("Option_MasterVolume", masterVolume);
        PlayerPrefs.SetInt("Option_MusicVolume", musicVolume);
        PlayerPrefs.SetInt("Option_SoundEffectVolume", soundEffectVolume);
        PlayerPrefs.SetInt("Option_KeyVolume", keyVolume);

        PlayerPrefs.SetFloat("Option_ScrollSpeed", scrollSpeed);
        PlayerPrefs.SetFloat("Option_NoteOffset", noteOffset);

        PlayerPrefs.Save();
    }

    private void LoadOptions()
    {
        isFullScreen = PlayerPrefs.GetInt("Option_IsFullScreen", 1) == 1;
        frameIndex = PlayerPrefs.GetInt("Option_FrameIndex", 1);

        masterVolume = PlayerPrefs.GetInt("Option_MasterVolume", 100);
        musicVolume = PlayerPrefs.GetInt("Option_MusicVolume", 100);
        soundEffectVolume = PlayerPrefs.GetInt("Option_SoundEffectVolume", 100);
        keyVolume = PlayerPrefs.GetInt("Option_KeyVolume", 100);

        scrollSpeed = PlayerPrefs.GetFloat("Option_ScrollSpeed", 1.0f);
        noteOffset = PlayerPrefs.GetFloat("Option_NoteOffset", 0.0f);

        frameIndex = Mathf.Clamp(frameIndex, 0, frameValues.Length - 1);

        masterVolume = Mathf.Clamp(masterVolume, 0, 100);
        musicVolume = Mathf.Clamp(musicVolume, 0, 100);
        soundEffectVolume = Mathf.Clamp(soundEffectVolume, 0, 100);
        keyVolume = Mathf.Clamp(keyVolume, 0, 100);

        scrollSpeed = Mathf.Clamp(scrollSpeed, 0.1f, 10.0f);
        noteOffset = Mathf.Clamp(noteOffset, -5.0f, 5.0f);
    }

    private void RefreshUI()
    {
        RefreshNameTexts();
        RefreshValueTexts();
    }

    private void RefreshNameTexts()
    {
        if (nameTexts == null)
            return;

        for (int i = 0; i < nameTexts.Length; i++)
        {
            if (nameTexts[i] == null)
                continue;

            string optionName = GetOptionName(i);

            nameTexts[i].text = optionName;

            if (i == currentIndex)
                nameTexts[i].color = selectedOptionTextColor;
            else
                nameTexts[i].color = normalOptionTextColor;
        }
    }

    private void RefreshValueTexts()
    {
        if (valueTexts == null)
            return;

        SetValueText(0, isFullScreen ? "ON" : "OFF");
        SetValueText(1, frameTexts[frameIndex]);
        SetValueText(2, masterVolume.ToString());
        SetValueText(3, musicVolume.ToString());
        SetValueText(4, soundEffectVolume.ToString());
        SetValueText(5, keyVolume.ToString());
        SetValueText(6, scrollSpeed.ToString("0.0"));
        SetValueText(7, noteOffset.ToString("0.0"));
    }

    private void SetValueText(int index, string value)
    {
        if (valueTexts == null)
            return;

        if (index < 0 || index >= valueTexts.Length)
            return;

        if (valueTexts[index] == null)
            return;

        valueTexts[index].text = value;
    }

    private string GetOptionName(int index)
    {
        if (index >= 0 && index < optionNames.Length)
            return optionNames[index];

        return "Option " + index;
    }
}