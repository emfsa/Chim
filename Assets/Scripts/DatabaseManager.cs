using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Table("elements")]
public class CElement
{
    [PrimaryKey] public int num { get; set; }
    public string name { get; set; }
    public float atomic_mass { get; set; }
}

[Table("classes")]
public class ClassEntity
{
    [PrimaryKey, Column("class")] public string class_name { get; set; }
}

[Table("substances")]
public class Substance
{
    [PrimaryKey] public string formula { get; set; }
    [Column("class")] public string class_name { get; set; } 
    public float melt_temp { get; set; }
    public float boil_temp { get; set; }
    public float heat_capacity { get; set; }
    public float solubility { get; set; }
}

[Table("color")]
public class SubstanceColor
{
    [PrimaryKey, AutoIncrement] public int c_id { get; set; }
    public string formula { get; set; } 
    public string state { get; set; }   
    public string hex { get; set; }     
}

[Table("activity")]
public class Activity
{
    [PrimaryKey] public string element { get; set; } 
    public int place { get; set; }
}

[Table("reactions")]
public class Reaction
{
    [PrimaryKey, AutoIncrement] public int r_id { get; set; }
    public string reagents { get; set; }
    public string products { get; set; }
    public string r_moles { get; set; }
    public string p_moles { get; set; }
    public string catalyst { get; set; }
    public int min_temp { get; set; }
}

[Table("class_reactions")]
public class ClassReaction
{
    [PrimaryKey, AutoIncrement] public int r_id { get; set; }
    public string reagents { get; set; }
    public string products { get; set; }
}

public class DatabaseManager : MonoBehaviour
{
    private SQLiteConnection db;

    private void Awake()
    {
        string dbPath = Path.Combine(Application.streamingAssetsPath, "chemistry.db");

        if (!File.Exists(dbPath))
        {
            Debug.LogError($"База данных не найдена: {dbPath}");
            return;
        }

        db = new SQLiteConnection(dbPath);
    }

    public Reaction FindReactionSorted(string[] input)
    {
        string key = string.Join(",", input.OrderBy(p => p).ToArray());
        return db.Table<Reaction>().AsEnumerable().FirstOrDefault(r =>
            string.Join(",", r.reagents.Split(',').OrderBy(p => p).ToArray()) == key);
    }

    public Reaction FindReaction(string[] input)
    {
        string key = string.Join(",", input);
        return db.Table<Reaction>().Where(r => r.reagents == key).FirstOrDefault();
    }

    public Substance GetSubstance(string formulaInput)
    {
        var substance = db.Table<Substance>().Where(f => f.formula == formulaInput).FirstOrDefault();
        if (substance == null)
        {
            Debug.LogWarning("Вещество " + formulaInput + " не найдено");
        }
        return substance;
    }

    public string GetColor(string formulaInput, string state = null)
    {
        List<SubstanceColor> hex = db.Table<SubstanceColor>().Where(f => f.formula == formulaInput).ToList();
        if (hex.Count == 1) return hex[0].hex;
        for (int i = 0; i < hex.Count; i++)
        {
            if (hex[i].state == state) return hex[i].hex;
        }
        return "#FFFFFF";
    }

    private string GetReagentClassOrFormula(string formula)
    {
        if (formula == "O2" || formula == "H2O" || formula == "H2")
            return formula;
        Substance sub = GetSubstance(formula);
        return sub != null ? sub.class_name : "";
    }

    public ClassReaction GetClassReactionSet(string[] input)
    {
        return GetClassReactions(input).FirstOrDefault();
    }

    public ClassReaction GetClassReaction(string[] input)
    {
        string[] classes = input.Select(GetReagentClassOrFormula).ToArray();
        string key = string.Join(",", classes);
        var exact = db.Table<ClassReaction>().Where(f => f.reagents == key).FirstOrDefault();
        return exact ?? GetClassReactionSet(input);
    }

