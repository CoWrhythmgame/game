using UnityEngine;

public class DirtyState : MonoBehaviour
{
    public bool IsDirty { get; private set; } = false;

    public void MarkDirty()
    {
        IsDirty = true;
        Debug.Log("DirtyState: true");
    }

    public void MarkSaved()
    {
        IsDirty = false;
        Debug.Log("DirtyState: false - saved");
    }

    public void ClearDirty()
    {
        IsDirty = false;
        Debug.Log("DirtyState: false - cleared");
    }
}