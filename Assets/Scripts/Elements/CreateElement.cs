using System;
using TMPro;
using UnityEngine;

public class CreateElement : MonoBehaviour
{
    public static Action<string> OnElementCreated;
    [SerializeField]private GameObject _legacyCube;


    public void CreateCube(string ElementName)
    {
        if (_legacyCube == null) return;
        if(_legacyCube != null)
        {
            TextMeshPro text = _legacyCube.GetComponentInChildren<TextMeshPro>();
            text.text = ElementName;
        }
        GameObject newCube = Instantiate(_legacyCube, Vector3.zero, Quaternion.identity);
        newCube.GetComponent<DragElement>().InitCamera(Camera.main);
        OnElementCreated?.Invoke(ElementName);
    }
}
