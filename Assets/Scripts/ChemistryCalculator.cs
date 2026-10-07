using SQLite;
using UnityEngine;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using System;

public class ChemistryCalculator : MonoBehaviour
{
    public DatabaseManager dbManager;

    public string Process(string[] reactants, float[] masses, float[] temperatures)
    {
        Reaction reaction = dbManager.FindReaction(reactants);
        if (reaction == null) { return ""; }

        int[] rCoeffs = reaction.reactantcoeffs.Split(',').Select(int.Parse).ToArray();
        string[] rFormula = reaction.reactants.Split(',');
        float minMolesReaction = float.MaxValue;

        float all_hc = 0;
        float Q = reaction.em_temperature;

        for (int i = 0; i < reactants.Length; i++)
        {
            if (reaction.catalyst == null) break;
            string[] catalyst = reaction.catalyst.Split(',');
            string form = reactants[i];
            if (form == catalyst[0]) 
            {
                reactants = reactants[0..i];
                rCoeffs = rCoeffs[0..i];
                rFormula = rFormula[0..i];
                break;
            }
        }



        for (int i = 0; i < reactants.Length; i++)
        {
            Substance sub = dbManager.GetSubstance(reactants[i]);
            all_hc += sub.heat_cap * rCoeffs[i];
            Q += (temperatures[i]+273) * sub.heat_cap * rCoeffs[i];
        }

        if (Q/all_hc - 273 < reaction.min_temperature)
            return "";

        for (int i = 0; i < reactants.Length; i++)
        {
            Substance sub = dbManager.GetSubstance(reactants[i]);
            float rmoles = masses[i] / sub.molar_mass;
            int rcoeff = rCoeffs[System.Array.IndexOf(rFormula, reactants[i])];
            float reactionmoles = rmoles / rcoeff;

            if (reactionmoles < minMolesReaction) minMolesReaction = reactionmoles;
        }

        string[] pFormula = reaction.products.Split(",");
        int[] pCoeffs = reaction.productcoeffs.Split(",").Select(int.Parse).ToArray();

        string result = "";

        for (int i = 0; i < pFormula.Length; i++)
        {
            Substance sub = dbManager.GetSubstance(pFormula[i]);
            float moles = minMolesReaction * pCoeffs[i];

            float mass = moles * sub.molar_mass;

            result += $"{pFormula[i]};{mass};";
        }

        result += "spent;";

        for (int i = 0; i < reactants.Length; i++)
        {
            Substance sub = dbManager.GetSubstance(reactants[i]);
            float mass = sub.molar_mass * minMolesReaction * rCoeffs[i];
            result += Convert.ToString(mass) + ";";
        }

        result = "temperature;" + Convert.ToString(Q/all_hc-273) + ";" + result;

        print(result);
        return result;
    }
}