using UnityEngine;

public class AnswerToggleData : MonoBehaviour
{
    public string OptionCode { get; private set; }

    public void Setup(string optionCode)
    {
        OptionCode = optionCode;
    }
}
