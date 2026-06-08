using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameObject questionUI;
    [SerializeField] private Image questionImage;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text answerBodyText;
    [SerializeField] private AnswerButton[] answerButtons;
    [SerializeField] private Button nextButton;
    [SerializeField] private AnswerFeedbackUI answerFeedbackUI;
    [SerializeField] private MonsterUI playerMonsterUI;
    [SerializeField] private MonsterUI enemyMonsterUI;
    [SerializeField] private bool hideQuestionUIUntilGameStarts = true;
    [SerializeField] private CanvasGroup questionUICanvasGroup;
    [SerializeField] private float questionIntroMoveOffsetY = 40f;
    [SerializeField] private float questionIntroDuration = 0.3f;

    private RectTransform questionUIRectTransform;
    private Vector3 questionUIStartPosition;
    private Vector3 questionUIStartScale;
    private Vector3 initialQuestionUIPosition;
    private Vector3 initialQuestionUIScale;
    private bool hasSavedDefaultQuestionUITransform;
    private Sequence startGameSequence;
    private Sequence questionSequence;
    private Action<Answer> onAnswerSelected;
    private Func<Question> nextQuestionProvider;
    private bool hasAnsweredCurrentQuestion;

    private void Awake()
    {
        if (questionUI != null)
        {
            questionUIRectTransform = questionUI.GetComponent<RectTransform>();
            if (questionUICanvasGroup == null)
            {
                questionUICanvasGroup = questionUI.GetComponent<CanvasGroup>();
                if (questionUICanvasGroup == null)
                {
                    questionUICanvasGroup = questionUI.AddComponent<CanvasGroup>();
                }
            }

            if (questionUIRectTransform != null)
            {
                questionUIStartPosition = questionUIRectTransform.position;
                questionUIStartScale = questionUIRectTransform.localScale;
            }
        }

        if (hideQuestionUIUntilGameStarts && questionUI != null)
        {
            questionUI.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNextQuestion);
            HideNextButton();
        }
    }

    private void OnDestroy()
    {
        startGameSequence?.Kill();
        questionSequence?.Kill();

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(ShowNextQuestion);
        }
    }

    public void StartGame(Question firstQuestion, Action<Answer> onAnswerSelected, Func<Question> nextQuestionProvider)
    {
        this.onAnswerSelected = onAnswerSelected;
        this.nextQuestionProvider = nextQuestionProvider;
        SetQuestion(firstQuestion, onAnswerSelected);

        if (firstQuestion == null)
        {
            return;
        }

        PlayStartGameSequence();
    }

    public void ShowQuestion(Question question, Action<Answer> onAnswerSelected, Func<Question> nextQuestionProvider)
    {
        this.onAnswerSelected = onAnswerSelected;
        this.nextQuestionProvider = nextQuestionProvider;
        SetQuestion(question, onAnswerSelected);

        if (question != null)
        {
            ShowQuestionUI();
        }
    }

    public void ResetGameplayUI()
    {
        startGameSequence?.Kill();
        questionSequence?.Kill();
        hasAnsweredCurrentQuestion = false;
        HideNextButton();

        if (answerFeedbackUI != null)
        {
            answerFeedbackUI.HideImmediate();
        }

        if (questionUICanvasGroup != null)
        {
            questionUICanvasGroup.alpha = 0f;
        }

        if (questionUI != null)
        {
            questionUI.SetActive(false);
        }
    }

    private void SetQuestion(Question question, Action<Answer> onAnswerSelected)
    {
        if (question == null)
        {
            return;
        }

        if (questionUI != null)
        {
            questionUI.SetActive(true);
        }

        hasAnsweredCurrentQuestion = false;
        HideNextButton();

        if (answerFeedbackUI != null)
        {
            answerFeedbackUI.HideImmediate();
        }

        if (questionImage != null)
        {
            questionImage.sprite = question.questionImage;
            questionImage.enabled = question.questionImage != null;
        }

        if (questionText != null)
        {
            questionText.text = question.questionText;
        }

        if (answerBodyText != null)
        {
            answerBodyText.text = question.answerBodyText;
        }

        SetAnswers(question);
    }

    private void SetAnswers(Question question)
    {
        if (answerButtons == null)
        {
            return;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null)
            {
                continue;
            }

            bool hasAnswer = question.answers != null && i < question.answers.Count;
            answerButtons[i].SetAnswer(hasAnswer ? question.answers[i] : null, HandleAnswerButtonSelected);
        }
    }

    private void HandleAnswerButtonSelected(AnswerButton selectedButton, Answer selectedAnswer)
    {
        bool isCorrect = selectedAnswer != null && selectedAnswer.isCorrect;
        if (hasAnsweredCurrentQuestion)
        {
            return;
        }

        hasAnsweredCurrentQuestion = true;

        if (answerButtons != null)
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] != null)
                {
                    answerButtons[i].SetInteractable(false);

                    if (!isCorrect && answerButtons[i].CurrentAnswer != null && answerButtons[i].CurrentAnswer.isCorrect)
                    {
                        answerButtons[i].ShowResult(true);
                    }
                }
            }
        }

        if (selectedButton != null)
        {
            selectedButton.ShowResult(isCorrect);
        }

        if (answerFeedbackUI != null)
        {
            answerFeedbackUI.Play(isCorrect, () =>
            {
                onAnswerSelected?.Invoke(selectedAnswer);
                ShowNextButton();
            });

            return;
        }

        onAnswerSelected?.Invoke(selectedAnswer);
        ShowNextButton();
    }

    private void ShowNextQuestion()
    {
        if (nextButton != null)
        {
            nextButton.interactable = false;
            nextButton.gameObject.SetActive(false);
        }

        Question nextQuestion = nextQuestionProvider?.Invoke();
        if (nextQuestion == null)
        {
            return;
        }

        questionSequence?.Kill();

        if (questionUICanvasGroup == null)
        {
            SetQuestion(nextQuestion, onAnswerSelected);
            ShowQuestionUI();
            return;
        }

        questionSequence = DOTween.Sequence()
            .Append(questionUICanvasGroup.DOFade(0f, questionIntroDuration).SetEase(Ease.InQuad))
            .AppendCallback(() =>
            {
                SetQuestion(nextQuestion, onAnswerSelected);
                ShowQuestionUI();
            });
    }

    private void PlayStartGameSequence()
    {
        startGameSequence?.Kill();
        HideQuestionUIForIntro();

        startGameSequence = DOTween.Sequence();

        Tween playerIntro = playerMonsterUI != null ? playerMonsterUI.PlayIntro() : null;
        Tween enemyIntro = enemyMonsterUI != null ? enemyMonsterUI.PlayIntro() : null;

        if (playerIntro != null)
        {
            startGameSequence.Join(playerIntro);
        }

        if (enemyIntro != null)
        {
            startGameSequence.Join(enemyIntro);
        }

        startGameSequence.AppendCallback(ShowQuestionUI);
    }

    private void HideQuestionUIForIntro()
    {
        if (questionUI == null)
        {
            return;
        }

        questionUI.SetActive(false);
        HideNextButton();

        if (questionUICanvasGroup != null)
        {
            questionUICanvasGroup.alpha = 0f;
        }
    }

    private void ShowNextButton()
    {
        if (nextButton == null)
        {
            return;
        }

        nextButton.gameObject.SetActive(true);
        nextButton.interactable = true;
    }

    private void HideNextButton()
    {
        if (nextButton == null)
        {
            return;
        }

        nextButton.interactable = false;
        nextButton.gameObject.SetActive(false);
    }

    private void ShowQuestionUI()
    {
        if (questionUI == null)
        {
            return;
        }

        questionSequence?.Kill();
        questionUI.SetActive(true);
        Canvas.ForceUpdateCanvases();
        SaveDefaultQuestionUITransform();

        Vector3 introEndPosition = initialQuestionUIPosition;
        Vector3 introEndScale = initialQuestionUIScale;
        questionUIStartPosition = introEndPosition;
        questionUIStartScale = introEndScale;

        if (questionUIRectTransform != null)
        {
            questionUIRectTransform.position = introEndPosition + Vector3.down * questionIntroMoveOffsetY;
            questionUIRectTransform.localScale = introEndScale;
        }

        if (questionUICanvasGroup != null)
        {
            questionUICanvasGroup.alpha = 0f;
        }

        questionSequence = DOTween.Sequence();

        if (questionUICanvasGroup != null)
        {
            questionSequence.Join(questionUICanvasGroup.DOFade(1f, questionIntroDuration).SetEase(Ease.OutQuad));
        }

        if (questionUIRectTransform != null)
        {
            questionSequence.Join(questionUIRectTransform.DOMove(introEndPosition, questionIntroDuration).SetEase(Ease.OutBack));
        }
    }

    private void SaveDefaultQuestionUITransform()
    {
        if (hasSavedDefaultQuestionUITransform || questionUIRectTransform == null)
        {
            return;
        }

        initialQuestionUIPosition = questionUIRectTransform.position;
        initialQuestionUIScale = questionUIRectTransform.localScale;
        questionUIStartPosition = initialQuestionUIPosition;
        questionUIStartScale = initialQuestionUIScale;
        hasSavedDefaultQuestionUITransform = true;
    }
}
