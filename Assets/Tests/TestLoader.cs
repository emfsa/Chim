using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestLoader : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform content;
    [SerializeField] private Transform testTitle;

    [Header("Prefabs")]
    [SerializeField] private GameObject questionCardPrefab;
    [SerializeField] private GameObject answerTogglePrefab;
    [SerializeField] private GameObject matchingRowPrefab;
    [SerializeField] private GameObject textAnswerInputPrefab;
    private void Start()
    {
        LoadTest();
    }

    private void LoadTest()
    {
        int testId = TestSession.SelectedTestId;

        if (testId <= 0)
        {
            Debug.LogError("Не выбран тест.");
            return;
        }

        TestInfo test = TestDataBaseServ.GetTest(testId);


        SetText(testTitle, test.title);

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        var questions = TestDataBaseServ.GetQuestions(testId);

        foreach (var question in questions)
        {
            GameObject card = Instantiate(questionCardPrefab, content);
            card.name = "Question_" + question.position;
            QuestCardData cardData =card.AddComponent<QuestCardData>();

            cardData.Setup(
                question.question_id,
                question.question_type,
                question.points
            );
            SetText(
                card.transform.Find("QuestionNumberText"),
                "Задание " + question.position
            );

            SetText(
                card.transform.Find("QuestionText"),
                question.question_text
            );

            Transform answerArea = card.transform.Find("AnswerArea");

            if (question.question_type == "matching")
            {
                var matchingItems = TestDataBaseServ.GetMatchingItems(
                    question.question_id
                );

                foreach (var item in matchingItems)
                {
                    GameObject row = Instantiate(
                        matchingRowPrefab,
                        answerArea
                    );

                    row.name = "Match_" + item.item_code;

                    MatchingRowData rowData = row.AddComponent<MatchingRowData>();

                    rowData.Setup(
                        item.item_code,
                        item.correct_option_code
                    );

                    SetText(
                        row.transform.Find("ItemText"),
                        item.item_code + ". " + item.item_text
                    );
                }

                continue;
            }
            if (question.question_type == "short_text" ||
    question.question_type == "long_text")
            {
                GameObject inputObject = Instantiate(
                    textAnswerInputPrefab,
                    answerArea
                );

                TMP_InputField inputField =
                    inputObject.GetComponent<TMP_InputField>();

                LayoutElement layoutElement =
                    inputObject.GetComponent<LayoutElement>();

                bool isLongText = question.question_type == "long_text";

                if (inputField != null)
                {
                    inputField.lineType = isLongText
                        ? TMP_InputField.LineType.MultiLineNewline
                        : TMP_InputField.LineType.SingleLine;

                    inputField.characterLimit = isLongText ? 1000 : 100;
                }

                if (layoutElement != null)
                {
                    layoutElement.preferredHeight = isLongText ? 180 : 58;
                }

                continue;
            }
            var options = TestDataBaseServ.GetOptions(
                question.question_id
            );

            ToggleGroup group = null;

            if (question.question_type == "single_choice")
            {
                group = answerArea.GetComponent<ToggleGroup>();

                if (group == null)
                {
                    group = answerArea.gameObject.AddComponent<ToggleGroup>();
                }

                group.allowSwitchOff = true;
            }

            foreach (var option in options)
            {
                GameObject toggleObject = Instantiate(
                    answerTogglePrefab,
                    answerArea
                );
                AnswerToggleData toggleData = toggleObject.AddComponent<AnswerToggleData>();

                toggleData.Setup(option.option_code);

                Toggle toggle = toggleObject.GetComponent<Toggle>();

                if (toggle != null)
                {
                    toggle.group = group;
                    toggle.isOn = false;
                }

                SetText(
                    toggleObject.transform.Find("Label"),
                    option.option_code + ") " + option.option_text
                );
            }
        }
    }

    private void SetText(Transform target, string value)
    {
        if (target == null)
        {
            return;
        }

        TMP_Text tmpText = target.GetComponent<TMP_Text>();

        if (tmpText != null)
        {
            tmpText.text = value;
            return;
        }

        UnityEngine.UI.Text normalText =
            target.GetComponent<UnityEngine.UI.Text>();

        if (normalText != null)
        {
            normalText.text = value;
        }
    }
}