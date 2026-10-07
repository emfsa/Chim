using System;
using UnityEngine;

public class ElementSettings : MonoBehaviour
{
    [SerializeField] DatabaseManager dbManager;

    [Header("Параметры элемента")]
    [SerializeField] private string _elementName;
    [SerializeField] private float _elementMass;
    [SerializeField] private float _elementMolar;
    [SerializeField] private float _elementDensity;
    [SerializeField] private float _elementTemperature;
    [SerializeField] private Color _elementColor;
    [SerializeField] private string _elementState;


    public event Action<float> OnTemperatureChanged;
    public event Action<string> OnNameChanged;

    private void Start()
    {
        OnTemperatureChanged += HandleTemperatureChanged;

        OnNameChanged?.Invoke(_elementName);
        OnTemperatureChanged?.Invoke(_elementTemperature);

        var sub = dbManager.GetSubstance(_elementName);
        if (sub != null)
            InitSubstance(sub, _elementMass, _elementTemperature);

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.color = _elementColor;

    }

    
    public void Init(string elementName, float mass = 1f, float density = 1f, float temperature = 20f, Color? elementColor = null, float molar_mass = 1, string elementState = "solid") 
    {
        Color finalColor = elementColor ?? Color.white;

        _elementName = elementName;
        _elementMass = mass;
        _elementDensity = density;
        _elementTemperature = temperature;
        _elementColor = finalColor;
        _elementMolar = molar_mass;
        _elementState = elementState;
        OnTemperatureChanged?.Invoke(_elementTemperature);
        OnNameChanged?.Invoke(_elementName);
    }

    public void InitSubstance(Substance el, float mass = 1.0f, float temperature = 20.0f) 
    {
        Color finalColor;

        _elementName = el.formula;
        _elementMass = mass;
        _elementDensity = el.density;
        _elementTemperature = temperature;
        _elementMolar = el.molar_mass;
        _elementState = "solid";

        if (ColorUtility.TryParseHtmlString(el.color, out finalColor))
            _elementColor = finalColor;
        else _elementColor = Color.white;

        OnTemperatureChanged?.Invoke(_elementTemperature);
        OnNameChanged?.Invoke(_elementName);
    }

    public void AddTemperature(float amount)
    {
        _elementTemperature = _elementTemperature + amount;
        OnTemperatureChanged?.Invoke(_elementTemperature);
    }

    public void SetTemperature(float amount)
    {
        _elementTemperature = amount;
        OnTemperatureChanged?.Invoke(_elementTemperature);
    }

    public float GetTemperature() => _elementTemperature;
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

    private void HandleTemperatureChanged(float temperature)
    {
        Substance el = dbManager.GetSubstance(_elementName);


        if (temperature < el.melt_temp)
        {
            _elementState = "solid";
            GetComponent<Rigidbody2D>().gravityScale = 1;
        }
        else if (temperature > el.boil_temp)
        {
            _elementState = "gas";
            if (dbManager.GetSubstance(_elementName).molar_mass < 29) GetComponent<Rigidbody2D>().gravityScale = -1;
        }
        else
        {
            _elementState = "liquid";
            GetComponent<Rigidbody2D>().gravityScale = 1;

        }
    }
}