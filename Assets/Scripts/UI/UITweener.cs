using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITweener : MonoBehaviour
{
    public enum UIAnimationType
    {
        Move,
        Scale,
        Fade
    }

    [SerializeField] private GameObject objectToTween;
    [SerializeField] private UIAnimationType animationType;
    [SerializeField] private LeanTweenType easeType;
    [SerializeField] private float duration;
    [SerializeField] private float delay;
    [SerializeField] private Vector3 from;
    [SerializeField] private Vector3 to;
    [SerializeField] private bool startOffset;
    [SerializeField] private bool disableOnComplete;
    [SerializeField] private bool playOnStart = true;

    private LTDescr _tweenObject;

    private void Awake()
    {
        if (objectToTween == null) objectToTween = gameObject;
    }

    private void OnEnable()
    {
        if (playOnStart) HandleTween();
    }

    public void HandleTween(bool instant = false)
    {
        if (!objectToTween.activeSelf)
            objectToTween.SetActive(true);

        switch (animationType)
        {
            case UIAnimationType.Scale:
                Scale(instant);
                break;

            case UIAnimationType.Fade:
                Fade(instant);
                break;

            case UIAnimationType.Move:
                MoveAbsolute(instant);
                break;
        }

        if (instant) return;
        _tweenObject.setDelay(delay);
        _tweenObject.setEase(easeType);
        _tweenObject.setOnComplete(() =>
        {
            if (disableOnComplete) objectToTween.SetActive(false);
        });
    }

    private void MoveAbsolute(bool instant)
    {
        RectTransform rectTransform = objectToTween.GetComponent<RectTransform>();
        if (instant)
        {
            rectTransform.anchoredPosition = to;
            return;
        }
        rectTransform.anchoredPosition = from;

        _tweenObject = LeanTween.move(rectTransform, to, duration);
    }

    private void Scale(bool instant)
    {
        RectTransform rectTransform = objectToTween.GetComponent<RectTransform>();
        if (instant)
        {
            rectTransform.localScale = to;
            return;
        }
        if (startOffset) rectTransform.localScale = from;

        _tweenObject = LeanTween.scale(objectToTween, to, duration);
    }

    private void Fade(bool instant)
    {
        if (!objectToTween.TryGetComponent<CanvasGroup>(out CanvasGroup canvasGroup))
            canvasGroup = objectToTween.AddComponent<CanvasGroup>();

        if (instant)
        {
            canvasGroup.alpha = to.x;
            return;
        }

        if (startOffset) canvasGroup.alpha = from.x;

        _tweenObject = LeanTween.alphaCanvas(canvasGroup, to.x, duration);
    }
}
