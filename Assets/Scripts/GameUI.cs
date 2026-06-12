using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhackAMole;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameObject questionUI;
    [SerializeField] private Image questionImage;
    [SerializeField] private Button questionImageButton;
    [SerializeField] private GameObject imagePreviewRoot;
    [SerializeField] private CanvasGroup imagePreviewCanvasGroup;
    [SerializeField] private Image imagePreviewImage;
    [SerializeField] private Button imagePreviewCloseButton;
    [SerializeField] private float imagePreviewFadeDuration = 0.2f;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text answerBodyText;
    [SerializeField] private AnswerButton[] answerButtons;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonText;
    [SerializeField] private Image nextButtonImage;
    [SerializeField] private string okButtonText = "OK";
    [SerializeField] private string nextButtonTextValue = "NEXT";
    [SerializeField] private Color okButtonColor = Color.green;
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
    private Tween imagePreviewTween;
    private Action<Answer> onAnswerSelected;
    private Func<Question> nextQuestionProvider;
    private AnswerButton pendingAnswerButton;
    private Answer pendingAnswer;
    private Color nextButtonDefaultColor = Color.white;
    private NextButtonMode nextButtonMode;
    private bool isAnswerLocked;

    private enum NextButtonMode
    {
        None,
        ConfirmAnswer,
        NextQuestion
    }

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

        SetupImagePreview();

        if (nextButton != null)
        {
            if (nextButtonText == null)
            {
                nextButtonText = nextButton.GetComponentInChildren<TMP_Text>();
            }

            if (nextButtonImage == null)
            {
                nextButtonImage = nextButton.targetGraphic as Image;
            }

            if (nextButtonImage == null)
            {
                nextButtonImage = nextButton.GetComponent<Image>();
            }

            if (nextButtonImage != null)
            {
                nextButtonDefaultColor = nextButtonImage.color;
            }

            nextButton.onClick.AddListener(HandleNextButtonPressed);
            HideNextButton();
        }
    }

    private void OnDestroy()
    {
        startGameSequence?.Kill();
        questionSequence?.Kill();
        imagePreviewTween?.Kill();

        if (questionImageButton != null)
        {
            questionImageButton.onClick.RemoveListener(ShowImagePreview);
        }

        if (imagePreviewCloseButton != null)
        {
            imagePreviewCloseButton.onClick.RemoveListener(HideImagePreview);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(HandleNextButtonPressed);
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
        isAnswerLocked = false;
        pendingAnswerButton = null;
        pendingAnswer = null;
        HideNextButton();
        HideImagePreviewImmediate();

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

        isAnswerLocked = false;
        pendingAnswerButton = null;
        pendingAnswer = null;
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

        if (questionImageButton != null)
        {
            questionImageButton.interactable = question.questionImage != null;
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
        if (isAnswerLocked)
        {
            return;
        }

        pendingAnswerButton = selectedButton;
        pendingAnswer = selectedAnswer;
        ResetAnswerButtonVisuals();

        if (pendingAnswerButton != null)
        {
            pendingAnswerButton.ShowSelected();
        }

        ShowOkButton();
    }

    private void ProcessConfirmedAnswer()
    {
        if (pendingAnswer == null || isAnswerLocked)
        {
            return;
        }

        isAnswerLocked = true;
        bool isCorrect = pendingAnswer != null && pendingAnswer.isCorrect;

        SetAnswerButtonsInteractable(false);

        if (answerButtons != null)
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (answerButtons[i] != null && !isCorrect && answerButtons[i].CurrentAnswer != null && answerButtons[i].CurrentAnswer.isCorrect)
                {
                    answerButtons[i].ShowResult(true);
                }
            }
        }

        if (pendingAnswerButton != null)
        {
            pendingAnswerButton.ShowResult(isCorrect);
        }

        HideNextButton();

        Answer confirmedAnswer = pendingAnswer;
        pendingAnswerButton = null;
        pendingAnswer = null;

        if (answerFeedbackUI != null)
        {
            answerFeedbackUI.Play(isCorrect, () =>
            {
                onAnswerSelected?.Invoke(confirmedAnswer);
                ShowNextButton();
            });

            return;
        }

        onAnswerSelected?.Invoke(confirmedAnswer);
        ShowNextButton();
    }

    private void ResetAnswerButtonVisuals()
    {
        if (answerButtons == null)
        {
            return;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i]?.ResetVisual();
        }
    }

    private void SetAnswerButtonsInteractable(bool interactable)
    {
        if (answerButtons == null)
        {
            return;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i]?.SetInteractable(interactable);
        }
    }

    private void HandleNextButtonPressed()
    {
        GameManager.Instance?.PlaySFX(SFX.SFX_PositiveClick);

        if (nextButtonMode == NextButtonMode.ConfirmAnswer)
        {
            ProcessConfirmedAnswer();
            return;
        }

        if (nextButtonMode == NextButtonMode.NextQuestion)
        {
            ShowNextQuestion();
        }
    }

    private void ShowNextQuestion()
    {
        if (nextButton != null)
        {
            nextButton.interactable = false;
            nextButton.gameObject.SetActive(false);
        }

        nextButtonMode = NextButtonMode.None;

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
            .Append(questionUICanvasGroup.DOFade(0f, questionIntroDuration).From(1f).SetEase(Ease.InQuad))
            .AppendCallback(() =>
            {
                SetQuestion(nextQuestion, onAnswerSelected);
                ShowQuestionUI();
            });
    }

    private void ShowOkButton()
    {
        if (nextButton == null)
        {
            return;
        }

        nextButtonMode = NextButtonMode.ConfirmAnswer;

        if (nextButtonText != null)
        {
            nextButtonText.text = okButtonText;
        }

        if (nextButtonImage != null)
        {
            nextButtonImage.color = okButtonColor;
        }

        nextButton.gameObject.SetActive(true);
        nextButton.interactable = true;
    }

    private void ShowNextButton()
    {
        if (nextButton == null)
        {
            return;
        }

        nextButtonMode = NextButtonMode.NextQuestion;

        if (nextButtonText != null)
        {
            nextButtonText.text = nextButtonTextValue;
        }

        if (nextButtonImage != null)
        {
            nextButtonImage.color = nextButtonDefaultColor;
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

        nextButtonMode = NextButtonMode.None;
        nextButton.interactable = false;
        nextButton.gameObject.SetActive(false);
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

    private void ShowQuestionUI()
    {
        if (questionUI == null)
        {
            return;
        }

        questionSequence?.Kill();
        questionUI.SetActive(true);
        GameManager.Instance?.PlaySFX(SFX.SFX_Show);
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
            questionSequence.Join(questionUICanvasGroup.DOFade(1f, questionIntroDuration).From(0f).SetEase(Ease.OutQuad));
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

    private void SetupImagePreview()
    {
        if (questionImageButton != null)
        {
            questionImageButton.onClick.AddListener(ShowImagePreview);
            questionImageButton.interactable = questionImage != null && questionImage.sprite != null;
        }

        if (imagePreviewRoot != null)
        {
            if (imagePreviewCanvasGroup == null)
            {
                imagePreviewCanvasGroup = imagePreviewRoot.GetComponent<CanvasGroup>();
                if (imagePreviewCanvasGroup == null)
                {
                    imagePreviewCanvasGroup = imagePreviewRoot.AddComponent<CanvasGroup>();
                }
            }

            if (imagePreviewImage == null)
            {
                Image[] previewImages = imagePreviewRoot.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < previewImages.Length; i++)
                {
                    if (previewImages[i].gameObject != imagePreviewRoot)
                    {
                        imagePreviewImage = previewImages[i];
                        break;
                    }
                }
            }
        }

        if (imagePreviewCloseButton != null)
        {
            imagePreviewCloseButton.onClick.AddListener(HideImagePreview);
        }

        HideImagePreviewImmediate();
    }

    private void ShowImagePreview()
    {
        if (imagePreviewRoot == null || imagePreviewImage == null || questionImage == null || questionImage.sprite == null)
        {
            return;
        }

        GameManager.Instance?.PlaySFX(SFX.SFX_PositiveClick);
        imagePreviewTween?.Kill();

        imagePreviewImage.sprite = questionImage.sprite;
        imagePreviewImage.preserveAspect = questionImage.preserveAspect;
        imagePreviewImage.enabled = true;
        imagePreviewRoot.SetActive(true);

        if (imagePreviewCanvasGroup != null)
        {
            imagePreviewCanvasGroup.alpha = 0f;
            imagePreviewCanvasGroup.interactable = true;
            imagePreviewCanvasGroup.blocksRaycasts = true;
            imagePreviewTween = imagePreviewCanvasGroup.DOFade(1f, imagePreviewFadeDuration).SetEase(Ease.OutQuad);
        }
    }

    private void HideImagePreview()
    {
        if (imagePreviewRoot == null)
        {
            return;
        }

        GameManager.Instance?.PlaySFX(SFX.SFX_PositiveClick);
        imagePreviewTween?.Kill();

        if (imagePreviewCanvasGroup == null)
        {
            HideImagePreviewImmediate();
            return;
        }

        imagePreviewCanvasGroup.interactable = false;
        imagePreviewCanvasGroup.blocksRaycasts = false;
        imagePreviewTween = imagePreviewCanvasGroup.DOFade(0f, imagePreviewFadeDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(HideImagePreviewImmediate);
    }

    private void HideImagePreviewImmediate()
    {
        imagePreviewTween?.Kill();

        if (imagePreviewCanvasGroup != null)
        {
            imagePreviewCanvasGroup.alpha = 0f;
            imagePreviewCanvasGroup.interactable = false;
            imagePreviewCanvasGroup.blocksRaycasts = false;
        }

        if (imagePreviewImage != null)
        {
            imagePreviewImage.sprite = null;
            imagePreviewImage.enabled = false;
        }

        if (imagePreviewRoot != null)
        {
            imagePreviewRoot.SetActive(false);
        }
    }
}
