using UnityEngine;

public class ElementSettings : MonoBehaviour
{
    [SerializeField] private string _elementName;
    [SerializeField] private float _elementMass;
    [SerializeField] private float _elementDensity;
    [SerializeField] private float _elementTemperature;

    private void OnEnable()
    {
        CreateElement.OnElementCreated += setName;
    }
    private void OnDisable()
    {
        CreateElement.OnElementCreated -= setName;
    }



    private void Start()
    {
        _elementMass = 1.0f;
        _elementDensity = 1.0f;
        _elementTemperature = 20.0f;
    }
  

    private void setName(string name)
    {
        _elementName = name;
    }

    
}
