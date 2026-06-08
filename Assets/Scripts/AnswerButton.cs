using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text answerText;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;

    private Answer currentAnswer;
    private Action<AnswerButton, Answer> onSelected;
    private Color defaultColor;

    public Button Button => button;
    public TMP_Text AnswerText => answerText;
    public Answer CurrentAnswer => currentAnswer;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (answerText == null)
        {
            answerText = GetComponentInChildren<TMP_Text>();
        }

        if (buttonImage == null && button != null)
        {
            buttonImage = button.targetGraphic as Image;
        }

        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }

        if (buttonImage != null)
        {
            defaultColor = buttonImage.color;
        }

        if (button != null)
        {
            button.onClick.AddListener(SelectAnswer);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(SelectAnswer);
        }
    }

    private void Reset()
    {
        button = GetComponent<Button>();
        answerText = GetComponentInChildren<TMP_Text>();
        buttonImage = button != null ? button.targetGraphic as Image : GetComponent<Image>();
    }

    public void SetAnswer(Answer answer, Action<AnswerButton, Answer> onAnswerSelected)
    {
        currentAnswer = answer;
        onSelected = onAnswerSelected;

        ResetVisual();

        bool hasAnswer = answer != null;
        gameObject.SetActive(hasAnswer);

        if (answerText != null)
        {
            answerText.text = hasAnswer ? answer.answerText : string.Empty;
        }

        if (button != null)
        {
            button.interactable = hasAnswer;
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    public void ShowResult(bool isCorrect)
    {
        if (buttonImage != null)
        {
            buttonImage.color = isCorrect ? correctColor : wrongColor;
        }
    }

    public void ResetVisual()
    {
        if (buttonImage != null)
        {
            buttonImage.color = defaultColor;
        }
    }

    private void SelectAnswer()
    {
        if (currentAnswer != null)
        {
            onSelected?.Invoke(this, currentAnswer);
        }
    }
}
