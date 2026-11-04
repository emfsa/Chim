using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SQLite;
public class TestDataBaseServ
{
    private static SQLiteConnection connection;

    private static SQLiteConnection Connection
    {
        get
        {
            if (connection == null)
            {
                string path = Path.Combine(
                    Application.streamingAssetsPath,
                    "TestsDatabase.db"
                );

                if (!File.Exists(path))
                {
                    Debug.LogError("База тестов не найдена: " + path);
                    return null;
                }

                connection = new SQLiteConnection(
                    path,
                    SQLiteOpenFlags.ReadOnly
                );

                Debug.Log("База тестов подключена: " + path);
            }

            return connection;
        }
    }

    public static List<TestInfo> GetAllTests()
    {
        if (Connection == null)
            return new List<TestInfo>();

        return Connection.Query<TestInfo>(
            @"SELECT test_id, grade, topic, test_number, title
              FROM tests
              ORDER BY grade, test_number"
        );
    }

    public static void Close()
    {
        connection?.Close();
        connection = null;
    }

    public static TestInfo GetTest(int testId)
    {
        var result = Connection.Query<TestInfo>(
            @"SELECT test_id, grade, topic, test_number, title
          FROM tests
          WHERE test_id = ?",
            testId
        );

        return result.Count > 0 ? result[0] : null;
    }

    public static List<QuestInfo> GetQuestions(int testId)
    {
        return Connection.Query<QuestInfo>(
            @"SELECT question_id, test_id, position,
                 question_type, question_text, points
          FROM questions
          WHERE test_id = ?
          ORDER BY position",
            testId
        );
    }

    public static List<AnswerOptionInfo> GetOptions(int questionId)
    {
        return Connection.Query<AnswerOptionInfo>(
            @"SELECT option_id, question_id,
                 option_code, option_text, position
          FROM answer_options
          WHERE question_id = ?
          ORDER BY position",
            questionId
        );
    }
    public static List<MatchingItemInfo> GetMatchingItems(int questionId)
    {
        if (Connection == null)
        {
            return new List<MatchingItemInfo>();
        }

        return Connection.Query<MatchingItemInfo>(
            @"SELECT matching_item_id, question_id,
                 item_code, item_text,
                 correct_option_code, position
          FROM matching_items
          WHERE question_id = ?
          ORDER BY position",
            questionId
        );
    }
    public static AnswerKeyInfo GetAnswerKey(int questionId)
    {
        if (Connection == null)
        {
            return null;
        }

        var result = Connection.Query<AnswerKeyInfo>(
            @"SELECT answer_key_id, question_id,
                 correct_value, accepted_values,
                 grading_criteria, needs_manual_review
          FROM answer_keys
          WHERE question_id = ?",
            questionId
        );

        return result.Count > 0 ? result[0] : null;
    }
}

