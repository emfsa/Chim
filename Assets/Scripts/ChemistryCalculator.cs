using SQLite;
using UnityEngine;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using System;

public class ChemistryCalculator : MonoBehaviour
{
    public DatabaseManager dbManager;

    public string Process(string[] reactants, float[] masses)
    {
        Reaction reaction = dbManager.FindReaction(reactants);
        if (reaction == null) { return "Реакция не найдена"; }

        int[] rCoeffs = reaction.reactantcoeffs.Split(',').Select(int.Parse).ToArray();
        print(rCoeffs[0].ToString() + rCoeffs[1].ToString());
        string[] rFormula = reaction.reactants.Split(',');
        print(reaction.reactants);
        float minMolesReaction = float.MaxValue;

        //for (int i = 0; i < reactants.Length; i++)
        //{
        //    Substance sub = dbManager.GetSubstance(reactants[i]);
        //    print(sub.molar_mass);
        //    float rmoles = masses[i] / sub.molar_mass;
        //    print(rmoles);
        //    int rcoeff = rCoeffs[System.Array.IndexOf(rFormula, reactants[i])];

        //    print(rcoeff);

        //    float reactionmoles = rmoles / rCoeffs[rcoeff-1];

        //    if (reactionmoles < minMolesReaction) minMolesReaction = reactionmoles;
        //}

        for (int i = 0; i < reactants.Length; i++)
        {
            Substance sub = dbManager.GetSubstance(reactants[i]);
            print(sub.molar_mass);
            float rmoles = masses[i] / sub.molar_mass;
            print(rmoles);
            int rcoeff = rCoeffs[System.Array.IndexOf(rFormula, reactants[i])];

            print(rcoeff);

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

        print(result);
        return result;
    }
}