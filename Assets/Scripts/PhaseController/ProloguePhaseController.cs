using UnityEngine;
using TMPro;

/// <summary>
/// プロローグフェーズコントローラー
/// 責務: ゲームのストーリー導入部分の管理
/// </summary>
public class ProloguePhaseController : PhaseController
{
    [SerializeField, TextArea] private string[] prologueTexts;
    [SerializeField] private TextMeshProUGUI prologueTextUI;
    [SerializeField] private float charInterval = 0.05f; // 1文字を出す間隔（秒）

    private int _currentTextIndex;
    private int _currentCharIndex;
    private float _charTimer;
    private bool _isTyping;
    private string _currentText;

    public override GameState PhaseType => GameState.Prologue;

    protected override void OnEnterImpl()
    {
        AudioManager.Instance.PlayBGM(BGMType.Prologue);
        InputManager.Instance.OnTap += OnScreenTapped;
        StartPrologue();
    }

    public override void UpdatePhase()
    {
        if (!_isTyping) return;

        _charTimer += Time.deltaTime;
        if (_charTimer >= charInterval)
        {
            _charTimer -= charInterval;
            _currentCharIndex++;
            prologueTextUI.text = _currentText.Substring(0, Mathf.Min(_currentCharIndex, _currentText.Length));

            if (_currentCharIndex >= _currentText.Length)
            {
                FinishTypingCurrent();
            }
        }
    }

    protected override void OnExitImpl()
    {   
        InputManager.Instance.OnTap -= OnScreenTapped;
        _isTyping = false;
    }

    private void CompletePrologue()
    {
        RequestTransitionTo(GameState.Tutorial);
    }
    
    private void OnScreenTapped()
    {
        AudioManager.Instance.PlaySe(SeType.ButtonClick);
        
        if (_isTyping)
        {
            // タイピング中なら即座に全文表示
            prologueTextUI.text = _currentText;
            FinishTypingCurrent();
            return;
        }

        // 全文表示後にタップで次のテキストへ
        _currentTextIndex++;
        if (_currentTextIndex >= prologueTexts.Length)
        {
            CompletePrologue();
        }
        else
        {
            StartCurrentText();
        }
    }

    private void StartPrologue()
    {
        _currentTextIndex = 0;
        StartCurrentText();
    }

    private void StartCurrentText()
    {
        if (prologueTexts == null || prologueTexts.Length == 0)
        {
            CompletePrologue();
            return;
        }

        _currentText = prologueTexts[_currentTextIndex];
        _currentCharIndex = 0;
        _charTimer = 0f;
        _isTyping = true;
        prologueTextUI.text = string.Empty;
    }

    private void FinishTypingCurrent()
    {
        _isTyping = false;
        _charTimer = 0f;
        _currentCharIndex = _currentText.Length;
    }
    
}
