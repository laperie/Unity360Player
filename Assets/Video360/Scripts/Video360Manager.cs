/*
 * MIT License
 * 
 * Copyright (c) 2025 Unity 360° Video Player
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 */

using UnityEngine;
using UnityEngine.Video;

public class Video360Manager : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private KeyCode quitKey = KeyCode.Q;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugInfo = true;
    [SerializeField] private KeyCode debugKey = KeyCode.F9;
    
    private VideoController videoController;
    private bool showDebugInfo = false;
    
    void Start()
    {
        videoController = FindObjectOfType<VideoController>();
        
        if (enableDebugInfo)
        {
            LogDebug("Video360Manager started");
            LogDebug($"Running in: {(Application.isEditor ? "Unity Editor" : "Built Application")}");
            
            if (videoController != null)
            {
                LogDebug($"Found VideoController: {videoController.name}");
                
                // Check and fix URL for built applications
                var config = videoController.GetComponent<VideoController>()?.GetType()
                    .GetField("config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .GetValue(videoController) as Video360Config;
                    
                if (config != null && !string.IsNullOrEmpty(config.videoURL))
                {
                    LogDebug($"Video URL: {config.videoURL}");
                    if (config.videoURL.Contains("localhost") && !Application.isEditor)
                    {
                        LogDebug("WARNING: Using 'localhost' in built application may not work. Consider using '127.0.0.1' instead.", true);
                    }
                }
            }
            else
            {
                LogDebug("WARNING: No VideoController found in scene!", true);
            }
        }
    }
    
    void Update()
    {
        // Quit application
        if (Input.GetKeyDown(quitKey))
        {
            LogDebug("Quit key pressed - exiting application");
            QuitApplication();
        }
        
        // Toggle debug info
        if (Input.GetKeyDown(debugKey))
        {
            showDebugInfo = !showDebugInfo;
            LogDebug($"Debug info display: {(showDebugInfo ? "ON" : "OFF")}");
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("<b>360° Video Debug Info</b>", new GUIStyle(GUI.skin.label) { richText = true });
        GUILayout.Space(10);
        
        // Video Controller Info
        if (videoController != null)
        {
            var videoPlayer = videoController.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                GUILayout.Label($"<color=green>VideoPlayer Status:</color>", new GUIStyle(GUI.skin.label) { richText = true });
                GUILayout.Label($"  • URL: {videoPlayer.url}");
                GUILayout.Label($"  • Is Prepared: {videoPlayer.isPrepared}");
                GUILayout.Label($"  • Is Playing: {videoPlayer.isPlaying}");
                GUILayout.Label($"  • Frame Count: {videoPlayer.frameCount}");
                GUILayout.Label($"  • Current Frame: {videoPlayer.frame}");
                GUILayout.Label($"  • Target Texture: {videoPlayer.targetTexture?.name ?? "None"}");
                GUILayout.Label($"  • Render Mode: {videoPlayer.renderMode}");
                
                // Check if render texture has actual video data
                if (videoPlayer.targetTexture != null)
                {
                    var rt = videoPlayer.targetTexture;
                    GUILayout.Label($"  • Render Texture Size: {rt.width}x{rt.height}");
                    GUILayout.Label($"  • Render Texture Format: {rt.format}");
                    GUILayout.Label($"  • Is Created: {rt.IsCreated()}");
                    
                    // Try to detect if texture has video data
                    if (rt.IsCreated())
                    {
                        GUILayout.Label($"  • Texture Status: Active");
                    }
                    else
                    {
                        GUILayout.Label($"  • Texture Status: NOT CREATED - PROBLEM!");
                    }
                }
                
                GUILayout.Space(5);
            }
        }
        else
        {
            GUILayout.Label("<color=red>No VideoController found!</color>", new GUIStyle(GUI.skin.label) { richText = true });
        }
        
        // Material Info
        var sphereRenderer = FindObjectOfType<MeshRenderer>();
        if (sphereRenderer != null)
        {
            var material = sphereRenderer.material;
            GUILayout.Label($"<color=green>Sphere Material:</color>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Label($"  • Material: {material.name}");
            GUILayout.Label($"  • Shader: {material.shader.name}");
            GUILayout.Label($"  • Main Texture: {material.mainTexture?.name ?? "NONE - THIS IS THE PROBLEM!"}");
            
            // Check if texture matches render texture
            if (videoController != null)
            {
                var videoPlayer = videoController.GetComponent<VideoPlayer>();
                if (videoPlayer?.targetTexture != null)
                {
                    bool texturesMatch = material.mainTexture == videoPlayer.targetTexture;
                    GUILayout.Label($"  • Render Texture Connected: {(texturesMatch ? "YES" : "NO - PROBLEM!")}");
                    if (!texturesMatch)
                    {
                        GUILayout.Label($"    Expected: {videoPlayer.targetTexture.name}");
                        GUILayout.Label($"    Actual: {material.mainTexture?.name ?? "None"}");
                    }
                }
            }
            GUILayout.Space(5);
        }
        
        // Camera Info
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            GUILayout.Label($"<color=green>Camera Info:</color>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Label($"  • Position: {mainCamera.transform.position}");
            GUILayout.Label($"  • Rotation: {mainCamera.transform.eulerAngles}");
            GUILayout.Label($"  • FOV: {mainCamera.fieldOfView}");
            GUILayout.Label($"  • Clear Flags: {mainCamera.clearFlags}");
            GUILayout.Space(5);
        }

        // Sphere Transform Info  
        var sphereTransform = sphereRenderer?.transform;
        if (sphereTransform != null)
        {
            GUILayout.Label($"<color=green>Sphere Transform:</color>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Label($"  • Position: {sphereTransform.position}");
            GUILayout.Label($"  • Scale: {sphereTransform.localScale}");
            GUILayout.Label($"  • Active: {sphereTransform.gameObject.activeInHierarchy}");
            GUILayout.Space(5);
        }
        
        GUILayout.Space(10);
        GUILayout.Label($"<color=yellow>Controls:</color>", new GUIStyle(GUI.skin.label) { richText = true });
        GUILayout.Label($"  • Press '{quitKey}' to quit");
        GUILayout.Label($"  • Press '{debugKey}' (F9) to toggle this debug info");
        GUILayout.Label($"  • Mouse: Look around");
        GUILayout.Space(5);
        GUILayout.Label($"<color=orange>Troubleshooting:</color>", new GUIStyle(GUI.skin.label) { richText = true });
        GUILayout.Label($"  • Can you hear audio? (Server working)");
        GUILayout.Label($"  • Is camera inside sphere? (Scale x: -1)");
        GUILayout.Label($"  • Is render texture active?");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    void QuitApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void LogDebug(string message, bool isWarning = false)
    {
        if (!enableDebugInfo) return;
        
        string prefix = "[Video360Manager] ";
        if (isWarning)
        {
            Debug.LogWarning(prefix + message);
        }
        else
        {
            Debug.Log(prefix + message);
        }
    }
}