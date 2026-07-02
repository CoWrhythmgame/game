public enum NoteType
{
    single = 0,
    hold = 1,
}
public class note
{
    public int lane = 0;
    public float time = 0;
    public NoteType noteType = NoteType.single;
    public float releaseTime = 0;
    public EditorOnlyNote editorOnlyNote;
    
}