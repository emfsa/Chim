using UnityEngine;

public class QuestCardData : MonoBehaviour
{
    public int QuestionId { get; private set; }
    public string QuestionType { get; private set; }
    public int Points { get; private set; }

    public void Setup(int questionId, string questionType, int points)
    {
        QuestionId = questionId;
        QuestionType = questionType;
        Points = points;
    }
}
