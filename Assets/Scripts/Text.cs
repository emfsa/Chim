using TMPro;
using UnityEngine;

public class Text : MonoBehaviour
{
    [SerializeField]private TMP_Text _text;

    private void Start()
    {
        _text = GetComponent<TMP_Text>();
    }
   /* private void OnEnable()
    {
        ElementSettings.OnTemperatureChanged += SwitchText;
    }
    private void OnDisable()
    {
        ElementSettings.OnTemperatureChanged -= SwitchText;
    }*/
    public void SwitchText(string newText)
    {
        _text.text = newText;
    }
}
