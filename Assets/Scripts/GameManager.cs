using UnityEngine;
using DG.Tweening;
using WhackAMole;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private QuestionManager questionManager;
    [SerializeField] private GameController gameController;
    [SerializeField] private MenuUI menuUI;
    [SerializeField] private GameEndUI gameEndUI;
    [SerializeField] private CanvasGroup gameplayCanvasGroup;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private bool startGameOnStart;
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private float gameplayFadeDuration = 0.35f;

    public bool HasStarted { get; private set; }

    public AudioManager AudioManager => audioManager;

    private bool audioInitialized;
    private Tween gameplayFadeTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;

        EnsureAudioManager();
    }

    private void OnDestroy()
    {
        gameplayFadeTween?.Kill();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        ShowMainMenuImmediate();
        PlayBGM(BGM.BGM_MainMenu);

        if (startGameOnStart)
        {
            StartGame();
        }
    }

    public void StartGameFromMenu()
    {
        if (HasStarted)
        {
            return;
        }

        if (menuUI == null)
        {
            StartGame();
            return;
        }

        menuUI.FadeOut(StartGame);
    }

    public void StartGame()
    {
        if (HasStarted)
        {
            return;
        }

        if (questionManager == null)
        {
            Debug.LogWarning($"{nameof(GameManager)} needs a {nameof(QuestionManager)} reference.", this);
            return;
        }

        if (gameController == null)
        {
            Debug.LogWarning($"{nameof(GameManager)} needs a {nameof(GameController)} reference.", this);
            return;
        }

        gameController.SetQuestionManager(questionManager);
        gameController.SetGameManager(this);
        gameController.ResetGameplayState();
        SetGameplayAlphaImmediate(0f);

        Question selectedQuestion = questionManager.SelectRandomQuestion();
        HasStarted = true;
        gameController.StartGame(selectedQuestion);
        FadeGameplay(1f);
    }

    public void ShowGameEnd(bool isWin)
    {
        if (gameEndUI == null)
        {
            return;
        }

        menuUI?.HideMenuImmediate();
        FadeGameplay(0f);
        gameEndUI.gameObject.SetActive(true);

        if (isWin)
        {
            gameEndUI.ShowWin();
        }
        else
        {
            gameEndUI.ShowLose();
        }
    }

    public void ReturnToMainMenu()
    {
        HasStarted = false;

        if (gameEndUI == null)
        {
            menuUI?.ShowMenuFadeIn();
            return;
        }

        gameEndUI.FadeOut(() =>
        {
            gameController?.ResetGameplayState();
            menuUI?.ShowMenuFadeIn();
        });
    }

    public void ResetToMainMenu()
    {
        HasStarted = false;
    }

    public void PlayBGM(BGM bgm)
    {
        if (!EnsureAudioManager())
        {
            return;
        }

        audioManager.PlayBGM(bgm);
    }

    public void PlaySFX(SFX sfx)
    {
        if (!EnsureAudioManager())
        {
            return;
        }

        audioManager.PlaySFX(sfx);
    }

    public void StopBGM()
    {
        if (!EnsureAudioManager())
        {
            return;
        }

        audioManager.StopBGM();
    }

    public void StopSFX()
    {
        if (!EnsureAudioManager())
        {
            return;
        }

        audioManager.StopSFX();
    }

    private void ShowMainMenuImmediate()
    {
        HasStarted = false;
        SetGameplayAlphaImmediate(0f);
        gameController?.ResetGameplayState();
        gameEndUI?.HideImmediate();
        menuUI?.ShowMenuImmediate();
    }

    private void FadeGameplay(float alpha)
    {
        if (gameplayCanvasGroup == null)
        {
            return;
        }

        gameplayFadeTween?.Kill();
        gameplayCanvasGroup.interactable = alpha > 0f;
        gameplayCanvasGroup.blocksRaycasts = alpha > 0f;
        gameplayFadeTween = gameplayCanvasGroup.DOFade(alpha, gameplayFadeDuration)
            .From(alpha > 0f ? 0f : 1f)
            .SetEase(Ease.OutQuad);
    }

    private void SetGameplayAlphaImmediate(float alpha)
    {
        if (gameplayCanvasGroup == null)
        {
            return;
        }

        gameplayFadeTween?.Kill();
        gameplayCanvasGroup.alpha = alpha;
        gameplayCanvasGroup.interactable = alpha > 0f;
        gameplayCanvasGroup.blocksRaycasts = alpha > 0f;
    }

    private bool EnsureAudioManager()
    {
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }

        if (audioManager == null)
        {
            Debug.LogWarning($"{nameof(GameManager)} needs an {nameof(AudioManager)} reference.", this);
            return false;
        }

        if (!audioInitialized)
        {
            audioManager.Init(this);
            audioInitialized = true;
        }

        return true;
    }
}
