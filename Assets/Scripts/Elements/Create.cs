using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Create : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private TMP_InputField mass;
    [SerializeField] private TMP_InputField element;
    [SerializeField] private GameObject _legacyCube;
    [SerializeField] private DatabaseManager dbManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        createButton.onClick.AddListener(() => ButtonClicked());
    }

    // Update is called once per frame
    public void ButtonClicked()
    {
        string el = element.text;
        float m = float.Parse(mass.text);

        GameObject newCube = Instantiate(_legacyCube, _legacyCube.transform.position, Quaternion.identity);

        DragElement drag = newCube.GetComponent<DragElement>();
        if (drag != null)
        {
            drag.InitCamera(Camera.main);
        }


        ElementSettings settings = newCube.GetComponent<ElementSettings>();
        if (settings != null)
        {
            settings.InitSubstance(dbManager.GetSubstance(el), m, 20);
        }

        Rigidbody2D rigidbody2D = newCube.GetComponent<Rigidbody2D>();

        rigidbody2D.constraints = RigidbodyConstraints2D.None;
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        SpriteRenderer renderer = newCube.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = 0;

        newCube.GetComponent<DragElement>().enabled = true;
        newCube.GetComponent<ElementSettings>().enabled = true;
    }
}
