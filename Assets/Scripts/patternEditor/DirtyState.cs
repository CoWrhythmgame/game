using UnityEngine;

public class DirtyState : MonoBehaviour
{
    public bool IsDirty { get; private set; } = false;

    public void MarkDirty()
    {
        IsDirty = true;
        Debug.Log("Editor dirty.");
    }

    public void MarkSaved()
    {
        IsDirty = false;
        Debug.Log("Editor saved.");
    }

    public void ClearDirty()
    {
        IsDirty = false;
        Debug.Log("Editor dirty cleared.");
    }
}