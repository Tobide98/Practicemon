using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WhackAMole;

public class GameEndUI : MonoBehaviour
{
    [SerializeField] private GameObject endRoot;
    [SerializeField] private GameObject winTextObject;
    [SerializeField] private GameObject loseTextObject;
    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite winSprite;
    [SerializeField] private Sprite loseSprite;
    [SerializeField] private Button returnButton;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private bool hideOnAwake = true;

    private Tween endTween;

    private void Awake()
    {
        if (endRoot == null)
        {
            endRoot = gameObject;
        }

        if (canvasGroup == null && endRoot != null)
        {
            canvasGroup = endRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = endRoot.AddComponent<CanvasGroup>();
            }
        }

        if (returnButton != null)
        {
            returnButton.onClick.AddListener(HandleReturnButtonPressed);
        }

        if (hideOnAwake)
        {
            Hide();
        }
    }

    private void OnDestroy()
    {
        endTween?.Kill();

        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(HandleReturnButtonPressed);
        }
    }

    public void ShowWin()
    {
        Show(true);
    }

    public void ShowLose()
    {
        Show(false);
    }

    public void Hide()
    {
        HideImmediate();
    }

    public void HideImmediate()
    {
        if (endRoot != null)
        {
            endRoot.SetActive(false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (winTextObject != null)
        {
            winTextObject.SetActive(false);
        }

        if (loseTextObject != null)
        {
            loseTextObject.SetActive(false);
        }
    }

    private void Show(bool isWin)
    {
        if (endRoot != null)
        {
            endRoot.SetActive(true);
        }

        if (canvasGroup != null)
        {
            endTween?.Kill();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (winTextObject != null)
        {
            winTextObject.SetActive(isWin);
        }

        if (loseTextObject != null)
        {
            loseTextObject.SetActive(!isWin);
        }

        if (returnButton != null)
        {
            returnButton.interactable = true;
        }

        if (resultImage != null)
        {
            resultImage.sprite = isWin ? winSprite : loseSprite;
            resultImage.enabled = resultImage.sprite != null;
        }

        GameManager.Instance?.PlaySFX(isWin ? SFX.SFX_Win : SFX.SFX_Lose);

        if (canvasGroup != null)
        {
            endTween = canvasGroup.DOFade(1f, fadeDuration)
                .From(0f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                });
        }
    }

    public void FadeOut(Action onComplete)
    {
        if (returnButton != null)
        {
            returnButton.interactable = false;
        }

        if (canvasGroup == null)
        {
            HideImmediate();
            onComplete?.Invoke();
            return;
        }

        endTween?.Kill();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        endTween = canvasGroup.DOFade(0f, fadeDuration)
            .From(1f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                HideImmediate();
                onComplete?.Invoke();
            });
    }

    private void HandleReturnButtonPressed()
    {
        GameManager.Instance?.PlaySFX(SFX.SFX_PositiveClick);
        gameManager?.ReturnToMainMenu();
    }
}
