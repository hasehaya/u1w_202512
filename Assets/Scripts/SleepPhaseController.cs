using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SleepPhaseController : MonoBehaviour
{
    [SerializeField] private Text timerText;
    [SerializeField] private Text checkCountText;
    [SerializeField] private Button checkButton;
    [SerializeField] private GameObject sheepImage; // 演出用

    private int checkCount = 3;
    private float totalTime;
    private float phaseStartTime;
    private bool isRunning = false;

    public void Initialize(float timeLimit)
    {
        totalTime = timeLimit;
        checkCount = 3;
        phaseStartTime = Time.time;
        isRunning = true;
        
        checkButton.interactable = true;
        UpdateUI();

        // 最初だけ時間を表示
        StartCoroutine(ShowTimerBriefly());
    }

    private void Update()
    {
        if (!isRunning || GameManager.Instance.CurrentState != GameState.Sleep) return;

        float elapsed = Time.time - phaseStartTime;
        float remaining = totalTime - elapsed;

        if (remaining <= 0)
        {
            isRunning = false;
            GameManager.Instance.ChangeState(GameState.Run); // 強制起床
        }
    }

    // ボタンから呼ばれる
    public void OnCheckTime()
    {
        if (checkCount > 0)
        {
            checkCount--;
            UpdateUI();
            StartCoroutine(ShowTimerBriefly());
            
            if (checkCount <= 0) checkButton.interactable = false;
        }
    }

    // ボタンから呼ばれる
    public void OnWakeUp()
    {
        isRunning = false;
        GameManager.Instance.ChangeState(GameState.Run);
    }

    private void UpdateUI()
    {
        checkCountText.text = $"{checkCount}";
    }

    private IEnumerator ShowTimerBriefly()
    {
        float elapsed = Time.time - phaseStartTime;
        float currentRem = Mathf.Max(0, totalTime - elapsed);
        
        timerText.text = currentRem.ToString("F2");
        timerText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1.5f);
        
        timerText.gameObject.SetActive(false);
    }
}