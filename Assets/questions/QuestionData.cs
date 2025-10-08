using UnityEngine;

[System.Serializable]
public class QuestionData
{
    public string questionText;
    public string[] choices = new string[4]; // a, b, c, d
    public int correctAnswerIndex; // 0 = a, 1 = b, 2 = c, 3 = d
}
