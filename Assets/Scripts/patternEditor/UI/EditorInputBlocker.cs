using UnityEngine;

public class EditorInputBlocker : MonoBehaviour
{
    public static bool IsBlocked { get; private set; }
    public static void SetBlocked(bool blocked)
    {
        IsBlocked = blocked;
    }
}