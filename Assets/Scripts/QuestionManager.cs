using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] private QuestionList questionList;
    [SerializeField] private bool selectQuestionOnStart;
    [SerializeField] private bool avoidImmediateRepeat = true;

    private Question currentQuestion;
    private int currentQuestionIndex = -1;

    public Question CurrentQuestion => currentQuestion;
    public int CurrentQuestionIndex => currentQuestionIndex;
    public bool HasQuestion => currentQuestion != null;

    private void Start()
    {
        if (selectQuestionOnStart)
        {
            SelectRandomQuestion();
        }
    }

    public Question SelectRandomQuestion()
    {
        if (questionList == null || questionList.Questions.Count == 0)
        {
            currentQuestion = null;
            currentQuestionIndex = -1;
            Debug.LogWarning($"{nameof(QuestionManager)} needs a question list with at least one question.", this);
            return null;
        }

        int nextIndex = GetRandomQuestionIndex();
        currentQuestionIndex = nextIndex;
        currentQuestion = questionList.Questions[nextIndex];

        return currentQuestion;
    }

    private int GetRandomQuestionIndex()
    {
        int questionCount = questionList.Questions.Count;

        if (!avoidImmediateRepeat || questionCount <= 1 || currentQuestionIndex < 0)
        {
            return Random.Range(0, questionCount);
        }

        int nextIndex = Random.Range(0, questionCount - 1);
        if (nextIndex >= currentQuestionIndex)
        {
            nextIndex++;
        }

        return nextIndex;
    }
}
