using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditor.Rendering;
using System.IO;
using System.Runtime.Remoting.Messaging;

#if UNITY_EDITOR
public class EnemyAIEditor : EditorWindow
{
    // 指定搜索的文件路径和保存路径
    private string[] enemyActionPaths = new string[] {"Red","Yellow","Purple" };
    private int selectedPathIndex = 0;
    private string buildingActionPath;
    private string trainingActionPath;
    private string savePath;

    private List<BuildingActionSO> availableBuildings = new();
    private BuildingActionSO selectedBuilding;

    private List<TrainingActionSO> availableTrainings = new();
    private Dictionary<TrainingActionSO, int> trainingSelectionCounts = new();
   // private float Timer = 0;

    [MenuItem("Tools/Enemy AI Creator")]
    public static void ShowWindow()
    {
        GetWindow<EnemyAIEditor>("Enemy AI Creator");
    }

    private void OnEnable()
    {
        UpdateEnemyActionPath();
        LoadAssets();
    }

    private void UpdateEnemyActionPath()
    {
        buildingActionPath = $"Assets/ScriptableObject/Structure/{enemyActionPaths[selectedPathIndex]}";
        trainingActionPath = $"Assets/ScriptableObject/Army/{enemyActionPaths[selectedPathIndex]}";
        savePath = $"Assets/ScriptableObject/EnemyAIStage/{enemyActionPaths[selectedPathIndex]}";
    }

    private void LoadAssets()
    {
        availableBuildings = AssetDatabase.FindAssets("t:BuildingActionSO", new[] { buildingActionPath })
                                    .Select(guid => AssetDatabase.LoadAssetAtPath<BuildingActionSO>(AssetDatabase.GUIDToAssetPath(guid))).ToList();

        availableTrainings = AssetDatabase.FindAssets("t:TrainingActionSO", new[] { trainingActionPath }).
                            Select(guid => AssetDatabase.LoadAssetAtPath<TrainingActionSO>(AssetDatabase.GUIDToAssetPath(guid))).ToList();

        foreach (var training in availableTrainings) // 更安全的初始化方式
        {
            if (!trainingSelectionCounts.ContainsKey(training))
            {
                trainingSelectionCounts[training] = 0;
            }
        }
    }


    private void OnGUI()
    {
        GUILayout.Label("Enemy AI Config Creator", EditorStyles.boldLabel);

         EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        int newIndex = EditorGUILayout.Popup( "Enemy Action Path",selectedPathIndex, enemyActionPaths);
        if (newIndex != selectedPathIndex)
        {
            // 用户选择了新的路径，弹出确认窗口
            if (EditorUtility.DisplayDialog("确认更改路径",
                $"是否将路径切换到 '{enemyActionPaths[newIndex]}'？",
                "确认", "取消"))
            {
                selectedPathIndex = newIndex;
                UpdateEnemyActionPath();
                LoadAssets();
            }
        }
        EditorGUILayout.EndHorizontal();
         EditorGUILayout.Space(10);

        DrawBuildingSelection();
        EditorGUILayout.Space(10);

        DrawTrainingSelection();
        EditorGUILayout.Space(20);

        if (GUILayout.Button("Generate Enemy AI Config"))
        {
            GenerateConfigAsset();
        }

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Reset All Config"))
        {
            ResetConfig();
        }

        GUI.enabled = true;
    }

    private void DrawBuildingSelection()
    {
        GUILayout.Label("Select Building Action:", EditorStyles.label);

        for (int i = 0; i < availableBuildings.Count; i++)
        {
            GUILayout.Space(5);
            var building = availableBuildings[i];
            bool isSelected = selectedBuilding == building;

            EditorGUI.BeginChangeCheck(); // 检测 toggle 是否被点击
            bool toggled = EditorGUILayout.ToggleLeft(building.name, isSelected);

            if (EditorGUI.EndChangeCheck() && toggled)
            {
                selectedBuilding = building; // 选择当前项
            }
            else if (EditorGUI.EndChangeCheck() && !toggled && isSelected)
            {
                selectedBuilding = null; // 取消当前选择
            }
        }
        if (selectedBuilding != null)
        {
            EditorGUILayout.HelpBox("Selected: " + selectedBuilding.name, MessageType.Info);
        }
    }

    private void DrawTrainingSelection()
    {
        GUILayout.Label("Select Training Actions (with Quantity):", EditorStyles.label);

        foreach (var training in availableTrainings)
        {
            int count = trainingSelectionCounts[training];

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();

            // 数量标志
            GUILayout.Label(count.ToString(), GUILayout.Width(20));

            // 减按钮
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                if (count > 0)
                    trainingSelectionCounts[training]--;
            }

            // 加按钮
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                trainingSelectionCounts[training]++;
            }
            GUILayout.Space(10);
            // 名称显示
            EditorGUILayout.LabelField(training.name);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
        }
    }


    private void GenerateConfigAsset()
    {
        var config = ScriptableObject.CreateInstance<EnemyAIStageSO>();
        config.BuildingAction = selectedBuilding;
        if (selectedBuilding == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a building action.", "OK");
            ResetConfig();
            return;
        }

        var finalTrainings = new List<TrainingActionSO>();

        foreach (var kvp in trainingSelectionCounts)
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                finalTrainings.Add(kvp.Key);
            }
        }

        config.TrainingActions = finalTrainings;
        config.StageExistTimer = 15 + finalTrainings.Count * 5;

        if (!IsTrainingSelectionVaild())
        {
            EditorUtility.DisplayDialog("Error", "Please select a training action at least.", "OK");
            return;
        }
        string fileName = $"Enemy_AI_Config_{selectedBuilding.name.Substring(0, 1)}_{GenerateCountID()}.asset";
        string fullPath = $"{savePath}/{fileName}";

        if (File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("Duplicate Detected", $"A config file {fileName} already exists.", "OK");
            ResetConfig();
            return;
        }
//        Timer = config.TrainingActions.

        AssetDatabase.CreateAsset(config, fullPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created config: {fullPath}");
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = config;

        ResetConfig();
    }

    private void ResetConfig()
    {
        selectedBuilding = null;

        var keys = trainingSelectionCounts.Keys.ToList();
        foreach (var key in keys)
        {
            trainingSelectionCounts[key] = 0;
        }

        Repaint();
    }

    private string GenerateCountID()
    {
        string sb = "";
        var keys = trainingSelectionCounts.Keys;

        foreach (var key in keys)
        {
            sb += trainingSelectionCounts[key].ToString();
        }
        return sb;

    }

    private bool IsTrainingSelectionVaild()
    {
        var keys = trainingSelectionCounts.Keys;

        foreach (var key in keys)
        {
            if (trainingSelectionCounts[key] != 0)
            {
                return true;
            }
        }
        return false;
    }

}

#endif