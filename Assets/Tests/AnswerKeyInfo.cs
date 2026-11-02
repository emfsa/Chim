using UnityEngine;

public class AnswerKeyInfo
{
    public int answer_key_id { get; set; }
    public int question_id { get; set; }
    public string correct_value { get; set; }
    public string accepted_values { get; set; }
    public string grading_criteria { get; set; }
    public int needs_manual_review { get; set; }
}
