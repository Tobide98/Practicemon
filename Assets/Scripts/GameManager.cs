using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private QuestionManager questionManager;
    [SerializeField] private GameController gameController;
    [SerializeField] private MenuUI menuUI;
    [SerializeField] private GameEndUI gameEndUI;
    [SerializeField] private bool startGameOnStart;
    [SerializeField] private int targetFrameRate = 60;

    public bool HasStarted { get; private set; }

    private void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;
    }

    private void Start()
    {
        ShowMainMenuImmediate();

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

        Question selectedQuestion = questionManager.SelectRandomQuestion();
        HasStarted = true;
        gameController.StartGame(selectedQuestion);
    }

    public void ShowGameEnd(bool isWin)
    {
        if (gameEndUI == null)
        {
            return;
        }

        menuUI?.HideMenuImmediate();
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

    private void ShowMainMenuImmediate()
    {
        HasStarted = false;
        gameController?.ResetGameplayState();
        gameEndUI?.HideImmediate();
        menuUI?.ShowMenuImmediate();
    }
}
