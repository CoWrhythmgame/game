using System;
using System.Collections.Generic;

[Serializable]
public class Song
{
    public string songname = "";
    public string artist = "";
    public float bpm = 0;
    public string songPath = "";
    public string previewPath = "";
    public List<Pattern> pattern;
    public List<Record> record;
}
