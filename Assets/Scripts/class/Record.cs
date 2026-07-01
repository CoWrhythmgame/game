public enum ComboResult
{
    none = 0, 
    fullcombo = 1,
    allperfact = 2,
}
public class Record
{
    float score = 0;
    int maxcombo = 0;
    ComboResult comboResult = ComboResult.none;
    float prate = 0;
}