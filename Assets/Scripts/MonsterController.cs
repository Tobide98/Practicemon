using System;
using UnityEngine;
using DG.Tweening;

public class MonsterController : MonoBehaviour
{
    [SerializeField] private string monsterName = "Monster";
    [SerializeField] private int maxHp = 10;
    [SerializeField] private int currentHp = 10;
    [SerializeField] private int baseAttack = 1;
    [SerializeField] private MonsterUI monsterUI;
    [SerializeField] private ParticleSystem hitParticle;

    public string MonsterName => monsterName;
    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;
    public int BaseAttack => baseAttack;
    public bool IsDefeated => currentHp <= 0;

    public event Action<MonsterController> Defeated;

    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        RefreshUI();
    }

    private void Reset()
    {
        monsterUI = GetComponentInChildren<MonsterUI>();
    }

    public void Initialize(string newMonsterName, int newMaxHp)
    {
        monsterName = newMonsterName;
        maxHp = Mathf.Max(1, newMaxHp);
        currentHp = maxHp;
        RefreshUI();
    }

    public void ResetMonsterState()
    {
        currentHp = maxHp;

        if (monsterUI != null)
        {
            monsterUI.ResetForNewGame();
            monsterUI.SetMonsterImmediate(monsterName, currentHp, maxHp);
        }

        if (hitParticle != null)
        {
            hitParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void Attack(MonsterController target)
    {
        if (target == null || IsDefeated)
        {
            return;
        }

        target.TakeDamage(baseAttack);
    }

    public void TakeDamage(int damage)
    {
        int appliedDamage = Mathf.Max(0, damage);
        if (appliedDamage <= 0 || IsDefeated)
        {
            return;
        }

        currentHp = Mathf.Clamp(currentHp - appliedDamage, 0, maxHp);
        RefreshUI();

        if (monsterUI != null)
        {
            monsterUI.PlayHitFeedback();
        }

        PlayHitParticle();

        if (IsDefeated)
        {
            Defeated?.Invoke(this);
        }
    }

    public void RefreshUI()
    {
        if (monsterUI != null)
        {
            monsterUI.SetMonster(monsterName, currentHp, maxHp);
        }
    }

    public Tween PlayIntro()
    {
        return monsterUI != null ? monsterUI.PlayIntro() : null;
    }

    private void PlayHitParticle()
    {
        if (hitParticle == null)
        {
            return;
        }

        hitParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        hitParticle.Play(true);
    }
}
