using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MixingZone : MonoBehaviour
{
    [SerializeField] ChemistryCalculator calculator;
    [SerializeField] DatabaseManager db;
    [SerializeField] CreateElement create;

    private List<Collider2D> collidersList = new List<Collider2D>();
    private List<ElementSettings> elementsList = new List<ElementSettings>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Element"))
        {
            ElementSettings elementSettings = collision.GetComponent<ElementSettings>();
            if (elementSettings == null) return;
            elementsList.Add(elementSettings);
            collidersList.Add(collision);
            share_temp();
            if (SortElements())
            {
                string result = tryReact();
                if (result != "") Finish(result);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Element"))
        {
            ElementSettings elementSettings = collision.GetComponent<ElementSettings>();
            if (elementSettings == null) return;
            elementsList.Remove(elementSettings);
            collidersList.Remove(collision);
            if (SortElements())
            {
                string result = tryReact();
                if (result != "") Finish(result);
            }
        }
    }

    private void share_temp()
    {
        float all_hc = 0;
        float Q = 0;

        for (int i = 0; i < elementsList.Count; i++)
        {
            Substance sub = db.GetSubstance(elementsList[i].GetName());
            all_hc += sub.heat_cap * elementsList[i].GetMass() / sub.molar_mass;
            Q += sub.heat_cap * (elementsList[i].GetTemperature() + 273) * (elementsList[i].GetMass() / sub.molar_mass);
        }

        float temperature = Q / all_hc - 273;

        for (int i = 0; i < elementsList.Count; i++)
        {
            elementsList[i].SetTemperature(temperature);
        }
    }

    private bool SortElements()
    {
        string[] reactants = new string[elementsList.Count];
        for (int i = 0; i < elementsList.Count; i++)
            reactants[i] = elementsList[i].GetName();
        Reaction reaction = db.FindReactionSorted(reactants);
        if (reaction == null) return false;
        string[] found = reaction.reactants.Split(',');
        List<Collider2D> collidersListnew = new List<Collider2D>();
        List<ElementSettings> elementsListnew = new List<ElementSettings>();
        for (int i = 0; i < found.Length;i++) 
            for (int j = 0; j < elementsList.Count; j++)
            {
                if (found[i] == elementsList[j].GetName())
                {
                    collidersListnew.Add(collidersList[j]);
                    elementsListnew.Add(elementsList[j]);
                }
            }
        collidersList = collidersListnew;
        elementsList = elementsListnew;
        return true;
    }

    private string tryReact()
    {
        string[] reactants = new string[elementsList.Count];
        float[] masses = new float[elementsList.Count];
        float[] temperatures = new float[elementsList.Count];
        for (int i = 0;  i < elementsList.Count; i++)
        {
            reactants[i] = elementsList[i].GetName();
            masses[i] = elementsList[i].GetMass();
            temperatures[i] = elementsList[i].GetTemperature();
        }
        string result = calculator.Process(reactants, masses, temperatures);
        return result;
    }

    private void Finish(string result)
    {
        string[] res = result.Split(';');
        float temperature = float.Parse(res[1]);
        res = res[2..];
        string resFormula = "";
        for (int i = 0; i < res.Length; i++)
        {
            if (res[i] == "spent")
            {
                res = res[(i + 1)..];
                break;
            }
            if (i%2==0)
            {
                resFormula = res[i];
            }
            else
            {
                print(res[i]);
                create.CreateCube(db.GetSubstance(resFormula), float.Parse(res[i]), collidersList[0].gameObject, temperature);
            }
        }
        for (int i = 0; i < res.Length - 1; i++)
        {
            ElementSettings thisSettings = elementsList[i];
            thisSettings.ChangeMass(-float.Parse(res[i]));
            
        }


    }
}
