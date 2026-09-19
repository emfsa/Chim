using System;
using Unity.VisualScripting;
using UnityEngine;

public class Element: MonoBehaviour
{
    [SerializeField] ChemistryCalculator calculator;
    [SerializeField] DatabaseManager db;
    [SerializeField] CreateElement create;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        print("Objects are colliding");

        ElementSettings thisSettings = GetComponent<ElementSettings>();
        if (thisSettings == null) return;

        ElementSettings otherSettings = collision.collider.GetComponent<ElementSettings>();
        if (otherSettings == null) return;

        string thisFormula = thisSettings.GetName(), otherFormula = otherSettings.GetName();
        string[] reactants = { thisFormula, otherFormula };
        float[] masses = { thisSettings.GetMass(), otherSettings.GetMass() };

        string[] result = calculator.Process(reactants, masses).Split(';');
        string resFormula = "";
        bool fl = true;
        int j = 0;
        for (int i = 0; i < result.Length; i++)
        {
            if (result[i] == "spent")
            {
                fl = false;
                continue;
            }
            if ((i%2==0) && fl) resFormula = result[i];
            else if ((i % 2 == 1) && fl)
            {
                print("resMassa");
                print(float.Parse(result[i]));
                create.CreateCube(db.GetSubstance(resFormula), float.Parse(result[i]), thisSettings.GetTemperature());
            }
            else
            {
                if (j==0)
                {
                    thisSettings.ChangeMass(-float.Parse(result[i]));
                    j++;
                }
                else
                {
                    otherSettings.ChangeMass(-float.Parse(result[i]));
                }
            }
        }

        
    }
}