using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionList", menuName = "MonGame/Question List")]
public class QuestionList : ScriptableObject
{
    [SerializeField] private List<Question> questions = new List<Question>();

    public IReadOnlyList<Question> Questions => questions;
}

[Serializable]
public class Question
{
    public Sprite questionImage;

    [TextArea]
    public string questionText;

    [TextArea]
    public string answerBodyText;

    public List<Answer> answers = new List<Answer>();
}

[Serializable]
public class Answer
{
    public bool isCorrect;

    [TextArea]
    public string answerText;
}
