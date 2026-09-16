using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ElementNameView : MonoBehaviour
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
            UpdateText(_element.GetName());
        }
    }

    private void OnEnable()
    {
        if (_element != null)
        {
            _element.OnNameChanged += UpdateText;
            UpdateText(_element.GetName());
        }
    }

    private void OnDisable()
    {
        if (_element != null)
        {
            _element.OnNameChanged -= UpdateText;
        }
    }

    private void UpdateText(string elementName)
    {
        if (_text != null)
        {
            _text.text = elementName;
        }
    }
}