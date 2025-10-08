
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject quizPanel;
    public GameObject resultPanel;

    [Header("Quiz UI Elements")]
    public TextMeshProUGUI questionText;
    public Button[] answerButtons; // 0=A, 1=B, 2=C, 3=D
    public TextMeshProUGUI resultText;
    [Header("Start UI")]
    public Button startButton; // optional: auto-wire StartQuiz

    [Header("Quiz Data")]
    public List<QuestionData> questions = new List<QuestionData>();

    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private int selectedAnswer = -1;
    private int totalQuestions = 10;

    private void LogFieldStatus()
    {
        Debug.Log($"[QuizManager] startPanel: {(startPanel != null)}");
        Debug.Log($"[QuizManager] quizPanel: {(quizPanel != null)}");
        Debug.Log($"[QuizManager] resultPanel: {(resultPanel != null)}");
        Debug.Log($"[QuizManager] questionText: {(questionText != null)}");
        Debug.Log($"[QuizManager] resultText: {(resultText != null)}");
        Debug.Log($"[QuizManager] answerButtons: {(answerButtons != null ? answerButtons.Length.ToString() : "null")}");
    }

    private void Awake()
    {
        if (questions == null || questions.Count < 10)
        {
            questions = new List<QuestionData>
            {
                new QuestionData {
                    questionText = "Which SQL statement is used to extract data from a database?",
                    choices = new string[] { "GET", "OPEN", "SELECT", "EXTRACT" },
                    correctAnswerIndex = 2
                },
                new QuestionData {
                    questionText = "What does SQL stand for?",
                    choices = new string[] { "Structured Query Language", "Strong Question Language", "Simple Query List", "Structured Question Language" },
                    correctAnswerIndex = 0
                },
                new QuestionData {
                    questionText = "Which command is used to remove all records from a table in SQL, but not the table itself?",
                    choices = new string[] { "DROP", "DELETE", "REMOVE", "ERASE" },
                    correctAnswerIndex = 1
                },
                new QuestionData {
                    questionText = "Which of the following is a valid SQL data type?",
                    choices = new string[] { "INTEGER", "NUMBERIC", "CHARACTERS", "ALPHANUMERIC" },
                    correctAnswerIndex = 0
                },
                new QuestionData {
                    questionText = "What is a primary key?",
                    choices = new string[] { "A unique identifier for a record", "A foreign key", "A duplicate value", "A table name" },
                    correctAnswerIndex = 0
                },
                new QuestionData {
                    questionText = "Which SQL clause is used to filter the results?",
                    choices = new string[] { "ORDER BY", "WHERE", "GROUP BY", "HAVING" },
                    correctAnswerIndex = 1
                },
                new QuestionData {
                    questionText = "Which keyword is used to sort the result-set in SQL?",
                    choices = new string[] { "SORT BY", "ORDER", "ORDER BY", "SORT" },
                    correctAnswerIndex = 2
                },
                new QuestionData {
                    questionText = "Which SQL statement is used to update data in a database?",
                    choices = new string[] { "MODIFY", "UPDATE", "CHANGE", "SET" },
                    correctAnswerIndex = 1
                },
                new QuestionData {
                    questionText = "What is a foreign key?",
                    choices = new string[] { "A key from another table", "A unique identifier", "A duplicate value", "A table name" },
                    correctAnswerIndex = 0
                },
                new QuestionData {
                    questionText = "Which SQL function is used to count the number of rows in a table?",
                    choices = new string[] { "SUM()", "COUNT()", "TOTAL()", "NUMBER()" },
                    correctAnswerIndex = 1
                }
            };
        }
    }

    void Start()
    {
        Debug.Log("[QuizManager] Start() - Checking field assignments...");
        LogFieldStatus();
        if (EventSystem.current == null)
        {
            Debug.LogWarning("[QuizManager] No EventSystem found in scene. UI buttons will not work.");
        }
        ShowPanel(startPanel);
        quizPanel.SetActive(false);
        resultPanel.SetActive(false);
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartQuiz);
        }
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int idx = i;
            answerButtons[i].onClick.AddListener(() => SelectAnswer(idx));
            // Set button label to A, B, C, D
            var tmp = answerButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = ((char)('A' + i)).ToString();
        }
    }

    public void StartQuiz()
    {
        Debug.Log("[QuizManager] StartQuiz() called");
        currentQuestionIndex = 0;
        correctAnswers = 0;
        selectedAnswer = -1;
        ShuffleQuestions();
        ShowPanel(quizPanel);
        resultPanel.SetActive(false);
        ShowQuestion();
    }

    void ShowPanel(GameObject panel)
    {
        Debug.Log($"[QuizManager] ShowPanel: {panel.name}");
        startPanel.SetActive(panel == startPanel);
        quizPanel.SetActive(panel == quizPanel);
        resultPanel.SetActive(panel == resultPanel);
    }

    void ShowQuestion()
    {
        Debug.Log($"[QuizManager] ShowQuestion() - Q{currentQuestionIndex+1}");
        if (currentQuestionIndex >= totalQuestions)
        {
            ShowResults();
            return;
        }
        var q = questions[currentQuestionIndex];
        // Format question and choices as a single string
        questionText.text = q.questionText + "\n\n" +
            "A. " + q.choices[0] + "\n" +
            "B. " + q.choices[1] + "\n" +
            "C. " + q.choices[2] + "\n" +
            "D. " + q.choices[3];
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = true;
        }
        selectedAnswer = -1;
    }

    void SelectAnswer(int idx)
    {
        Debug.Log($"[QuizManager] SelectAnswer({idx})");
        selectedAnswer = idx;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = false;
        }
        // Automatically go to next question after a short delay
        Invoke(nameof(NextQuestion), 0.5f);
        if (selectedAnswer == questions[currentQuestionIndex].correctAnswerIndex)
        {
            correctAnswers++;
        }
    }

    void NextQuestion()
    {
        Debug.Log("[QuizManager] NextQuestion()");
        currentQuestionIndex++;
        ShowQuestion();
    }

    void ShowResults()
    {
        Debug.Log("[QuizManager] ShowResults()");
        ShowPanel(resultPanel);
        float percent = (correctAnswers / (float)totalQuestions) * 100f;
        resultText.text = $"Result: {correctAnswers}/{totalQuestions} ({percent:F0}%)";
    }

    void ShuffleQuestions()
    {
        Debug.Log("[QuizManager] ShuffleQuestions()");
        for (int i = 0; i < questions.Count; i++)
        {
            var temp = questions[i];
            int randomIndex = Random.Range(i, questions.Count);
            questions[i] = questions[randomIndex];
            questions[randomIndex] = temp;
        }
    }
}
