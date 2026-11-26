using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Assignment10Tools
{
    [MenuItem("Tools/Assignment 10/Open Enemy Lab")]
    static void Open()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/enemy_lab.unity");
    }
    [MenuItem("Tools/Assignment 10/Validate encounter")]
    public static void Validate()
    {
        if (!File.Exists("Assets/enemy_lab.unity")) throw new Exception("Missing enemy_lab scene.");
        foreach (string name in new[] { "Patroller", "Shooter", "Charger" })
        {
            EnemyTuning t = AssetDatabase.LoadAssetAtPath<EnemyTuning>("Assets/Resources/EnemyLab/" + name + "Settings.asset");
            if (t == null || t.windUp < 0.3f || t.windUp > 0.8f || t.cooldown <= 0 || t.hitPoints < 1 || t.chaseRange < t.visionRange)
                throw new Exception("Invalid tuning asset: " + name);
        }
        if (Application.isPlaying)
        {
            if (UnityEngine.Object.FindFirstObjectByType<PatrollerBrain>() == null ||
                UnityEngine.Object.FindFirstObjectByType<ShooterBrain>() == null ||
                UnityEngine.Object.FindFirstObjectByType<ChargerBrain>() == null)
                throw new Exception("Run level1, level2 or enemy_lab with all three enemies alive and Enemies ON before checking.");
            EnemyLabBootstrap lab = UnityEngine.Object.FindFirstObjectByType<EnemyLabBootstrap>();
            CampaignEnemies campaign = UnityEngine.Object.FindFirstObjectByType<CampaignEnemies>();
            bool validLab = lab != null && lab.Enemies != null && lab.Enemies.Length == 3;
            bool validCampaign = campaign != null && campaign.Enemies != null && campaign.Enemies.Length == 3;
            if (!validLab && !validCampaign) throw new Exception("Missing encounter bootstrap.");
        }
        Debug.Log("Assignment 10 asset checks passed. Phone playtests and behaviour verification are still required.");
    }
}
