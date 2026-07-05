using UnityEngine;
using UnityEngine.InputSystem;

public class KeySoundTestInput : MonoBehaviour
{
    [SerializeField] private KeySoundPlayer keySoundPlayer;

    private void Update()
    {
        if (Keyboard.current == null || keySoundPlayer == null)
            return;

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            keySoundPlayer.PlayLane1Sound();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            keySoundPlayer.PlayLane2Sound();
        }

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            keySoundPlayer.PlayLane3Sound();
        }

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            keySoundPlayer.PlayLane4Sound();
        }
    }
}