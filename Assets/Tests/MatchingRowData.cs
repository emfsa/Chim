using UnityEngine;

public class MatchingRowData : MonoBehaviour
{
    public string ItemCode { get; private set; }
    public string CorrectValue { get; private set; }

    public void Setup(string itemCode, string correctValue)
    {
        ItemCode = itemCode;
        CorrectValue = correctValue;
    }
}
