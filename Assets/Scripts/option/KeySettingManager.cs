using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class KeySettingManager : MonoBehaviour
{
    [Header("Input Action")]
    [SerializeField] private InputActionReference laneActionReference;

    private readonly string[] defaultLaneKeyPaths =
    {
        "<Keyboard>/a",
        "<Keyboard>/s",
        "<Keyboard>/semicolon",
        "<Keyboard>/quote"
    };

    private readonly string[] laneKeyPaths = new string[4];

    private int waitingLaneIndex = -1;
    private int waitingStartFrame = -1;

    public bool IsWaitingForKey => waitingLaneIndex >= 0;
    public int WaitingLaneIndex => waitingLaneIndex;

    private void Awake()
    {
        LoadLaneKeys();
        ApplyAllLaneKeys();
    }

    private void Update()
    {
        if (!IsWaitingForKey)
            return;

        ListenForKeyInput();
    }

    public void BeginChangeLaneKey(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= laneKeyPaths.Length)
            return;

        waitingLaneIndex = laneIndex;
        waitingStartFrame = Time.frameCount;

        Debug.Log($"Lane {laneIndex + 1} 키 입력 대기 중...");
    }

    public void CancelChangeLaneKey()
    {
        waitingLaneIndex = -1;
        waitingStartFrame = -1;
    }

    public string GetLaneKeyDisplayName(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= laneKeyPaths.Length)
            return "-";

        if (IsWaitingForKey && WaitingLaneIndex == laneIndex)
            return "Press Key...";

        return InputControlPath.ToHumanReadableString(
            laneKeyPaths[laneIndex],
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
    }

    private void ListenForKeyInput()
    {
        if (Keyboard.current == null)
            return;

        //  키 변경을 시작한 바로 그 프레임의 입력은 무시
        if (Time.frameCount == waitingStartFrame)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelChangeLaneKey();
            return;
        }

        foreach (KeyControl key in Keyboard.current.allKeys)
        {
            if (!key.wasPressedThisFrame)
                continue;

            string newPath = "<Keyboard>/" + key.name;

            if (IsDuplicateKey(newPath, waitingLaneIndex))
            {
                Debug.LogWarning("이미 다른 레인에서 사용 중인 키입니다.");
                CancelChangeLaneKey();
                return;
            }

            SetLaneKey(waitingLaneIndex, newPath);
            CancelChangeLaneKey();
            return;
        }
    }

    private bool IsDuplicateKey(string keyPath, int exceptLaneIndex)
    {
        for (int i = 0; i < laneKeyPaths.Length; i++)
        {
            if (i == exceptLaneIndex)
                continue;

            if (laneKeyPaths[i] == keyPath)
                return true;
        }

        return false;
    }

    private void SetLaneKey(int laneIndex, string keyPath)
    {
        if (laneIndex < 0 || laneIndex >= laneKeyPaths.Length)
            return;

        laneKeyPaths[laneIndex] = keyPath;

        ApplyLaneKey(laneIndex);
        SaveLaneKeys();

        Debug.Log($"Lane {laneIndex + 1} Key Changed: {keyPath}");
    }

    private void ApplyAllLaneKeys()
    {
        for (int i = 0; i < laneKeyPaths.Length; i++)
        {
            ApplyLaneKey(i);
        }
    }

    private void ApplyLaneKey(int laneIndex)
    {
        if (laneActionReference == null || laneActionReference.action == null)
        {
            Debug.LogWarning("KeySettingManager: Lane Action Reference가 연결되지 않았습니다.");
            return;
        }

        InputAction laneAction = laneActionReference.action;
        int bindingIndex = GetKeyboardBindingIndex(laneAction, laneIndex);

        if (bindingIndex < 0)
        {
            Debug.LogWarning($"KeySettingManager: Lane {laneIndex + 1}에 해당하는 Keyboard Binding을 찾지 못했습니다.");
            return;
        }

        bool wasEnabled = laneAction.enabled;

        if (wasEnabled)
            laneAction.Disable();

        laneAction.ApplyBindingOverride(bindingIndex, laneKeyPaths[laneIndex]);

        if (wasEnabled)
            laneAction.Enable();
    }

    private int GetKeyboardBindingIndex(InputAction action, int laneIndex)
    {
        int keyboardBindingCount = 0;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];

            if (binding.isComposite || binding.isPartOfComposite)
                continue;

            string path = string.IsNullOrEmpty(binding.effectivePath)
                ? binding.path
                : binding.effectivePath;

            if (string.IsNullOrEmpty(path))
                continue;

            if (!path.StartsWith("<Keyboard>"))
                continue;

            if (keyboardBindingCount == laneIndex)
                return i;

            keyboardBindingCount++;
        }

        return -1;
    }

    private void LoadLaneKeys()
    {
        for (int i = 0; i < laneKeyPaths.Length; i++)
        {
            laneKeyPaths[i] = PlayerPrefs.GetString(
                $"Option_LaneKey_{i}",
                defaultLaneKeyPaths[i]
            );
        }
    }

    private void SaveLaneKeys()
    {
        for (int i = 0; i < laneKeyPaths.Length; i++)
        {
            PlayerPrefs.SetString($"Option_LaneKey_{i}", laneKeyPaths[i]);
        }

        PlayerPrefs.Save();
    }

    public void ResetDefaultKeys()
    {
        for (int i = 0; i < laneKeyPaths.Length; i++)
        {
            laneKeyPaths[i] = defaultLaneKeyPaths[i];
        }

        ApplyAllLaneKeys();
        SaveLaneKeys();
    }
}