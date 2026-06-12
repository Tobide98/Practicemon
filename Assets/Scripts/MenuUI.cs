using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WhackAMole;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private Button startButton;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float fadeInDuration = 0.35f;

    private bool isStarting;
    private Tween menuTween;

    private void Awake()
    {
        if (menuRoot == null)
        {
            menuRoot = gameObject;
        }

        if (menuCanvasGroup == null && menuRoot != null)
        {
            menuCanvasGroup = menuRoot.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
            {
                menuCanvasGroup = menuRoot.AddComponent<CanvasGroup>();
            }
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(HandleStartButtonPressed);
        }
    }

    private void OnDestroy()
    {
        menuTween?.Kill();

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(HandleStartButtonPressed);
        }
    }

    private void OnEnable()
    {
        isStarting = false;

        if (menuRoot == null)
        {
            menuRoot = gameObject;
        }

        if (menuCanvasGroup == null && menuRoot != null)
        {
            menuCanvasGroup = menuRoot.GetComponent<CanvasGroup>();
        }

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }

        if (startButton != null)
        {
            startButton.interactable = true;
        }
    }

    public void HandleStartButtonPressed()
    {
        if (isStarting)
        {
            return;
        }

        if (gameManager == null)
        {
            Debug.LogWarning($"{nameof(MenuUI)} needs a {nameof(GameManager)} reference.", this);
            return;
        }

        GameManager.Instance?.PlaySFX(SFX.SFX_PositiveClick);
        isStarting = true;
        gameManager.StartGameFromMenu();
    }

    public void FadeOut(Action onComplete)
    {
        if (startButton != null)
        {
            startButton.interactable = false;
        }

        if (menuCanvasGroup == null)
        {
            HideMenuImmediate();
            onComplete?.Invoke();
            return;
        }

        menuTween?.Kill();
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;
        menuTween = menuCanvasGroup.DOFade(0f, fadeOutDuration)
            .From(1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                HideMenuImmediate();
                onComplete?.Invoke();
            });
    }

    public void ShowMenuImmediate()
    {
        if (menuRoot == null)
        {
            menuRoot = gameObject;
        }

        if (menuRoot != null)
        {
            menuRoot.SetActive(true);
        }

        if (menuCanvasGroup == null && menuRoot != null)
        {
            menuCanvasGroup = menuRoot.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
            {
                menuCanvasGroup = menuRoot.AddComponent<CanvasGroup>();
            }
        }

        isStarting = false;

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }

        if (startButton != null)
        {
            startButton.interactable = true;
        }
    }

    public void ShowMenuFadeIn()
    {
        if (menuRoot == null)
        {
            menuRoot = gameObject;
        }

        if (menuRoot != null)
        {
            menuRoot.SetActive(true);
        }

        if (menuCanvasGroup == null && menuRoot != null)
        {
            menuCanvasGroup = menuRoot.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
            {
                menuCanvasGroup = menuRoot.AddComponent<CanvasGroup>();
            }
        }

        isStarting = false;

        if (startButton != null)
        {
            startButton.interactable = true;
        }

        if (menuCanvasGroup == null)
        {
            return;
        }

        menuTween?.Kill();
        menuCanvasGroup.alpha = 0f;
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;
        menuTween = menuCanvasGroup.DOFade(1f, fadeInDuration)
            .From(0f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                menuCanvasGroup.interactable = true;
                menuCanvasGroup.blocksRaycasts = true;
            });
    }

    public void HideMenuImmediate()
    {
        menuTween?.Kill();

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }

        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }
    }
}
