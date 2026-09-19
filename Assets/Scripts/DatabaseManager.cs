using SQLite;
using UnityEngine;
using System.IO;
using System.Linq;
using Unity.VisualScripting;

[Table("substances")]
public class Substance
{
    [PrimaryKey] public string formula { get; set; }
    public string name { get; set; }
    public float molar_mass { get; set; }
    public string state { get; set; }
    public string color { get; set; }

}

[Table("reactions")]
public class Reaction
{
    [PrimaryKey, AutoIncrement] public int r_id { get; set; }
    public string reactants { get; set; }
    public string products { get; set; }
    public string reactantcoeffs { get; set; }
    public string productcoeffs { get; set; }
    public string conditions { get; set; }

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

        if (db == null )
        {
            Debug.LogError($"База данных не найдена: {dbPath}");
            return;
        }
    }

    public Reaction FindReaction(string[] input)
    {
        //System.Array.Sort(input);
        string Key = string.Join(",", input);
        return db.Table<Reaction>().Where(r => r.reactants == Key).FirstOrDefault();
    }

    public Substance GetSubstance(string input)
    {   
        var substance = db.Table<Substance>().Where(f => f.formula == input).FirstOrDefault();
        if (substance == null)
        {
            print("Вещество " + input + " не найдено");
        }
        return substance;
    }

}

