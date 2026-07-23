using System;

public enum NoteType
{
    single = 0,
    hold = 1,
}
[Serializable]
public class Note
{
    public int lane = 0;
    public double time = 0;
    public NoteType noteType = NoteType.single;
    public double releaseTime = 0;
    public float bpm = 1;
    public EditorOnlyNote editorOnlyNote;
    
}