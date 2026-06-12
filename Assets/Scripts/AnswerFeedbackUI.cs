using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhackAMole;

public class AnswerFeedbackUI : MonoBehaviour
{
    [SerializeField] private GameObject feedbackRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite correctSprite;
    [SerializeField] private Sprite wrongSprite;
    [SerializeField] private string correctMessage = "Correct!";
    [SerializeField] private string wrongMessage = "Wrong!";
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float visibleDuration = 1.5f;
    [SerializeField] private float punchScale = 0.12f;
    [SerializeField] private float punchDuration = 0.25f;

    private RectTransform feedbackRectTransform;
    private Vector3 startScale;
    private Sequence feedbackSequence;
    private bool isActivatingForPlay;

    private void Awake()
    {
        if (feedbackRoot == null)
        {
            feedbackRoot = gameObject;
        }

        feedbackRectTransform = feedbackRoot.GetComponent<RectTransform>();
        if (feedbackRectTransform != null)
        {
            startScale = feedbackRectTransform.localScale;
        }

        if (canvasGroup == null)
        {
            canvasGroup = feedbackRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = feedbackRoot.AddComponent<CanvasGroup>();
            }
        }

        HideImmediate();
    }

    private void OnDestroy()
    {
        feedbackSequence?.Kill();
    }

    private void OnEnable()
    {
        if (!isActivatingForPlay)
        {
            HideImmediate();
        }
    }

    public void Play(bool isCorrect, Action onComplete)
    {
        if (feedbackRoot == null)
        {
            onComplete?.Invoke();
            return;
        }

        feedbackSequence?.Kill();
        isActivatingForPlay = true;
        feedbackRoot.SetActive(true);
        isActivatingForPlay = false;

        if (feedbackText != null)
        {
            feedbackText.text = isCorrect ? correctMessage : wrongMessage;
            feedbackText.color = isCorrect ? correctColor : wrongColor;
        }

        SetResultImage(isCorrect);
        GameManager.Instance?.PlaySFX(isCorrect ? SFX.SFX_Correct : SFX.SFX_Wrong);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (feedbackRectTransform != null)
        {
            feedbackRectTransform.localScale = startScale;
        }

        feedbackSequence = DOTween.Sequence();

        if (canvasGroup != null)
        {
            feedbackSequence.Append(canvasGroup.DOFade(1f, fadeDuration).From(0f).SetEase(Ease.OutQuad));
        }

        if (feedbackRectTransform != null)
        {
            feedbackSequence.Join(feedbackRectTransform.DOPunchScale(Vector3.one * punchScale, punchDuration, 6, 0.6f));
        }

        feedbackSequence
            .AppendInterval(visibleDuration)
            .Append(canvasGroup != null ? canvasGroup.DOFade(0f, fadeDuration).From(1f).SetEase(Ease.InQuad) : DOVirtual.DelayedCall(fadeDuration, () => { }))
            .OnComplete(() =>
            {
                HideImmediate();
                onComplete?.Invoke();
            });
    }

    public void HideImmediate()
    {
        feedbackSequence?.Kill();
        HideResultImages();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (feedbackRectTransform != null)
        {
            feedbackRectTransform.localScale = startScale;
        }

        if (feedbackRoot != null)
        {
            feedbackRoot.SetActive(false);
        }
    }

    private void HideResultImages()
    {
        if (resultImage != null)
        {
            resultImage.sprite = null;
            resultImage.gameObject.SetActive(false);
        }
    }

    private void SetResultImage(bool isCorrect)
    {
        if (resultImage == null)
        {
            return;
        }

        resultImage.sprite = isCorrect ? correctSprite : wrongSprite;
        resultImage.gameObject.SetActive(resultImage.sprite != null);
    }
}
