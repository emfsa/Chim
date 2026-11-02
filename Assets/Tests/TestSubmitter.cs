using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestSubmitter : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject resultScreen;
    [SerializeField] private GameObject TestScreen;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text manualText;
    public void SubmitTest()
    {
        int score = 0;
        int automaticTotal = 0;
        int manualCount = 0;

        QuestCardData[] cards =
            content.GetComponentsInChildren<QuestCardData>();

        foreach (QuestCardData card in cards)
        {
            AnswerKeyInfo key =
                TestDataBaseServ.GetAnswerKey(card.QuestionId);

            if (key == null)
            {
                continue;
            }

            if (key.needs_manual_review == 1)
            {
                manualCount++;
                continue;
            }

            automaticTotal += card.Points;

            if (IsAnswerCorrect(card, key))
            {
                score += card.Points;
            }
        }

        
        scoreText.text = "Правильных ответов: " + score + " из " + automaticTotal;

        manualText.text = manualCount > 0
            ? "Заданий для ручной проверки: " + manualCount
            : "Все задания проверены автоматически.";

        resultScreen.SetActive(true);
        TestScreen.SetActive(false);
    }

    private bool IsAnswerCorrect(
        QuestCardData card,
        AnswerKeyInfo key
    )
    {
        if (card.QuestionType == "single_choice")
        {
            AnswerToggleData[] toggles =
                card.GetComponentsInChildren<AnswerToggleData>();

            foreach (AnswerToggleData toggleData in toggles)
            {
                Toggle toggle = toggleData.GetComponent<Toggle>();

                if (toggle != null && toggle.isOn)
                {
                    return IsAccepted(
                        toggleData.OptionCode,
                        key
                    );
                }
            }

            return false;
        }

        if (card.QuestionType == "matching")
        {
            MatchingRowData[] rows =
                card.GetComponentsInChildren<MatchingRowData>();

            if (rows.Length == 0)
            {
                return false;
            }

            foreach (MatchingRowData row in rows)
            {
                TMP_InputField input =
                    row.GetComponentInChildren<TMP_InputField>();

                if (input == null ||
                    Normalize(input.text) !=
                    Normalize(row.CorrectValue))
                {
                    return false;
                }
            }

            return true;
        }

        if (card.QuestionType == "short_text")
        {
            TMP_InputField input =
                card.GetComponentInChildren<TMP_InputField>();

            if (input == null)
            {
                return false;
            }

            return IsAccepted(input.text, key);
        }

        return false;
    }

    private bool IsAccepted(
        string userAnswer,
        AnswerKeyInfo key
    )
    {
        if (Normalize(userAnswer) ==
            Normalize(key.correct_value))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(key.accepted_values))
        {
            return false;
        }

        string[] acceptedAnswers =
            key.accepted_values.Split(';');

        foreach (string answer in acceptedAnswers)
        {
            if (Normalize(userAnswer) ==
                Normalize(answer))
            {
                return true;
            }
        }

        return false;
    }

    private string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string result = string.Empty;

        foreach (char symbol in value.ToUpperInvariant())
        {
            if (char.IsLetterOrDigit(symbol))
            {
                result += symbol;
            }
        }

        return result;
    }
    public void HideResult()
    {
        resultScreen.SetActive(false);
    }
}