using UnityEngine;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// TextMeshProのテキストを一文字ずつ表示するコンポーネント
/// ドラクエ風のテキスト表示アニメーション
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TextRevealAnimation : MonoBehaviour
{
    [Header("アニメーション設定")]
    [SerializeField] private float characterDelay = 0.05f; // 1文字あたりの表示間隔（秒）
    [SerializeField] private bool playOnEnable = false; // 有効化時に自動再生するか
    
    [Header("表示テキスト")]
    [SerializeField, TextArea(3, 10)] private string displayText = ""; // 表示するテキスト
    
    private TextMeshProUGUI textMesh;
    private string fullText;
    private Coroutine revealCoroutine;
    private bool isRevealing = false;
    
    /// <summary>
    /// テキスト表示が完了した時に呼ばれるイベント
    /// </summary>
    public event Action OnRevealCompleted;
    
    /// <summary>
    /// 現在アニメーション中かどうか
    /// </summary>
    public bool IsRevealing => isRevealing;
    
    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        fullText = displayText;
    }
    
    private void OnEnable()
    {
        if (playOnEnable && !string.IsNullOrEmpty(fullText))
        {
            StartReveal();
        }
    }
    
    private void OnDisable()
    {
        StopReveal();
    }
    
    /// <summary>
    /// テキスト表示アニメーションを開始
    /// </summary>
    public void StartReveal()
    {
        if (isRevealing)
        {
            StopReveal();
        }
        
        fullText = string.IsNullOrEmpty(displayText) ? textMesh.text : displayText;
        revealCoroutine = StartCoroutine(RevealText());
    }
    
    /// <summary>
    /// テキスト表示アニメーションを開始（テキストを指定）
    /// </summary>
    /// <param name="text">表示するテキスト</param>
    public void StartReveal(string text)
    {
        if (isRevealing)
        {
            StopReveal();
        }
        
        fullText = text;
        textMesh.text = text;
        revealCoroutine = StartCoroutine(RevealText());
    }
    
    /// <summary>
    /// テキスト表示アニメーションを停止
    /// </summary>
    public void StopReveal()
    {
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }
        isRevealing = false;
    }
    
    /// <summary>
    /// アニメーションをスキップして全文を即座に表示
    /// </summary>
    public void SkipToEnd()
    {
        StopReveal();
        textMesh.text = fullText;
        textMesh.maxVisibleCharacters = fullText.Length;
        OnRevealCompleted?.Invoke();
    }
    
    /// <summary>
    /// 1文字ずつ表示速度を変更
    /// </summary>
    /// <param name="delay">1文字あたりの表示間隔（秒）</param>
    public void SetCharacterDelay(float delay)
    {
        characterDelay = delay;
    }
    
    /// <summary>
    /// 表示するテキストを設定
    /// </summary>
    /// <param name="text">表示するテキスト</param>
    public void SetDisplayText(string text)
    {
        displayText = text;
    }
    
    private IEnumerator RevealText()
    {
        isRevealing = true;
        
        // テキストを設定
        textMesh.text = fullText;
        textMesh.maxVisibleCharacters = 0;
        
        // TextMeshProの情報を強制更新
        textMesh.ForceMeshUpdate();
        
        int totalCharacters = textMesh.textInfo.characterCount;
        
        // 1文字ずつ表示
        for (int i = 0; i <= totalCharacters; i++)
        {
            textMesh.maxVisibleCharacters = i;
            yield return new WaitForSeconds(characterDelay);
        }
        
        isRevealing = false;
        
        // 完了イベントを発火
        OnRevealCompleted?.Invoke();
    }
    
    /// <summary>
    /// イベントをすべてクリア
    /// </summary>
    public void ClearEvents()
    {
        OnRevealCompleted = null;
    }
}
