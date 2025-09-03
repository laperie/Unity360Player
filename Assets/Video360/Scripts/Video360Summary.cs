using UnityEngine;

// Summary script to verify all components are working and properly linked
public class Video360Summary : MonoBehaviour
{
    [Header("System Status")]
    [SerializeField] private bool allComponentsFound = false;
    [SerializeField] private string[] componentStatus;
    [SerializeField] private bool allReferencesValid = false;

    void Start()
    {
        ValidateComponents();
        ValidateReferences();
    }

    [ContextMenu("Validate Components")]
    void ValidateComponents()
    {
        var statusList = new System.Collections.Generic.List<string>();
        
        // Check if Video360Config exists
        var config = Resources.Load<Video360Config>("Config/DefaultVideoConfig");
        if (config != null)
        {
            statusList.Add("✅ Video360Config: Found");
            
            // Check config properties
            if (!string.IsNullOrEmpty(config.videoURL))
            {
                statusList.Add($"✅ Video URL configured: {config.videoURL}");
            }
            else if (config.videoClip != null)
            {
                statusList.Add($"✅ VideoClip configured: {config.videoClip.name}");
            }
            else
            {
                statusList.Add("⚠️ No video source configured");
            }
        }
        else
        {
            statusList.Add("❌ Video360Config: Missing");
        }
        
        // Check for VideoController script
        var videoController = FindObjectOfType<VideoController>();
        if (videoController != null)
        {
            statusList.Add("✅ VideoController: Found in scene");
            
            // Check if VideoController has VideoPlayer component
            var videoPlayer = videoController.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null)
            {
                statusList.Add("✅ VideoPlayer component: Found");
            }
            else
            {
                statusList.Add("❌ VideoPlayer component: Missing");
            }
            
            // Check if VideoController has AudioSource component
            var audioSource = videoController.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                statusList.Add("✅ AudioSource component: Found");
            }
            else
            {
                statusList.Add("❌ AudioSource component: Missing");
            }
        }
        else
        {
            statusList.Add("⚠️ VideoController: Not found in scene");
        }
        
        // Check for PlayerLookController script
        var lookController = FindObjectOfType<PlayerLookController>();
        if (lookController != null)
        {
            statusList.Add("✅ PlayerLookController: Found in scene");
        }
        else
        {
            statusList.Add("⚠️ PlayerLookController: Not found in scene");
        }

#if UNITY_EDITOR
        // Check for SimpleVideoServer script
        var videoServer = FindObjectOfType<SimpleVideoServer>();
        if (videoServer != null)
        {
            statusList.Add("✅ SimpleVideoServer: Found in scene (Editor only)");
            if (videoServer.IsServerRunning)
            {
                statusList.Add($"✅ HTTP Server running at: {videoServer.GetServerURL()}");
            }
            else
            {
                statusList.Add("⚠️ HTTP Server not yet started (starts in Play mode)");
            }
        }
        else
        {
            statusList.Add("⚠️ SimpleVideoServer: Not found in scene");
        }
#endif

        componentStatus = statusList.ToArray();
        
        Debug.Log("[Video360Summary] Component validation completed:");
        foreach (string status in componentStatus)
        {
            Debug.Log($"  {status}");
        }
        
        allComponentsFound = statusList.Count > 0 && !System.Array.Exists(componentStatus, s => s.StartsWith("❌"));
    }

    [ContextMenu("Validate References")]
    void ValidateReferences()
    {
        Debug.Log("[Video360Summary] Validating GUID references...");
        
        bool allValid = true;
        
        // Check if prefabs can be loaded
        var sphereRigPrefab = Resources.Load("Video360/Prefabs/PF_360SphereRig");
        if (sphereRigPrefab == null)
        {
            Debug.LogError("❌ PF_360SphereRig prefab not found");
            allValid = false;
        }
        else
        {
            Debug.Log("✅ PF_360SphereRig prefab loaded successfully");
        }
        
        var videoPlayerPrefab = Resources.Load("Video360/Prefabs/PF_VideoPlayer");
        if (videoPlayerPrefab == null)
        {
            Debug.LogError("❌ PF_VideoPlayer prefab not found");
            allValid = false;
        }
        else
        {
            Debug.Log("✅ PF_VideoPlayer prefab loaded successfully");
        }
        
        var serverPrefab = Resources.Load("Video360/Prefabs/VideoServerManager");
        if (serverPrefab == null)
        {
            Debug.LogError("❌ VideoServerManager prefab not found");
            allValid = false;
        }
        else
        {
            Debug.Log("✅ VideoServerManager prefab loaded successfully");
        }
        
        allReferencesValid = allValid;
        
        if (allValid)
        {
            Debug.Log("✅ All GUID references appear to be valid");
        }
        else
        {
            Debug.LogError("❌ Some GUID references are broken");
        }
    }

    [ContextMenu("Full System Check")]
    void FullSystemCheck()
    {
        ValidateComponents();
        ValidateReferences();
        
        Debug.Log("=== FULL SYSTEM CHECK RESULTS ===");
        Debug.Log($"Components Found: {allComponentsFound}");
        Debug.Log($"References Valid: {allReferencesValid}");
        
        if (allComponentsFound && allReferencesValid)
        {
            Debug.Log("🎉 Unity 360° Video Player is ready to use!");
        }
        else
        {
            Debug.LogWarning("⚠️ System setup incomplete - check logs above");
        }
    }
}