using System;
using System.Collections.Generic;

[Serializable]
public class Song
{
    public string songname = "";
    public string artist = "";
    public float bpm = 0;
    public List<PatternInfo> patternInfo;
    public List<Record> record;
}
