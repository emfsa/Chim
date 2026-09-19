using System;
using UnityEngine;

public class ElementSettings : MonoBehaviour
{
    [SerializeField] DatabaseManager dbManager;

    [Header("Параметры элемента")]
    [SerializeField] private string _elementName;
    [SerializeField] private float _elementMass;
    [SerializeField] private float _elementDensity;
    [SerializeField] private float _elementTemperature;
    [SerializeField] private float _elementMaxTemperature;
    

    public event Action<float> OnTemperatureChanged;
    public event Action<string> OnNameChanged;

    private void Start()
    {
        OnNameChanged?.Invoke(_elementName);
        OnTemperatureChanged?.Invoke(_elementTemperature);

        var sub = dbManager.GetSubstance(_elementName);
        if (sub != null)
            InitSubstance(sub, _elementMass, _elementTemperature);

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

    public void InitSubstance(Substance el, float mass = 1.0f, float temperature = 20.0f) // передаем сюда данные из БД
    {
        _elementName = el.formula;
        _elementMass = mass;
        _elementDensity = el.molar_mass;
        _elementTemperature = temperature;
        _elementMaxTemperature = 100.0f;

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
    public float GetMass() => _elementMass;

    public void ChangeMass(float _mass)
    {
        _elementMass += _mass;

        if (_elementMass<=0f)
        {
            Destroy(gameObject);
        }
    }

    public void SetMass(float _mass)
    {
        _elementMass = _mass;
        if (_elementMass <= 0f)
        {
            Destroy(gameObject);
        }
    }
}