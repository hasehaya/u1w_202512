using UnityEngine;

/// <summary>
/// [廃止予定] UI表示を専門に担当するマネージャー
/// 
/// このクラスの機能はGameManagerに統合されました。
/// UIパネルの切り替えはGameManager.SwitchPanel()で行われます。
/// 
/// 理由:
/// - UIManagerの責務はパネル切り替えのみで小さすぎた
/// - GameManagerがフェーズ管理をしているため、UI切り替えも統合した方が自然
/// - クラス数を減らしてシンプルに保つため
/// 
/// このファイルは後方互換性のために残していますが、使用しないでください。
/// Unityシーン上のGameManagerオブジェクトからUIManager参照を削除してください。
/// </summary>
[System.Obsolete("UIManagerはGameManagerに統合されました。GameManagerを使用してください。")]
public class UIManager : MonoBehaviour
{

}