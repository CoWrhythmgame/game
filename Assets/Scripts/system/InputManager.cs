using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // 어디서나 접근할 수 있는 전역 인스턴스
    public static InputManager Instance { get; private set; }
    [SerializeField]private InputActionAsset inputActions;
    private InputActionMap inputActions_UI;
    private InputActionMap inputActions_Game;
    void Awake()
    {
        // 1. 이미 인스턴스가 존재하는데, 그게 내가 아니라면? (중복 생성된 경우)
        if (Instance != null && Instance != this)
        {
            // 나 자신을 파괴하여 중복 생성을 막음
            Destroy(gameObject);
            return;
        }

        // 2. 내가 최초로 생성된 매니저라면 인스턴스로 등록
        Instance = this;

        // 3. 씬이 바뀌어도 파괴되지 않도록 설정 (최상위 오브젝트여야 작동함)
        DontDestroyOnLoad(gameObject);
        
        InitializeInputActions();
        //인게임 테스트용
        EnableGamePlay();
    }

    private void InitializeInputActions()
    {
        inputActions_UI = inputActions.FindActionMap("UI");
        inputActions_Game = inputActions.FindActionMap("GamePlay");
        Debug.Log($"Input Actions Initialized: UI Map - {inputActions_UI.name}, Game Map - {inputActions_Game.name}");
    }

    private void EnableUI()
    {
        inputActions_Game.Disable();
        inputActions_UI.Enable();
    }
    private void EnableGamePlay()
    {
        inputActions_Game.Enable();
        inputActions_Game.FindAction("Lane").Enable();
        inputActions_UI.Disable();
    }
    public void OnPause()
    {
        EnableUI();
    }
    public void OnResume()
    {
        EnableGamePlay();
    }
}
