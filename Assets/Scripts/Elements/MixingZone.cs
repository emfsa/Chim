using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class MixingZone : MonoBehaviour
{
    [SerializeField] ChemistryCalculator calculator;
    [SerializeField] DatabaseManager db;
    [SerializeField] CreateElement create;

    private List<Collider2D> collidersList = new List<Collider2D>();
    private List<ElementSettings> elementsList = new List<ElementSettings>();
    private bool isProcessingReaction = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isProcessingReaction) return;

        if (collision.CompareTag("Element"))
        {
            ElementSettings elementSettings = collision.GetComponent<ElementSettings>();
            if (elementSettings == null || elementsList.Contains(elementSettings)) return;

            elementsList.Add(elementSettings);
            collidersList.Add(collision);

            CleanupDestroyedElements();
            share_temp();

            if (SortElements())
            {
                string result = tryReact();
                if (!string.IsNullOrEmpty(result))
                {
                    Finish(result);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isProcessingReaction) return;

        if (collision.CompareTag("Element"))
        {
            ElementSettings elementSettings = collision.GetComponent<ElementSettings>();
            if (elementSettings == null) return;

            elementsList.Remove(elementSettings);
            collidersList.Remove(collision);

            CleanupDestroyedElements();

            if (SortElements())
            {
                string result = tryReact();
                if (!string.IsNullOrEmpty(result))
                {
                    Finish(result);
                }
            }
        }
    }

    /// <summary>Удаляет из списков уничтоженные объекты или реагенты с нулевой массой</summary>
    private void CleanupDestroyedElements()
    {
        for (int i = elementsList.Count - 1; i >= 0; i--)
        {
            if (elementsList[i] == null || collidersList[i] == null || elementsList[i].GetMass() <= 0.0001f)
            {
                elementsList.RemoveAt(i);
                collidersList.RemoveAt(i);
            }
        }
    }

    private void share_temp()
    {
        if (elementsList.Count == 0) return;

        float all_hc = 0f;
        float Q = 0f;

        for (int i = 0; i < elementsList.Count; i++)
        {
            Substance sub = db.GetSubstance(elementsList[i].GetName());
            if (sub == null) continue;

            float mass = elementsList[i].GetMass();
            all_hc += sub.heat_capacity * mass;
            Q += sub.heat_capacity * (elementsList[i].GetTemperature() + 273f) * mass;
        }

        if (all_hc <= 0f) return;

        float temperature = Q / all_hc - 273f;

        for (int i = 0; i < elementsList.Count; i++)
        {
            elementsList[i].SetTemperature(temperature);
        }
    }

    private bool SortElements()
    {
        if (elementsList.Count < 1) return false;

        string[] reactants = new string[elementsList.Count];
        for (int i = 0; i < elementsList.Count; i++)
        {
            reactants[i] = elementsList[i].GetName();
        }

        // 1. Проверяем наличие точной реакции в таблице reactions
        Reaction reaction = db.FindReactionSorted(reactants);
        if (reaction != null)
        {
            string[] found = reaction.reagents.Split(',');
            ReorderListsByTarget(found, useClasses: false);
            return true;
        }

        // 2. Проверяем наличие реакции по классам в таблице class_reactions
        ClassReaction classReaction = db.GetClassReactionSet(reactants);
        if (classReaction != null)
        {
            string[] foundClasses = classReaction.reagents.Split(',');
            ReorderListsByTarget(foundClasses, useClasses: true);
            return true;
        }

        return false;
    }

    /// <summary>Сортирует списки реагентов без потери и дублирования объектов</summary>
    private void ReorderListsByTarget(string[] targets, bool useClasses)
    {
        List<Collider2D> collidersListNew = new List<Collider2D>();
        List<ElementSettings> elementsListNew = new List<ElementSettings>();
        bool[] used = new bool[elementsList.Count];

        for (int i = 0; i < targets.Length; i++)
        {
            for (int j = 0; j < elementsList.Count; j++)
            {
                if (used[j]) continue;

                string formula = elementsList[j].GetName();
                string key = formula;

                if (useClasses)
                {
                    if (formula != "O2" && formula != "H2O" && formula != "H2")
                    {
                        Substance sub = db.GetSubstance(formula);
                        key = sub != null ? sub.class_name : "";
                    }
                }

                if (targets[i] == key)
                {
                    used[j] = true;
                    collidersListNew.Add(collidersList[j]);
                    elementsListNew.Add(elementsList[j]);
                    break;
                }
            }
        }

        // Если остались элементы (например, катализатор), добавляем их в конец
        for (int j = 0; j < elementsList.Count; j++)
        {
            if (!used[j])
            {
                collidersListNew.Add(collidersList[j]);
                elementsListNew.Add(elementsList[j]);
            }
        }

        collidersList = collidersListNew;
        elementsList = elementsListNew;
    }

    private string tryReact()
    {
        string[] reactants = new string[elementsList.Count];
        float[] masses = new float[elementsList.Count];
        float[] temperatures = new float[elementsList.Count];

        for (int i = 0; i < elementsList.Count; i++)
        {
            reactants[i] = elementsList[i].GetName();
            masses[i] = elementsList[i].GetMass();
            temperatures[i] = elementsList[i].GetTemperature();
        }

        return calculator.Process(reactants, masses, temperatures);
    }

    private void Finish(string result)
    {
        isProcessingReaction = true;

        try
        {
            string[] res = result.Split(';');
            float temperature = ParseFloat(res[1]);
            res = res[2..];

            GameObject spawnReference = collidersList[0] != null ? collidersList[0].gameObject : gameObject;

            string resFormula = "";
            int spentIndex = -1;

            for (int i = 0; i < res.Length; i++)
            {
                if (res[i] == "spent")
                {
                    spentIndex = i + 1;
                    break;
                }
                if (i % 2 == 0)
                {
                    resFormula = res[i];
                }
                else
                {
                    float productMass = ParseFloat(res[i]);
                    if (productMass > 0f)
                    {
                        create.CreateCube(db.GetSubstance(resFormula), productMass, spawnReference, temperature);
                    }
                }
            }

            if (spentIndex >= 0)
            {
                string[] spentMasses = res[spentIndex..];
                for (int i = 0; i < spentMasses.Length - 1 && i < elementsList.Count; i++)
                {
                    if (elementsList[i] != null)
                    {
                        float spentMass = ParseFloat(spentMasses[i]);
                        elementsList[i].ChangeMass(-spentMass);
                    }
                }
            }

            CleanupDestroyedElements();
        }
        finally
        {
            isProcessingReaction = false;
        }
    }

    private float ParseFloat(string value)
    {
        return float.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
    }
}
