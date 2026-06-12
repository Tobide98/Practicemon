using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image monsterImage;
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Color healthyHpColor = Color.green;
    [SerializeField] private Color halfHpColor = Color.yellow;
    [SerializeField] private Color lowHpColor = Color.red;
    [SerializeField] private Transform animatedTarget;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool hideUntilIntro = true;
    [SerializeField] private float introMoveOffsetY = 60f;
    [SerializeField] private float introDuration = 0.35f;
    [SerializeField] private float idleMoveOffsetY = 10f;
    [SerializeField] private float idleMoveDuration = 0.35f;
    [SerializeField] private float minIdleLoopDelay = 1f;
    [SerializeField] private float maxIdleLoopDelay = 1.5f;
    [SerializeField] private float idleSquashY = 0.95f;
    [SerializeField] private float idleSquashDuration = 0.08f;
    [SerializeField] private float attackMoveOffsetX = 0.2f;
    [SerializeField] private float attackMoveDuration = 0.12f;
    [SerializeField] private float hitBlinkDuration = 0.5f;
    [SerializeField] private float hitPunchScale = 0.18f;
    [SerializeField] private float hpFillTweenDuration = 0.35f;

    private Sequence idleLoop;
    private Sequence attackSequence;
    private Sequence hitFeedback;
    private Tween hpFillTween;
    private Vector3 startPosition;
    private Vector3 startScale;
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private bool hasSavedDefaultTransform;
    private Vector3 hpFillStartScale;
    private RectTransform hpFillRectTransform;
    private RectTransform monsterImageRectTransform;
    private Vector3 monsterImageStartScale;

    private void Awake()
    {
        if (animatedTarget == null)
        {
            animatedTarget = transform;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (hpFillImage != null)
        {
            hpFillRectTransform = hpFillImage.rectTransform;
            hpFillStartScale = hpFillRectTransform.localScale;
        }

        if (monsterImage != null)
        {
            monsterImageRectTransform = monsterImage.rectTransform;
            monsterImageStartScale = monsterImageRectTransform.localScale;
        }

        startPosition = animatedTarget.position;
        startScale = animatedTarget.localScale;

        if (hideUntilIntro && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void OnEnable()
    {
        if (!hideUntilIntro)
        {
            PlayIdleLoop();
        }
    }

    private void OnDisable()
    {
        StopIdleLoop();
        attackSequence?.Kill();
        hpFillTween?.Kill();
    }

    public void SetMonster(string monsterName, int currentHp, int maxHp)
    {
        if (nameText != null)
        {
            nameText.text = monsterName;
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHp}/{maxHp}";
        }

        if (hpFillImage != null)
        {
            float hpPercent = maxHp > 0 ? (float)currentHp / maxHp : 0f;
            hpFillTween?.Kill();
            hpFillTween = hpFillImage.DOFillAmount(hpPercent, hpFillTweenDuration).SetEase(Ease.OutQuad);
            hpFillImage.color = GetHpColor(hpPercent);
        }
    }

    public void SetMonsterImmediate(string monsterName, int currentHp, int maxHp)
    {
        hpFillTween?.Kill();

        if (nameText != null)
        {
            nameText.text = monsterName;
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHp}/{maxHp}";
        }

        if (hpFillImage != null)
        {
            float hpPercent = maxHp > 0 ? (float)currentHp / maxHp : 0f;
            hpFillImage.fillAmount = hpPercent;
            hpFillImage.color = GetHpColor(hpPercent);
        }
    }

    public void PlayHitFeedback()
    {
        if (hpFillImage == null && monsterImage == null)
        {
            return;
        }

        if (hpFillRectTransform == null)
        {
            hpFillRectTransform = hpFillImage != null ? hpFillImage.rectTransform : null;
        }

        if (monsterImageRectTransform == null)
        {
            monsterImageRectTransform = monsterImage != null ? monsterImage.rectTransform : null;
        }

        hitFeedback?.Kill();
        Color currentHpColor = hpFillImage != null ? hpFillImage.color : Color.white;
        Color currentMonsterColor = monsterImage != null ? monsterImage.color : Color.white;

        if (hpFillImage != null)
        {
            hpFillImage.enabled = true;
        }

        if (monsterImage != null)
        {
            monsterImage.enabled = true;
        }

        hitFeedback = DOTween.Sequence();

        if (hpFillRectTransform != null)
        {
            hitFeedback.Join(hpFillRectTransform.DOPunchScale(Vector3.one * hitPunchScale, 0.18f, 8, 0.6f));
        }

        if (monsterImageRectTransform != null)
        {
            hitFeedback.Join(monsterImageRectTransform.DOPunchScale(Vector3.one * hitPunchScale, 0.18f, 8, 0.6f));
        }

        hitFeedback
            .Append(CreateBlinkTween(0f, 0.08f))
            .Append(CreateBlinkTween(1f, 0.08f))
            .Append(CreateBlinkTween(0f, 0.08f))
            .Append(CreateBlinkTween(1f, Mathf.Max(0f, hitBlinkDuration - 0.24f)))
            .OnComplete(() =>
            {
                RestoreHitFeedback(currentHpColor, currentMonsterColor);
            });
    }

    private Tween CreateBlinkTween(float alpha, float duration)
    {
        Sequence blinkSequence = DOTween.Sequence();

        if (hpFillImage != null)
        {
            blinkSequence.Join(hpFillImage.DOFade(alpha, duration));
        }

        if (monsterImage != null)
        {
            blinkSequence.Join(monsterImage.DOFade(alpha, duration));
        }

        return blinkSequence;
    }

    private void RestoreHitFeedback(Color hpColor, Color monsterColor)
    {
        if (hpFillImage != null)
        {
            hpFillImage.enabled = true;
            hpColor.a = 1f;
            hpFillImage.color = hpColor;
        }

        if (hpFillRectTransform != null)
        {
            hpFillRectTransform.localScale = hpFillStartScale;
        }

        if (monsterImage != null)
        {
            monsterImage.enabled = true;
            monsterColor.a = 1f;
            monsterImage.color = monsterColor;
        }

        if (monsterImageRectTransform != null)
        {
            monsterImageRectTransform.localScale = monsterImageStartScale;
        }
    }

    public Tween PlayIntro()
    {
        if (animatedTarget == null)
        {
            return null;
        }

        SaveDefaultTransform();
        StopIdleLoop(false);

        startPosition = initialPosition;
        startScale = initialScale;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        animatedTarget.position = initialPosition + Vector3.up * introMoveOffsetY;
        animatedTarget.localScale = initialScale;

        Sequence introSequence = DOTween.Sequence();

        if (canvasGroup != null)
        {
            introSequence.Join(canvasGroup.DOFade(1f, introDuration).From(0f).SetEase(Ease.OutQuad));
        }

        introSequence
            .Join(animatedTarget.DOMove(initialPosition, introDuration).SetEase(Ease.OutQuad))
            .OnComplete(PlayIdleLoop);

        return introSequence;
    }

    public Tween PlayAttack(MonsterUI target, Action onComplete)
    {
        if (animatedTarget == null)
        {
            onComplete?.Invoke();
            return null;
        }

        StopIdleLoop(true);
        attackSequence?.Kill();

        float direction = 1f;
        if (target != null)
        {
            direction = Mathf.Sign(target.GetAnimatedPosition().x - startPosition.x);
            if (Mathf.Approximately(direction, 0f))
            {
                direction = 1f;
            }
        }

        Vector3 attackPosition = startPosition + Vector3.right * direction * attackMoveOffsetX;
        attackSequence = DOTween.Sequence()
            .Append(animatedTarget.DOMove(attackPosition, attackMoveDuration).SetEase(Ease.OutQuad))
            .Append(animatedTarget.DOMove(startPosition, attackMoveDuration).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                attackSequence = null;
                onComplete?.Invoke();
                PlayIdleLoop();
            });

        return attackSequence;
    }

    public void ResetForNewGame()
    {
        SaveDefaultTransform();
        StopIdleLoop(false);
        startPosition = initialPosition;
        startScale = initialScale;

        if (canvasGroup != null && hideUntilIntro)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private Color GetHpColor(float hpPercent)
    {
        if (hpPercent <= 0.4f)
        {
            return lowHpColor;
        }

        if (hpPercent <= 0.5f)
        {
            return halfHpColor;
        }

        return healthyHpColor;
    }

    public void PlayIdleLoop()
    {
        if (animatedTarget == null)
        {
            return;
        }

        StopIdleLoop();

        animatedTarget.position = startPosition;
        animatedTarget.localScale = startScale;

        idleLoop = DOTween.Sequence()
            .Append(animatedTarget.DOMove(startPosition + Vector3.up * idleMoveOffsetY, idleMoveDuration).SetEase(Ease.OutQuad))
            .Append(animatedTarget.DOMove(startPosition, idleMoveDuration).SetEase(Ease.InQuad))
            .Append(animatedTarget.DOScale(new Vector3(startScale.x, startScale.y * idleSquashY, startScale.z), idleSquashDuration).SetEase(Ease.OutQuad))
            .Append(animatedTarget.DOScale(startScale, idleSquashDuration).SetEase(Ease.OutQuad))
            .AppendInterval(GetRandomIdleLoopDelay())
            .OnComplete(() =>
            {
                idleLoop = null;
                PlayIdleLoop();
            });
    }

    private float GetRandomIdleLoopDelay()
    {
        float minDelay = Mathf.Min(minIdleLoopDelay, maxIdleLoopDelay);
        float maxDelay = Mathf.Max(minIdleLoopDelay, maxIdleLoopDelay);
        return UnityEngine.Random.Range(minDelay, maxDelay);
    }

    public void StopIdleLoop(bool resetTransform = true)
    {
        if (idleLoop != null)
        {
            idleLoop.Kill();
            idleLoop = null;
        }

        if (hitFeedback != null)
        {
            hitFeedback.Kill();
            hitFeedback = null;
        }

        if (hpFillImage != null)
        {
            Color hpColor = hpFillImage.color;
            hpColor.a = 1f;
            hpFillImage.color = hpColor;
        }

        if (hpFillRectTransform != null)
        {
            hpFillRectTransform.localScale = hpFillStartScale;
        }

        if (monsterImage != null)
        {
            Color monsterColor = monsterImage.color;
            monsterColor.a = 1f;
            monsterImage.color = monsterColor;
        }

        if (monsterImageRectTransform != null)
        {
            monsterImageRectTransform.localScale = monsterImageStartScale;
        }

        if (resetTransform && animatedTarget != null)
        {
            animatedTarget.position = startPosition;
            animatedTarget.localScale = startScale;
        }
    }

    public Vector3 GetAnimatedPosition()
    {
        return animatedTarget != null ? animatedTarget.position : transform.position;
    }

    private void SaveDefaultTransform()
    {
        if (hasSavedDefaultTransform || animatedTarget == null)
        {
            return;
        }

        initialPosition = animatedTarget.position;
        initialScale = animatedTarget.localScale;
        startPosition = initialPosition;
        startScale = initialScale;
        hasSavedDefaultTransform = true;
    }
}
