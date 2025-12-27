using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Text hintText;
    [SerializeField] private Text arrowText;

    private ObstaclePosition currentPos;
    private bool isAnimating = false;
    private Vector2 hiddenPos;
    private Vector2 showPos = Vector2.zero; // アンカー中心

    // 画面外の座標設定（Canvasサイズに合わせて調整してください）
    private float offsetDistance = 1000f; 

    public ObstaclePosition CurrentPosition => currentPos;
    public bool IsActive { get; private set; } = false;

    public void Spawn(ObstaclePosition pos)
    {
        currentPos = pos;
        IsActive = true;
        gameObject.SetActive(true);

        SetupPositionAndText(pos);
        StartCoroutine(SlideIn());
    }

    public void Hide(bool success)
    {
        if (!IsActive) return;
        StartCoroutine(SlideOut(success));
    }

    private void SetupPositionAndText(ObstaclePosition pos)
    {
        // 位置とテキストの初期化
        switch (pos)
        {
            case ObstaclePosition.Top:
                hiddenPos = new Vector2(0, offsetDistance);
                arrowText.text = "⬆️";
                hintText.text = "SWIPE UP!";
                break;
            case ObstaclePosition.Bottom:
                hiddenPos = new Vector2(0, -offsetDistance);
                arrowText.text = "⬇️";
                hintText.text = "SWIPE DOWN!";
                break;
            case ObstaclePosition.Left:
                hiddenPos = new Vector2(-offsetDistance, 0);
                arrowText.text = "⬅️";
                hintText.text = "SWIPE LEFT!";
                break;
            case ObstaclePosition.Right:
                hiddenPos = new Vector2(offsetDistance, 0);
                arrowText.text = "➡️";
                hintText.text = "SWIPE RIGHT!";
                break;
        }
        rectTransform.anchoredPosition = hiddenPos;
    }

    private IEnumerator SlideIn()
    {
        isAnimating = true;
        float t = 0;
        while(t < 1f)
        {
            t += Time.deltaTime * 5f; // スピード調整
            rectTransform.anchoredPosition = Vector2.Lerp(hiddenPos, showPos, t);
            yield return null;
        }
        rectTransform.anchoredPosition = showPos;
        isAnimating = false;
    }

    private IEnumerator SlideOut(bool success)
    {
        isAnimating = true;
        IsActive = false; // 先に判定を切る

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            // 成功時は来た方向へ戻る（hiddenPosへ）
            rectTransform.anchoredPosition = Vector2.Lerp(showPos, hiddenPos, t);
            yield return null;
        }
        
        gameObject.SetActive(false);
        isAnimating = false;
    }
}