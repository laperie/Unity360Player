using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "Video360Config", menuName = "Video360/Config", order = 1)]
public class Video360Config : ScriptableObject
{
    [System.Serializable]
    public enum RenderMode
    {
        Sphere,
        Skybox
    }

    [System.Serializable]
    public enum StereoLayout
    {
        None,
        SideBySide,
        OverUnder
    }

    [Header("Video Source")]
    [Tooltip("URL for streaming video (takes priority over VideoClip)")]
    public string videoURL = "";
    
    [Tooltip("Local video clip (used if URL is empty)")]
    public VideoClip videoClip;

    [Header("Render Settings")]
    public RenderMode renderMode = RenderMode.Sphere;
    public StereoLayout stereoLayout = StereoLayout.None;
    
    [Tooltip("Render texture for video output")]
    public RenderTexture renderTexture;

    [Header("Materials")]
    [Tooltip("Material for sphere rendering")]
    public Material sphereMaterial;
    
    [Tooltip("Material for skybox rendering - Mono")]
    public Material skyboxMaterialMono;
    
    [Tooltip("Material for skybox rendering - Side by Side")]
    public Material skyboxMaterialSBS;
    
    [Tooltip("Material for skybox rendering - Over Under")]
    public Material skyboxMaterialOU;

    [Header("Playback Settings")]
    [Tooltip("Start playback automatically when prepared")]
    public bool autoPlay = true;
    
    [Tooltip("Loop the video")]
    public bool isLooping = true;
    
    [Range(0f, 1f)]
    [Tooltip("Audio volume")]
    public float volume = 1f;

    public Material GetSkyboxMaterial()
    {
        switch (stereoLayout)
        {
            case StereoLayout.SideBySide:
                return skyboxMaterialSBS;
            case StereoLayout.OverUnder:
                return skyboxMaterialOU;
            default:
                return skyboxMaterialMono;
        }
    }

    public bool HasValidSource()
    {
        return !string.IsNullOrEmpty(videoURL) || videoClip != null;
    }
}