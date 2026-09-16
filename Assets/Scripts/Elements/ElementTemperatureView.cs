using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ElementTemperatureView : MonoBehaviour
{
    private TMP_Text _text;
    private ElementSettings _element;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _element = GetComponentInParent<ElementSettings>();
    }

    private void Start()
    {
        if (_element != null)
        {
            UpdateText(_element.GetTemperature());
        }
    }

    private void OnEnable()
    {
        if (_element != null)
        {
            _element.OnTemperatureChanged += UpdateText;
            UpdateText(_element.GetTemperature());
        }
    }

    private void OnDisable()
    {
        if (_element != null)
        {
            _element.OnTemperatureChanged -= UpdateText;
        }
    }

    private void UpdateText(float temperature)
    {
        if (_text != null)
        {
            _text.text = $"{temperature:F1}°C";
        }
    }
}