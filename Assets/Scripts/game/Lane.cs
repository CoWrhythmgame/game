using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class Lane : MonoBehaviour
{
    public int laneIndex;
    public GameObject keyBeam;
    public GameObject keyBomb;
    [SerializeField]private InputActionProperty inputActions;

    void OnEnable()
    {
        Debug.Log("Lane OnEnable");
        inputActions.action.Enable();
        inputActions.action.performed += OnLanePerformed;
    }
    void OnDisable()
    {   
        Debug.Log("Lane OnDisable");
        inputActions.action.Disable();
        inputActions.action.performed -= OnLanePerformed;
    }
    private void OnLanePerformed(InputAction.CallbackContext context)
    {
        // 1. 현재 입력을 발생시킨 구체적인 컨트롤(키) 정보를 가져옵니다.
        var control = context.control;

        // 2. 이 컨트롤이 'Hit' 액션의 몇 번째 바인딩인지 인덱스를 찾습니다.
        int laneindex = -1;
        var bindings = context.action.bindings;
        
        for (int i = 0; i < bindings.Count; i++)
        {
            // 바인딩 경로가 현재 누른 키의 경로와 일치하는지 확인
            if (context.action.controls[i] == control)
            {
                laneindex = i;
                break;
            }
        }

        // 3. 찾은 레인 인덱스로 판정 및 처리 진행
        if (laneindex == laneIndex)
        {
            if(context.ReadValueAsButton())
            {
                Debug.Log($"[레인 {laneindex + 1}] 입력 감지! 타임스탬프: {context.time}");
                OnLanePressed(context);
            }
            else
            {
                Debug.Log($"[레인 {laneindex + 1}] 입력 해제! 타임스탬프: {context.time}");
                OnLaneReleased(context);
            }
        }
    }
    public void OnLanePressed(InputAction.CallbackContext context)
    {
        keyBeam.SetActive(true);
        // 여기에 레인 입력 시 발생할 이벤트를 추가합니다.
    }
    public void OnLaneReleased(InputAction.CallbackContext context)
    {
        keyBeam.SetActive(false);
    }
}
