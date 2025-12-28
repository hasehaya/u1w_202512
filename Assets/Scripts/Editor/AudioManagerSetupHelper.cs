using UnityEngine;
using UnityEditor;

/// <summary>
/// AudioManagerのセットアップを支援するエディタスクリプト
/// </summary>
public class AudioManagerSetupHelper : EditorWindow
{
    [MenuItem("Tools/Audio/Setup AudioManager")]
    public static void SetupAudioManager()
    {
        // AudioManagerが既に存在するか確認
        AudioManager existingManager = FindFirstObjectByType<AudioManager>();
        if (existingManager != null)
        {
            bool result = EditorUtility.DisplayDialog(
                "AudioManager Setup",
                "AudioManagerは既にシーンに存在します。\n選択しますか？",
                "選択する",
                "キャンセル"
            );
            
            if (result)
            {
                Selection.activeGameObject = existingManager.gameObject;
                EditorGUIUtility.PingObject(existingManager.gameObject);
            }
            return;
        }
        
        // AudioManagerオブジェクトを作成
        GameObject audioManagerObject = new GameObject("AudioManager");
        AudioManager audioManager = audioManagerObject.AddComponent<AudioManager>();
        
        // AudioClipDataアセットを探す
        string[] guids = AssetDatabase.FindAssets("t:AudioClipData");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            AudioClipData clipData = AssetDatabase.LoadAssetAtPath<AudioClipData>(path);
            
            SerializedObject serializedObject = new SerializedObject(audioManager);
            SerializedProperty clipDataProperty = serializedObject.FindProperty("audioClipData");
            clipDataProperty.objectReferenceValue = clipData;
            serializedObject.ApplyModifiedProperties();
            
            Debug.Log($"AudioManagerをセットアップしました。AudioClipData: {clipData.name}");
        }
        else
        {
            Debug.LogWarning("AudioClipDataアセットが見つかりませんでした。手動で設定してください。");
        }
        
        // 作成したオブジェクトを選択
        Selection.activeGameObject = audioManagerObject;
        EditorGUIUtility.PingObject(audioManagerObject);
        
        // Undo登録
        Undo.RegisterCreatedObjectUndo(audioManagerObject, "Setup AudioManager");
    }
    
    [MenuItem("Tools/Audio/Create AudioClipData Asset")]
    public static void CreateAudioClipDataAsset()
    {
        // Resourcesフォルダのパスを確認
        string resourcesPath = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        string audioFolderPath = resourcesPath + "/Audio";
        if (!AssetDatabase.IsValidFolder(audioFolderPath))
        {
            AssetDatabase.CreateFolder(resourcesPath, "Audio");
        }
        
        // AudioClipDataアセットを作成
        AudioClipData asset = ScriptableObject.CreateInstance<AudioClipData>();
        
        string assetPath = audioFolderPath + "/MainAudioClipData.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 作成したアセットを選択
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
        
        Debug.Log($"AudioClipDataアセットを作成しました: {assetPath}");
    }
    
    [MenuItem("Tools/Audio/Open Audio Manager Guide")]
    public static void OpenAudioManagerGuide()
    {
        string guidePath = "Assets/../AUDIO_MANAGER_GUIDE.md";
        System.Diagnostics.Process.Start(guidePath);
    }
}

