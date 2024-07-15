using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class CommonUI : MonoBehaviour
{
    public TextMeshProUGUI textToast;
    public RectTransform toast;

    public CurrencyUI _CurrencyUI;
    [SerializeField] private CanvasGroup canvasGroupLoading;
    private Coroutine coroutine;

    // Start is called before the first frame update
    /// <summary>
    /// Initialize all UI.
    /// </summary>
    void UIReset()
    {
        toast.DOAnchorPosY(200f, 0f);
        canvasGroupLoading.DOFade(0f, 0f);
    }
    /// <summary>
    /// Enable toast popup
    /// </summary>
    public void SetToast(string info, float time = 1.5f)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            toast.DOKill();
            toast.DOAnchorPosY(200f, 0f);
        }

        textToast.text = info;
        toast.DOAnchorPosY(0f, 0.2f).SetEase(Ease.OutBack);
        coroutine = StartCoroutine(SetToastCo());
    }

    IEnumerator SetToastCo()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        toast.DOAnchorPosY(200f, 0.25f).SetEase(Ease.OutCubic);
    }

    #if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SetToast("BREAD", 2f);
        }
    }
#endif
}
