using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public class Lane : MonoBehaviour
{
    public int laneIndex;
    public GameObject keyBeam;
    public GameObject keyBomb;
    [SerializeField]private InputActionProperty inputActions;
    [SerializeField]private JudgementManager judgementManager;
    public float Scrollspeed = 1f;
    void Awake()
    {
        judgementManager = GameObject.FindGameObjectWithTag("JudgementManager").GetComponent<JudgementManager>();
    }
    void OnEnable()
    {
        inputActions.action.Enable();
        inputActions.action.performed += OnLanePerformed;
    }
    void OnDisable()
    {   
        inputActions.action.Disable();
        inputActions.action.performed -= OnLanePerformed;
    }
    
    //키 입력 감지시 호출
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
                OnLanePressed(context);
            }
            else
            {
                OnLaneReleased(context);
            }
        }
    }
    //눌렀을 때
    public void OnLanePressed(InputAction.CallbackContext context)
    {
        keyBeam.SetActive(true);
        judgementManager.OnLaneInputFired(laneIndex, context.time);
    }
    //땠을 때
    public void OnLaneReleased(InputAction.CallbackContext context)
    {
        keyBeam.SetActive(false);
        //롱노트 관련
    }
}
