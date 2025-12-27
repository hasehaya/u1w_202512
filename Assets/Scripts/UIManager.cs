using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject prologuePanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject sleepPanel;
    [SerializeField] private GameObject runPanel;
    [SerializeField] private GameObject resultPanel;

    [Header("Result UI")]
    [SerializeField] private Text resultSleepTimeText;
    [SerializeField] private Text resultRemainingTimeText;
    [SerializeField] private Text resultScoreText;
    [SerializeField] private Text resultTitleText;

    public void SwitchPanel(GameState state)
    {
        titlePanel.SetActive(false);
        prologuePanel.SetActive(false);
        tutorialPanel.SetActive(false);
        sleepPanel.SetActive(false);
        runPanel.SetActive(false);
        resultPanel.SetActive(false);

        switch (state)
        {
            case GameState.Title: titlePanel.SetActive(true); break;
            case GameState.Prologue: prologuePanel.SetActive(true); break;
            case GameState.Tutorial: tutorialPanel.SetActive(true); break;
            case GameState.Sleep: sleepPanel.SetActive(true); break;
            case GameState.Run: runPanel.SetActive(true); break;
            case GameState.Result: resultPanel.SetActive(true); break;
        }
    }

    public void ShowResult(float sleepTime, float remainTime, int score)
    {
        resultSleepTimeText.text = $"{sleepTime:F2}s";
        
        if (score > 0)
        {
            resultTitleText.text = "切り替え完璧社会人 クリア！";
            resultTitleText.color = Color.green;
            resultRemainingTimeText.text = $"{remainTime:F2}s";
        }
        else
        {
            resultTitleText.text = "ちこく確定... (クビ)";
            resultTitleText.color = Color.red;
            resultRemainingTimeText.text = "Time Up";
        }

        resultScoreText.text = $"{score}";
    }
    
    // UIボタン用
    public void OnClickStart() => GameManager.Instance.ChangeState(GameState.Prologue);
    public void OnClickToTutorial() => GameManager.Instance.ChangeState(GameState.Tutorial);
    public void OnClickToGame() => GameManager.Instance.ChangeState(GameState.Sleep);
    public void OnClickReset() => GameManager.Instance.ChangeState(GameState.Title);
}