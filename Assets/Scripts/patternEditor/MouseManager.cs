using UnityEngine;
using UnityEngine.InputSystem;

public class MouseManager : MonoBehaviour
{
    
    [SerializeField] private LayerMask _noteLayerMask; 
    private EditorNote _editorNote = null;
    private void Update()
    {
        if (EditorInputBlocker.IsBlocked) // 다른 판넬 열려있으면 막는 로직
        {
            if (_editorNote != null)
            {
                _editorNote.OnOffMouse();
                _editorNote = null;
            }
            return;
        }
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        RaycastHit2D noteHit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, _noteLayerMask);

        if (noteHit.collider != null)
        {
            if(_editorNote != noteHit.transform.GetComponent<EditorNote>()){
                if(_editorNote != null) _editorNote.OnOffMouse();

                _editorNote = noteHit.transform.GetComponent<EditorNote>();
                _editorNote.OnOverMouse();
            }
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                _editorNote.DeleteNote();
            }
        }
        else if(_editorNote != null)
        {
            _editorNote.OnOffMouse();
            _editorNote = null;
        }
    }
}
