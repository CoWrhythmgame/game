using UnityEngine;
using SimpleFileBrowser;

public class RuntimeSongFilePicker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EditorSongFileLoader songFileLoader;

    private void OnEnable()
    {
        if (songFileLoader != null)
            songFileLoader.OnSongFileOpenRequested += OpenFileBrowser;
    }

    private void OnDisable()
    {
        if (songFileLoader != null)
            songFileLoader.OnSongFileOpenRequested -= OpenFileBrowser;
    }

    private void OpenFileBrowser()
    {
        FileBrowser.SetFilters(
            false,
            new FileBrowser.Filter("Audio Files", ".mp3", ".wav", ".ogg")
        );

        FileBrowser.SetDefaultFilter(".mp3");

        FileBrowser.ShowLoadDialog(
            OnFileSelected,
            OnFileSelectCanceled,
            FileBrowser.PickMode.Files,
            false,
            null,
            null,
            "Load Song Audio",
            "Load"
        );
    }

    private void OnFileSelected(string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return;

        string path = paths[0];

        if (string.IsNullOrWhiteSpace(path))
            return;

        songFileLoader.LoadSongFromPath(path);
    }

    private void OnFileSelectCanceled()
    {
        Debug.Log("Song loading canceled.");
    }
}