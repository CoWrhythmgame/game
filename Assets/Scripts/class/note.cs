public enum NoteType
{
    single = 0,
    hold = 1,
}
public class note
{
    int lane = 0;
    float time = 0;
    NoteType noteType = NoteType.single;
    float releaseTime = 0;
    EditorOnlyNote editorOnlyNote;
    
}