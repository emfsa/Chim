using UnityEngine;

public class CreateElement : MonoBehaviour
{
    [SerializeField] private GameObject _legacyCube;
    [SerializeField] private DatabaseManager dbManager;
    //public void CreateCube(string elementName/*оставим пока БД не подключим*/)
    //{
    //    if (_legacyCube == null)
    //    {
    //        Debug.LogError("cube prefab is NULL!");
    //        return;
    //    }

    //    GameObject newCube = Instantiate(_legacyCube, Vector3.zero, Quaternion.identity);

    //    DragElement drag = newCube.GetComponent<DragElement>();
    //    if (drag != null)
    //    {
    //        drag.InitCamera(Camera.main);
    //    }


    //    ElementSettings settings = newCube.GetComponent<ElementSettings>(); //Задаем все настройки элемента(подтягиваем из БД)
    //    if (settings != null)
    //    {
    //        settings.Init(
    //            elementName: elementName,
    //            mass: 1.0f,
    //            density: 1.0f,
    //            temperature: 20.0f,
    //            maxTemperature: 100.0f
    //        );
    //    }
    //}

    public void CreateCube(Substance Formula, float resmass, float restemperature = 20.0f)
    {
        print("mass after create");
        print(resmass);
        if (_legacyCube == null)
        {
            Debug.LogError("cube prefab is NULL!");
            return;
        }

        GameObject newCube = Instantiate(_legacyCube, Vector3.zero, Quaternion.identity);

        DragElement drag = newCube.GetComponent<DragElement>();
        if (drag != null)
        {
            drag.InitCamera(Camera.main);
        }


        ElementSettings settings = newCube.GetComponent<ElementSettings>();
        if (settings != null)
        {
            settings.Init(
                elementName: Formula.formula,
                mass: resmass,
                density: Formula.molar_mass,
                temperature: restemperature,
                maxTemperature: 100.0f
            );
        }
    }
}