    public List<ClassReaction> GetClassReactions(string[] input)
    {
        string[] classes = input.Select(GetReagentClassOrFormula).ToArray();
        string sortedKey = string.Join(",", classes.OrderBy(c => c));

        List<ClassReaction> matches = db.Table<ClassReaction>().AsEnumerable()
            .Where(f => string.Join(",", f.reagents.Split(',').OrderBy(c => c)) == sortedKey)
            .ToList();

        if (matches.Count == 0)
        {
            string[] fallbackClasses = input.Select(f => GetSubstance(f)?.class_name ?? "").ToArray();
            string fallbackKey = string.Join(",", fallbackClasses.OrderBy(c => c));
            matches = db.Table<ClassReaction>().AsEnumerable()
                .Where(f => string.Join(",", f.reagents.Split(',').OrderBy(c => c)) == fallbackKey)
                .ToList();
        }

        return matches;
    }

    public bool IsClass(string input)
    {
        return db.Table<ClassEntity>().Where(f => f.class_name == input).FirstOrDefault() != null;
    }

    public string GetClass(string input)
    {
        return db.Table<ClassEntity>().Where(f => f.class_name == input).FirstOrDefault()?.class_name;
    }

    public Dictionary<string, int> ParseFormula(string formula)
    {
        Stack<Dictionary<string, int>> stack = new Stack<Dictionary<string, int>>();
        stack.Push(new Dictionary<string, int>());

        int i = 0;
        while (i < formula.Length)
        {
            if (formula[i] == '(')
            {
                stack.Push(new Dictionary<string, int>());
                i++;
            }
            else if (formula[i] == ')')
            {
                i++;
                int start = i;
                while (i < formula.Length && char.IsDigit(formula[i])) i++;
                int mult = start < i ? int.Parse(formula.Substring(start, i - start)) : 1;

                var popped = stack.Pop();
                var top = stack.Peek();
                foreach (var kvp in popped)
                {
                    if (!top.ContainsKey(kvp.Key)) top[kvp.Key] = 0;
                    top[kvp.Key] += kvp.Value * mult;
                }
            }
            else if (char.IsUpper(formula[i]))
            {
                int start = i;
                i++;
                while (i < formula.Length && char.IsLower(formula[i])) i++;
                string element = formula.Substring(start, i - start);

                start = i;
                while (i < formula.Length && char.IsDigit(formula[i])) i++;
                int count = start < i ? int.Parse(formula.Substring(start, i - start)) : 1;

                var top = stack.Peek();
                if (!top.ContainsKey(element)) top[element] = 0;
                top[element] += count;
            }
            else
            {
                i++;
            }
        }

        return stack.Pop();
    }

    public string[] GetElements(string formula)
    {
        return ParseFormula(formula).Keys.ToArray();
    }

    public Substance GetSubstance(string class_name, string[] elements)
    {
        HashSet<string> targetElements = new HashSet<string>(elements);
        List<Substance> subs = db.Table<Substance>().Where(f => f.class_name == class_name).AsEnumerable().ToList();

        foreach (var sub in subs)
        {
            HashSet<string> subElements = new HashSet<string>(GetElements(sub.formula));
            if (subElements.SetEquals(targetElements))
            {
                return sub;
            }
        }
        return null;
    }

    public List<Substance> GetSubstancesByClass(string class_name)
    {
        return db.Table<Substance>().Where(f => f.class_name == class_name).AsEnumerable().ToList();
    }

    public bool IsMoreActive(string el1, string el2)
    {
        var a1 = db.Table<Activity>().Where(a => a.element == el1).FirstOrDefault();
        var a2 = db.Table<Activity>().Where(a => a.element == el2).FirstOrDefault();
        if (a1 == null || a2 == null) return false;
        return a1.place < a2.place;
    }

    public float GetMolarMass(string inputFormula)
    {
        Substance sub = GetSubstance(inputFormula);
        string formula = sub != null ? sub.formula : inputFormula;
        Dictionary<string, int> parsed = ParseFormula(formula);

        float molar_mass = 0f;
        foreach (var kvp in parsed)
        {
            var element = db.Table<CElement>().Where(f => f.name == kvp.Key).FirstOrDefault();
            if (element != null)
            {
                molar_mass += element.atomic_mass * kvp.Value;
            }
        }
        return molar_mass;
    }
}

