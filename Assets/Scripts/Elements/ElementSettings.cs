using System;
using UnityEngine;

public class ElementSettings : MonoBehaviour
{
    [Header("Параметры элемента")]
    [SerializeField] private string _elementName = "Default";
    [SerializeField] private float _elementMass = 1.0f;
    [SerializeField] private float _elementDensity = 1.0f;
    [SerializeField] private float _elementTemperature = 20.0f;
    [SerializeField] private float _elementMaxTemperature = 100.0f;

    public event Action<float> OnTemperatureChanged;
    public event Action<string> OnNameChanged;

    private void Start()
    {
        OnNameChanged?.Invoke(_elementName);
        OnTemperatureChanged?.Invoke(_elementTemperature);
    }

    
    public void Init(string elementName, float mass = 1f, float density = 1f, float temperature = 20f, float maxTemperature = 100f) // передаем сюда данные из БД
    {
        _elementName = elementName;
        _elementMass = mass;
        _elementDensity = density;
        _elementTemperature = temperature;
        _elementMaxTemperature = maxTemperature;

        OnNameChanged?.Invoke(_elementName);
        OnTemperatureChanged?.Invoke(_elementTemperature);
    }

    public void AddTemperature(float amount)
    {
        if (_elementTemperature < _elementMaxTemperature)
        {
            _elementTemperature = Mathf.Min(_elementTemperature + amount, _elementMaxTemperature);
            OnTemperatureChanged?.Invoke(_elementTemperature);
        }
    }

    public float GetTemperature() => _elementTemperature;
    public float GetMaxTemperature() => _elementMaxTemperature;
    public string GetName() => _elementName;
}