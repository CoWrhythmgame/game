using UnityEngine;
using UnityEngine.InputSystem;

public class AddMeasureButton : MonoBehaviour
{
    [SerializeField] private MeasureList measureList;
    private Transform _transform;
    private SpriteRenderer _spriteRenderer;
    private Color _color;
    private Vector3 _mousePos;
    void Awake()
    {
        _transform = transform.GetComponent<Transform>();
        _spriteRenderer = transform.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        _mousePos = GetMouseWorldPosition();
        if (IsMouseOn())
        {
            OnOverMouse();
        }
        else
        {
            OnOffMouse();
        }
        _spriteRenderer.color = _color;
    }
    private void OnOverMouse()
    {
        _color = new Color(.75f,.75f,.75f,1f);
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            measureList.AddMeasure();
        }
    }
    private void OnOffMouse()
    {
        _color = Color.white;
    }
    private Vector3 GetMouseWorldPosition()
    {   
        // 1. 화면 픽셀 좌표 가져오기
        Vector2 screenPos = Mouse.current.position.ReadValue();
        
        // 2. 메인 카메라를 이용해 월드 좌표로 변환
        // Z값이 카메라 위치로 고정되므로, 2D 평면인 Z=0으로 맞춰줍니다.
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        
        return worldPos;
    }
    private bool IsMouseOn()
    {
        if(Mathf.Abs(_transform.position.x-_mousePos.x) < _transform.localScale.x / 2)
        {
            if(Mathf.Abs(_mousePos.y - _transform.position.y) < _transform.localScale.y/2)
            {
                return true;
            }
        }
        return false;
    }
}
