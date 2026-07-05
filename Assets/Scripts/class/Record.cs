using System;

public enum ComboResult
{
    none = 0, 
    fullcombo = 1,
    allperfact = 2,
}
[Serializable]
public class Record
{
    public float score = 0;
    public int maxcombo = 0;
    public ComboResult comboResult = ComboResult.none;
    public float prate = 0;
}