using UnityEngine;

public class CreateElement : MonoBehaviour
{

    [SerializeField] private DatabaseManager dbManager;

    public void CreateCube(Substance Formula, float resmass, GameObject _legacyCube, float restemperature = 20.0f)
    {
        if (_legacyCube == null)
        {
            Debug.LogError("cube prefab is NULL!");
            return;
        }

        GameObject newCube = Instantiate(_legacyCube, _legacyCube.transform.position, Quaternion.identity);

        DragElement drag = newCube.GetComponent<DragElement>();
        if (drag != null)
        {
            drag.InitCamera(Camera.main);
        }


        ElementSettings settings = newCube.GetComponent<ElementSettings>();
        if (settings != null)
        {
            settings.InitSubstance(Formula, resmass, restemperature);
        }
    }
}