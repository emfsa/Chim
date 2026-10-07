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
        //ElementSettings thisSettings = GetComponent<ElementSettings>();
        //if (thisSettings == null) return;

        //ElementSettings otherSettings = collision.collider.GetComponent<ElementSettings>();
        //if (otherSettings == null) return;

        //string thisFormula = thisSettings.GetName(), otherFormula = otherSettings.GetName();
        //string[] reactants = { thisFormula, otherFormula };
        //float[] masses = { thisSettings.GetMass(), otherSettings.GetMass() };
        //float[] temperatures = {thisSettings.GetTemperature(), otherSettings.GetTemperature() };

        //string[] result = calculator.Process(reactants, masses, temperatures).Split(';');
        //if (result[0] != "temperature") return;


        //float temperature = float.Parse(result[1]);

        //string resFormula = "";
        //bool fl = true;
        //int j = 0;
        //for (int i = 2; i < result.Length; i++)
        //{
        //    if (result[i] == "spent")
        //    {
        //        fl = false;
        //        continue;
        //    }
        //    if ((i%2==0) && fl) resFormula = result[i];
        //    else if ((i % 2 == 1) && fl)
        //    {
        //        print("resMassa");
        //        print(float.Parse(result[i]));
        //        create.CreateCube(db.GetSubstance(resFormula), float.Parse(result[i]), gameObject, temperature);
        //    }
        //    else
        //    {
        //        if (j==0)
        //        {
        //            thisSettings.ChangeMass(-float.Parse(result[i]));
        //            j++;
        //        }
        //        else
        //        {
        //            otherSettings.ChangeMass(-float.Parse(result[i]));
        //        }
        //    }
        //}

        
    }
}