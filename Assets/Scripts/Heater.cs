using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heater : MonoBehaviour
{
    [SerializeField] private float _heatSpeed = 10f; 
    [SerializeField] private float _timeInterval = 0.5f;

    private Dictionary<ElementSettings, Coroutine> _activeCoroutines = new Dictionary<ElementSettings, Coroutine>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Element"))
        {
            ElementSettings elementSettings = collision.GetComponent<ElementSettings>();
            if (elementSettings == null) return;

            if (!_activeCoroutines.ContainsKey(elementSettings))
            {
                Coroutine coroutine = StartCoroutine(HeatRoutine(elementSettings));
                _activeCoroutines.Add(elementSettings, coroutine);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Element"))
        {
            ElementSettings elementSettings = collision.GetComponent<ElementSettings>();
            if (elementSettings == null) return;

            if (_activeCoroutines.TryGetValue(elementSettings, out Coroutine coroutine))
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
                _activeCoroutines.Remove(elementSettings);
            }
        }
    }

    private IEnumerator HeatRoutine(ElementSettings element)
    {
        while (true)
        {
            yield return new WaitForSeconds(_timeInterval);

            if (element == null) yield break;

            element.AddTemperature(_heatSpeed);
        }
    }
}