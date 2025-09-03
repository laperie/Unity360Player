using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
public class VideoController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Video360Config config;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    private bool isInitialized = false;

    public System.Action OnVideoReady;
    public System.Action OnVideoStarted;
    public System.Action<string> OnVideoError;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        audioSource = GetComponent<AudioSource>();
        
        videoPlayer.prepareCompleted += OnPrepareCompleted;
        videoPlayer.errorReceived += OnErrorReceived;
        videoPlayer.started += OnVideoStartedEvent;
    }

    void Start()
    {
        if (config != null && config.HasValidSource())
        {
            InitializeVideo();
        }
        else
        {
            LogDebug("No valid video source found in config");
        }
    }

    public void SetConfig(Video360Config newConfig)
    {
        config = newConfig;
        if (config != null && config.HasValidSource())
        {
            InitializeVideo();
        }
    }

    void InitializeVideo()
    {
        if (isInitialized)
        {
            LogDebug("Video already initialized, preparing...");
            PrepareVideo();
            return;
        }

        LogDebug("Initializing video player...");

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = config.isLooping;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = config.renderTexture;

        audioSource.playOnAwake = false;
        audioSource.volume = config.volume;

        if (!string.IsNullOrEmpty(config.videoURL))
        {
            LogDebug($"Setting video URL: {config.videoURL}");
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = config.videoURL;
        }
        else if (config.videoClip != null)
        {
            LogDebug($"Setting video clip: {config.videoClip.name}");
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = config.videoClip;
        }

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        isInitialized = true;
        PrepareVideo();
    }

    void PrepareVideo()
    {
        if (!isInitialized)
        {
            LogDebug("Cannot prepare video - not initialized");
            return;
        }

        LogDebug("Preparing video...");
        videoPlayer.Prepare();
    }

    public void PlayVideo()
    {
        if (!videoPlayer.isPrepared)
        {
            LogDebug("Video not prepared yet, preparing first...");
            PrepareVideo();
            return;
        }

        if (!videoPlayer.isPlaying)
        {
            LogDebug("Starting video playback");
            videoPlayer.Play();
        }
    }

    public void PauseVideo()
    {
        if (videoPlayer.isPlaying)
        {
            LogDebug("Pausing video");
            videoPlayer.Pause();
        }
    }

    public void StopVideo()
    {
        if (videoPlayer.isPlaying)
        {
            LogDebug("Stopping video");
            videoPlayer.Stop();
        }
    }

    void OnPrepareCompleted(VideoPlayer vp)
    {
        LogDebug("Video preparation completed");
        OnVideoReady?.Invoke();
        
        if (config.autoPlay)
        {
            PlayVideo();
        }
    }

    void OnErrorReceived(VideoPlayer vp, string message)
    {
        LogDebug($"Video error: {message}", true);
        OnVideoError?.Invoke(message);
    }

    void OnVideoStartedEvent(VideoPlayer vp)
    {
        LogDebug("Video playback started");
        OnVideoStarted?.Invoke();
    }

    void LogDebug(string message, bool isError = false)
    {
        if (!enableDebugLogs) return;

        string logMessage = $"[VideoController] {message}";
        if (isError)
            Debug.LogError(logMessage);
        else
            Debug.Log(logMessage);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnPrepareCompleted;
            videoPlayer.errorReceived -= OnErrorReceived;
            videoPlayer.started -= OnVideoStartedEvent;
        }
    }

    public bool IsPlaying => videoPlayer != null && videoPlayer.isPlaying;
    public bool IsPrepared => videoPlayer != null && videoPlayer.isPrepared;
    public double Length => videoPlayer != null ? videoPlayer.length : 0;
    public double Time => videoPlayer != null ? videoPlayer.time : 0;
}