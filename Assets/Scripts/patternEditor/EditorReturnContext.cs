public static class EditorReturnContext
{
    public static bool ShouldRestore { get; private set; }
    public static string SongName { get; private set; }
    public static int DifficultyIndex { get; private set; }

    public static void Set(string songName, int difficultyIndex)
    {
        ShouldRestore = true;
        SongName = songName;
        DifficultyIndex = difficultyIndex;
    }

    public static void Clear()
    {
        ShouldRestore = false;
        SongName = "";
        DifficultyIndex = 0;
    }
}