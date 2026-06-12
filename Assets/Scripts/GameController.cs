using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameUI gameUI;
    [SerializeField] private QuestionManager questionManager;
    [SerializeField] private MonsterController playerMonster;
    [SerializeField] private MonsterController enemyMonster;

    private GameManager gameManager;
    private bool isGameEnded;

    private void OnEnable()
    {
        SubscribeMonsterEvents();
    }

    private void OnDisable()
    {
        UnsubscribeMonsterEvents();
    }

    public void SetQuestionManager(QuestionManager newQuestionManager)
    {
        questionManager = newQuestionManager;
    }

    public void SetGameManager(GameManager newGameManager)
    {
        gameManager = newGameManager;
    }

    public void StartGame(Question firstQuestion)
    {
        if (firstQuestion == null)
        {
            Debug.LogWarning($"{nameof(GameController)} cannot start a null question.", this);
            return;
        }

        isGameEnded = false;
        ResetMonsters();

        if (gameUI != null)
        {
            gameUI.StartGame(firstQuestion, HandleAnswerSelected, SelectNextQuestion);
        }
    }

    public void ResetGameplayState()
    {
        isGameEnded = false;
        ResetMonsters();

        if (gameUI != null)
        {
            gameUI.ResetGameplayUI();
        }
    }

    public void StartQuestion(Question question)
    {
        if (question == null)
        {
            Debug.LogWarning($"{nameof(GameController)} cannot show a null question.", this);
            return;
        }

        if (gameUI != null)
        {
            gameUI.ShowQuestion(question, HandleAnswerSelected, SelectNextQuestion);
        }
    }

    private void HandleAnswerSelected(Answer answer)
    {
        if (isGameEnded)
        {
            return;
        }

        bool isCorrect = answer != null && answer.isCorrect;
        ApplyAnswerResult(isCorrect);
    }

    public void ApplyAnswerResult(bool isCorrect)
    {
        if (isCorrect)
        {
            if (playerMonster != null)
            {
                playerMonster.Attack(enemyMonster);
            }

            return;
        }

        if (enemyMonster != null)
        {
            enemyMonster.Attack(playerMonster);
            return;
        }

        if (playerMonster != null)
        {
            playerMonster.TakeDamage(1);
        }
    }

    private Question SelectNextQuestion()
    {
        if (isGameEnded)
        {
            return null;
        }

        if (questionManager == null)
        {
            Debug.LogWarning($"{nameof(GameController)} needs a {nameof(QuestionManager)} reference to select the next question.", this);
            return null;
        }

        return questionManager.SelectRandomQuestion();
    }

    private void SubscribeMonsterEvents()
    {
        if (playerMonster != null)
        {
            playerMonster.Defeated += HandleMonsterDefeated;
        }

        if (enemyMonster != null)
        {
            enemyMonster.Defeated += HandleMonsterDefeated;
        }
    }

    private void UnsubscribeMonsterEvents()
    {
        if (playerMonster != null)
        {
            playerMonster.Defeated -= HandleMonsterDefeated;
        }

        if (enemyMonster != null)
        {
            enemyMonster.Defeated -= HandleMonsterDefeated;
        }
    }

    private void HandleMonsterDefeated(MonsterController defeatedMonster)
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        if (defeatedMonster == enemyMonster)
        {
            gameManager?.ShowGameEnd(true);
        }
        else if (defeatedMonster == playerMonster)
        {
            gameManager?.ShowGameEnd(false);
        }
    }

    private void ResetMonsters()
    {
        if (playerMonster != null)
        {
            playerMonster.ResetMonsterState();
        }

        if (enemyMonster != null)
        {
            enemyMonster.ResetMonsterState();
        }
    }
}
