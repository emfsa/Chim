using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChemistryCalculator : MonoBehaviour
{
    public DatabaseManager dbManager;

    public string Process(string[] reactants, float[] masses, float[] temperatures)
    {
        if (reactants == null || reactants.Length == 0)
            return "";

        Reaction reaction = dbManager.FindReaction(reactants) ?? dbManager.FindReactionSorted(reactants);
        if (reaction != null && !string.IsNullOrEmpty(reaction.products))
        {
            int[] rCoeffs = reaction.r_moles.Split(',').Select(int.Parse).ToArray();
            string[] rFormula = reaction.reagents.Split(',');
            string[] pFormula = reaction.products.Split(',');
            int[] pCoeffs = reaction.p_moles.Split(',').Select(int.Parse).ToArray();

            return CalculateReactionResult(
                reactants, masses, temperatures,
                rFormula, rCoeffs, pFormula, pCoeffs,
                reaction.catalyst, reaction.min_temp
            );
        }

        if (reactants.Length < 2)
            return "";

        List<ClassReaction> classReactions = dbManager.GetClassReactions(reactants);
        if (classReactions == null || classReactions.Count == 0)
            return "";

        HashSet<string> reactantElements = new HashSet<string>(dbManager.GetElements(reactants[0]));
        for (int i = 1; i < reactants.Length; i++)
        {
            reactantElements.UnionWith(dbManager.GetElements(reactants[i]));
        }

        foreach (ClassReaction creaction in classReactions)
        {
            // Проверка ряда активности для реакций замещения (металл + кислота / металл + соль)
            if (!CheckActivityRules(reactants, creaction))
                continue;

            string[] prodTemplates = creaction.products.Split(',');

            // Подбираем вещества-продукты, подходящие под классы и набор элементов
            if (TryResolveClassProducts(reactants, prodTemplates, reactantElements, out string[] pFormula, out int[] rCoeffs, out int[] pCoeffs))
            {
                return CalculateReactionResult(
                    reactants, masses, temperatures,
                    reactants, rCoeffs, pFormula, pCoeffs,
                    null, 0
                );
            }
        }

        return "";
    }

    private string CalculateReactionResult(
        string[] reactants, float[] masses, float[] temperatures,
        string[] rFormula, int[] rCoeffs, string[] pFormula, int[] pCoeffs,
        string catalystStr, int minTemp)
    {
        float minMolesReaction = float.MaxValue;
        float all_hc = 0f;
        float Q = 0f;

        if (!string.IsNullOrEmpty(catalystStr))
        {
            string[] catalyst = catalystStr.Split(',');
            for (int i = 0; i < reactants.Length; i++)
            {
                if (reactants[i] == catalyst[0])
                {
                    reactants = reactants[0..i];
                    rCoeffs = rCoeffs[0..i];
                    rFormula = rFormula[0..i];
                    break;
                }
            }
        }

        for (int i = 0; i < reactants.Length; i++)
        {
            Substance sub = dbManager.GetSubstance(reactants[i]);
            if (sub == null) return "";

            int coeffIndex = Array.IndexOf(rFormula, reactants[i]);
            int rcoeff = coeffIndex >= 0 ? rCoeffs[coeffIndex] : rCoeffs[i];

            all_hc += sub.heat_capacity * masses[i];
            Q += (temperatures[i] + 273f) * sub.heat_capacity * masses[i];
        }

        float finalTemp = Q / all_hc - 273f;
        if (finalTemp < minTemp)
            return "";

        for (int i = 0; i < reactants.Length; i++)
        {
            Substance sub = dbManager.GetSubstance(reactants[i]);
            float rmoles = masses[i] / dbManager.GetMolarMass(sub.formula);
            int coeffIndex = Array.IndexOf(rFormula, reactants[i]);
            int rcoeff = coeffIndex >= 0 ? rCoeffs[coeffIndex] : rCoeffs[i];
            float reactionMoles = rmoles / rcoeff;

            if (reactionMoles < minMolesReaction)
                minMolesReaction = reactionMoles;
        }

        string result = "";

        for (int i = 0; i < pFormula.Length; i++)
        {
            Substance sub = dbManager.GetSubstance(pFormula[i]);
            float moles = minMolesReaction * pCoeffs[i];
            float mass = moles * dbManager.GetMolarMass(sub.formula);
            result += $"{pFormula[i]};{mass};";
        }

        result += "spent;";

        for (int i = 0; i < reactants.Length; i++)
        {
            Substance sub = dbManager.GetSubstance(reactants[i]);
            int coeffIndex = Array.IndexOf(rFormula, reactants[i]);
            int rcoeff = coeffIndex >= 0 ? rCoeffs[coeffIndex] : rCoeffs[i];
            float mass = dbManager.GetMolarMass(sub.formula) * minMolesReaction * rcoeff;
            result += Convert.ToString(mass) + ";";
        }

        result = "temperature;" + Convert.ToString(finalTemp) + ";" + result;

        print(result);
        return result;
    }

    private bool CheckActivityRules(string[] reactants, ClassReaction creaction)
    {
        Substance sub0 = dbManager.GetSubstance(reactants[0]);
        Substance sub1 = dbManager.GetSubstance(reactants[1]);
        if (sub0 == null || sub1 == null) return false;

        Substance metalSub = sub0.class_name == "metal" ? sub0 : (sub1.class_name == "metal" ? sub1 : null);
        Substance otherSub = metalSub == sub0 ? sub1 : sub0;

        if (metalSub == null) return true;

        string metalElement = dbManager.GetElements(metalSub.formula).FirstOrDefault();

        // Металл + Кислота -> Металл должен стоять левее водорода (H)
        if (otherSub.class_name == "acid")
        {
            return dbManager.IsMoreActive(metalElement, "H");
        }

        // Металл + Соль -> Металл должен быть активнее металла в составе соли
        if (otherSub.class_name.EndsWith("salt"))
        {
            string saltMetal = dbManager.GetElements(otherSub.formula)
                .FirstOrDefault(el => dbManager.GetSubstance(el)?.class_name == "metal");
            if (!string.IsNullOrEmpty(saltMetal))
            {
                return dbManager.IsMoreActive(metalElement, saltMetal);
            }
        }

        return true;
    }

    private bool TryResolveClassProducts(
        string[] reactants, string[] prodTemplates, HashSet<string> reactantElements,
        out string[] pFormula, out int[] rCoeffs, out int[] pCoeffs)
    {
        pFormula = null;
        rCoeffs = null;
        pCoeffs = null;

        List<List<Substance>> candidatesPerSlot = new List<List<Substance>>();

        for (int i = 0; i < prodTemplates.Length; i++)
        {
            string template = prodTemplates[i];
            if (dbManager.IsClass(template))
            {
                if (prodTemplates.Length == 1)
                {
                    Substance exact = dbManager.GetSubstance(template, reactantElements.ToArray());
                    if (exact == null) return false;
                    candidatesPerSlot.Add(new List<Substance> { exact });
                }
                else
                {
                    var candidates = dbManager.GetSubstancesByClass(template)
                        .Where(s => !reactants.Contains(s.formula) &&
                                    dbManager.GetElements(s.formula).All(el => reactantElements.Contains(el)))
                        .ToList();

                    if (candidates.Count == 0 && template == "basic_salt")
                    {
                        candidates = dbManager.GetSubstancesByClass("neutral_salt")
                            .Where(s => !reactants.Contains(s.formula) &&
                                        dbManager.GetElements(s.formula).All(el => reactantElements.Contains(el)))
                            .ToList();
                    }

                    if (candidates.Count == 0) return false;
                    candidatesPerSlot.Add(candidates);
                }
            }
            else
            {
                Substance fixedSub = dbManager.GetSubstance(template);
                if (fixedSub == null) return false;
                candidatesPerSlot.Add(new List<Substance> { fixedSub });
            }
        }

        List<Substance> currentSelection = new List<Substance>();
        return SearchAndBalanceCombinations(reactants, candidatesPerSlot, 0, currentSelection, reactantElements, out pFormula, out rCoeffs, out pCoeffs);
    }

    private bool SearchAndBalanceCombinations(
        string[] reactants, List<List<Substance>> candidatesPerSlot, int slotIndex,
        List<Substance> currentSelection, HashSet<string> reactantElements,
        out string[] pFormula, out int[] rCoeffs, out int[] pCoeffs)
    {
        pFormula = null;
        rCoeffs = null;
        pCoeffs = null;

        if (slotIndex == candidatesPerSlot.Count)
        {
            HashSet<string> prodElements = new HashSet<string>();
            HashSet<string> formulas = new HashSet<string>();
            foreach (var sub in currentSelection)
            {
                if (!formulas.Add(sub.formula)) return false;
                prodElements.UnionWith(dbManager.GetElements(sub.formula));
            }

            if (!prodElements.SetEquals(reactantElements))
                return false;

            string[] candidateFormulas = currentSelection.Select(s => s.formula).ToArray();
            if (BalanceEquation(reactants, candidateFormulas, out rCoeffs, out pCoeffs))
            {
                pFormula = candidateFormulas;
                return true;
            }
            return false;
        }

        foreach (Substance candidate in candidatesPerSlot[slotIndex])
        {
            currentSelection.Add(candidate);
            if (SearchAndBalanceCombinations(reactants, candidatesPerSlot, slotIndex + 1, currentSelection, reactantElements, out pFormula, out rCoeffs, out pCoeffs))
            {
                return true;
            }
            currentSelection.RemoveAt(currentSelection.Count - 1);
        }

        return false;
    }

    private bool BalanceEquation(string[] reactants, string[] products, out int[] rCoeffs, out int[] pCoeffs)
    {
        rCoeffs = null;
        pCoeffs = null;

        var rCounts = reactants.Select(r => dbManager.ParseFormula(r)).ToArray();
        var pCounts = products.Select(p => dbManager.ParseFormula(p)).ToArray();
        var allElements = rCounts.SelectMany(d => d.Keys).Distinct().ToArray();

        int maxCoeff = 12;

        for (int r1 = 1; r1 <= maxCoeff; r1++)
        {
            for (int r2 = 1; r2 <= maxCoeff; r2++)
            {
                if (products.Length == 1)
                {
                    for (int p1 = 1; p1 <= maxCoeff; p1++)
                    {
                        if (IsBalanced(allElements, rCounts, new[] { r1, r2 }, pCounts, new[] { p1 }))
                        {
                            int gcd = GCD(GCD(r1, r2), p1);
                            rCoeffs = new[] { r1 / gcd, r2 / gcd };
                            pCoeffs = new[] { p1 / gcd };
                            return true;
                        }
                    }
                }
                else if (products.Length == 2)
                {
                    for (int p1 = 1; p1 <= maxCoeff; p1++)
                    {
                        for (int p2 = 1; p2 <= maxCoeff; p2++)
                        {
                            if (IsBalanced(allElements, rCounts, new[] { r1, r2 }, pCounts, new[] { p1, p2 }))
                            {
                                int gcd = GCD(GCD(r1, r2), GCD(p1, p2));
                                rCoeffs = new[] { r1 / gcd, r2 / gcd };
                                pCoeffs = new[] { p1 / gcd, p2 / gcd };
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    private bool IsBalanced(
        string[] elements,
        Dictionary<string, int>[] rCounts, int[] rCoeffs,
        Dictionary<string, int>[] pCounts, int[] pCoeffs)
    {
        foreach (string el in elements)
        {
            int left = 0;
            for (int i = 0; i < rCounts.Length; i++)
            {
                if (rCounts[i].TryGetValue(el, out int c))
                    left += c * rCoeffs[i];
            }

            int right = 0;
            for (int i = 0; i < pCounts.Length; i++)
            {
                if (pCounts[i].TryGetValue(el, out int c))
                    right += c * pCoeffs[i];
            }

            if (left != right) return false;
        }
        return true;
    }

    private int GCD(int a, int b)
    {
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }
        return a;
    }
}