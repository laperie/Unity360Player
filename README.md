# Unity 360° Video Player

A complete Unity package for immersive 360° video playback with built-in HTTP server, cross-platform controls, and multiple rendering modes. Perfect for VR applications, architectural visualization, and interactive media experiences.

![Unity Version](https://img.shields.io/badge/Unity-2022.3+-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Desktop%20%7C%20Mobile%20%7C%20WebGL-green.svg)
![License](https://img.shields.io/badge/License-Educational-yellow.svg)

## 🚀 Quick Start (60 seconds)

1. **Import the Package**
   ```
   Copy the Assets/Video360/ folder to your Unity project
   ```

2. **Open Demo Scene**
   ```
   Assets/Video360/Scenes/Video360_Demo.unity
   ```

3. **Add Your Video**
   ```
   Drop your 360° MP4 file into: Assets/StreamingAssets/Videos/
   Rename it to: sample360.mp4
   ```

4. **Press Play** ▶️
   ```
   The HTTP server starts automatically
   Your 360° video plays immediately at http://localhost:8080/
   ```

**That's it!** Use mouse/touch to look around your 360° video.

---

## 📋 Table of Contents

- [Features](#features)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [HTTP Server Guide](#http-server-guide)
- [Usage Guide](#usage-guide)
- [Configuration](#configuration)
- [Platform Support](#platform-support)
- [Troubleshooting](#troubleshooting)
- [API Reference](#api-reference)
- [Development Tools](#development-tools)
- [Performance Tips](#performance-tips)
- [FAQ](#faq)

---

## ✨ Features

### Core Functionality
- 🎥 **360° Video Playback** - Equirectangular format support
- 🌐 **Built-in HTTP Server** - Local video streaming for development
- 🎮 **Multi-Input Controls** - Mouse, touch, gyroscope support
- 🔄 **Multiple Render Modes** - Sphere geometry and Unity skybox
- 👀 **Stereo Video Support** - Mono, Side-by-Side, Over-Under layouts
- 📱 **Cross-Platform** - Desktop, Mobile, WebGL compatible

### Developer Tools
- 🛠️ **Test Video Generator** - Create procedural 360° test content
- ✅ **Validation Tests** - Component verification system
- 📊 **Performance Monitoring** - Debug logs and status indicators
- 🎯 **Editor Integration** - Custom menu items and inspectors

---

## 🖥️ System Requirements

### Unity Version
- **Unity 2022.3 LTS** or higher
- **Built-in Render Pipeline** (URP/HDRP compatible with modifications)

### Platform Requirements
| Platform | Minimum | Recommended |
|----------|---------|-------------|
| **Windows** | Win 10, 4GB RAM | Win 11, 8GB RAM |
| **macOS** | macOS 10.15, 4GB RAM | macOS 12+, 8GB RAM |
| **iOS** | iOS 12, A12 chip | iOS 15+, A14 chip |
| **Android** | API 24, OpenGL ES 3.0 | API 30, Vulkan |
| **WebGL** | Chrome 90+, Firefox 88+ | Chrome 100+, 8GB RAM |

### Video Requirements
- **Format**: MP4, WebM, MOV
- **Codec**: H.264 (recommended), VP8/VP9
- **Projection**: Equirectangular (360°)
- **Aspect Ratio**: 2:1 (e.g., 4096×2048, 2048×1024)
- **Max Size**: 4K (4096×2048) on desktop, 2K (2048×1024) on mobile

---

## 📦 Installation

### Method 1: Direct Import (Recommended)
```bash
1. Download/clone this repository
2. Copy Assets/Video360/ folder to your Unity project
3. Unity will automatically import all components
4. Open Video360_Demo scene and press Play
```

### Method 2: Unity Package Manager
```bash
1. Open Unity Package Manager
2. Add package from git URL: [your-git-url]
3. Import samples if desired
```

### Verify Installation
After import, check the Unity Console for:
```
[Video360Summary] Component validation completed:
  ✅ Video360Config: Found
  ✅ VideoController: Found in scene
  ✅ PlayerLookController: Found in scene
  ✅ SimpleVideoServer: Found in scene (Editor only)
```

---

## 🌐 HTTP Server Guide

The built-in HTTP server enables seamless video streaming during development without manually configuring web servers.

### Server Overview
- **Purpose**: Local video streaming for Unity development
- **Port**: 8080 (configurable)
- **Protocol**: HTTP/1.1 with byte-range support
- **CORS**: Enabled for WebGL compatibility
- **Platform**: Editor-only (automatically excluded from builds)

### Setup Instructions

#### 1. Basic Setup
```csharp
// Add VideoServerManager prefab to your scene
1. Drag Assets/Video360/Prefabs/VideoServerManager.prefab into scene
2. Server automatically starts when scene plays
3. Access videos at: http://localhost:8080/filename.mp4
```

#### 2. Directory Structure
```
Assets/
├── StreamingAssets/
│   └── Videos/
│       ├── sample360.mp4      ← Your 360° videos go here
│       ├── test_video_4K.mp4
│       └── vr_experience.mp4
└── Video360/
    └── Server/
        └── SimpleVideoServer.cs
```

#### 3. Server Configuration
```csharp
// In VideoServerManager component:
public int port = 8080;                    // Server port
public string videoFolder = "StreamingAssets/Videos";  // Video directory
public bool enableDebugLogs = true;       // Console logging
```

### Server Features

#### Byte-Range Support
The server supports HTTP Range requests for efficient video streaming:
```http
GET /sample360.mp4 HTTP/1.1
Range: bytes=0-1023

HTTP/1.1 206 Partial Content
Content-Range: bytes 0-1023/2048000
Content-Length: 1024
```

#### CORS Headers
Automatically includes CORS headers for WebGL compatibility:
```http
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, HEAD, OPTIONS
Access-Control-Allow-Headers: Range
```

#### Directory Listing
Navigate to `http://localhost:8080/` to see available videos:
```html
<h1>Available Videos</h1>
<ul>
  <li><a href="/sample360.mp4">sample360.mp4</a></li>
  <li><a href="/test_video.mp4">test_video.mp4</a></li>
</ul>
```

### Server Management

#### Start/Stop Server
```csharp
// Access server component
SimpleVideoServer server = FindObjectOfType<SimpleVideoServer>();

// Check status
if (server.IsServerRunning)
{
    Debug.Log($"Server running at: {server.GetServerURL()}");
}

// Server automatically stops when exiting Play mode
```

#### Server Logs
Monitor server activity in Unity Console:
```
[VideoServer] Started at http://localhost:8080/
[VideoServer] Serving videos from: /path/to/project/Assets/StreamingAssets/Videos
[VideoServer] Request: GET /sample360.mp4
[VideoServer] Serving sample360.mp4: bytes 0-2047999/2048000
```

#### Port Configuration
Change server port if 8080 is in use:
```csharp
// In VideoServerManager inspector:
Port: 8081  // or any available port

// URL becomes: http://localhost:8081/
```

### Production Deployment

The HTTP server is **Editor-only** and automatically excluded from builds:

#### Desktop Builds
```csharp
// Use local VideoClip references instead of URLs
config.videoClip = yourVideoClip;  // Instead of URL
```

#### Mobile/WebGL Builds
```csharp
// Use external CDN or hosting service
config.videoURL = "https://your-cdn.com/videos/sample360.mp4";
```

---

## 📖 Usage Guide

### Basic Usage

#### 1. Scene Setup
```csharp
// Add core prefab to scene
1. Drag PF_360SphereRig.prefab into scene
2. Position at (0, 0, 0)
3. Prefab includes:
   - Main Camera with PlayerLookController
   - Sphere with video material
   - VideoPlayer with VideoController
```

#### 2. Configure Video Source
```csharp
// Select DefaultVideoConfig asset
Assets/Video360/Resources/Config/DefaultVideoConfig.asset

// Set video source (choose one):
Video URL: http://localhost:8080/sample360.mp4  // HTTP server
Video Clip: [Assign VideoClip asset]           // Local file
```

#### 3. Choose Render Mode
```csharp
// In Video360Config:
Render Mode: 
├── Sphere   → Renders on inverted sphere geometry (recommended)
└── Skybox   → Uses Unity's skybox system (VR-friendly)
```

### Advanced Configuration

#### Stereo Video Setup
```csharp
// For 3D/VR 360° videos:
Stereo Layout:
├── None        → Standard mono 360° video
├── SideBySide  → Left/right eye images horizontally
└── OverUnder   → Left/right eye images vertically

// Automatically selects appropriate skybox material
```

#### Custom Controls
```csharp
// PlayerLookController settings:
Mouse Sensitivity: 3.0      // Mouse look speed
Touch Sensitivity: 2.0      // Touch drag speed
Gyro Sensitivity: 1.0       // Device orientation response
Enable Gyroscope: false     // Mobile device orientation
Min/Max Pitch: ±85°         // Vertical look constraints
```

#### Performance Settings
```csharp
// Video360Config performance options:
Render Texture:
├── RT_360_2K   → 2048×1024 (mobile-friendly)
└── RT_360_4K   → 4096×2048 (desktop quality)

Auto Play: true             // Start immediately when ready
Is Looping: true           // Continuous playback
Volume: 1.0                // Audio level
```

### Runtime Control

#### Video Playback Control
```csharp
// Get VideoController reference
VideoController controller = FindObjectOfType<VideoController>();

// Playback control
controller.PlayVideo();     // Start playback
controller.PauseVideo();    // Pause playback
controller.StopVideo();     // Stop playback

// Status checking
bool isPlaying = controller.IsPlaying;
bool isPrepared = controller.IsPrepared;
double videoLength = controller.Length;
double currentTime = controller.Time;
```

#### Camera Control
```csharp
// Get PlayerLookController reference
PlayerLookController lookController = FindObjectOfType<PlayerLookController>();

// Manual rotation
lookController.SetRotation(45f, 30f);  // yaw, pitch in degrees
lookController.ResetRotation();        // Return to center

// Get current rotation
Vector2 rotation = lookController.GetCurrentRotation();
Debug.Log($"Looking at: {rotation.x}° yaw, {rotation.y}° pitch");

// Gyroscope calibration
lookController.CalibrateGyroscope();   // Recalibrate baseline
```

#### Configuration Changes
```csharp
// Load different video at runtime
Video360Config newConfig = Resources.Load<Video360Config>("Config/MyCustomConfig");
VideoController controller = FindObjectOfType<VideoController>();
controller.SetConfig(newConfig);
```

---

## ⚙️ Configuration

### Video360Config Asset

The central configuration asset located at `Assets/Video360/Resources/Config/DefaultVideoConfig.asset`:

```csharp
[CreateAssetMenu(fileName = "Video360Config", menuName = "Video360/Config")]
public class Video360Config : ScriptableObject
{
    [Header("Video Source")]
    public string videoURL = "";           // HTTP/HTTPS URL
    public VideoClip videoClip;           // Local VideoClip asset
    
    [Header("Render Settings")]
    public RenderMode renderMode = RenderMode.Sphere;
    public StereoLayout stereoLayout = StereoLayout.None;
    public RenderTexture renderTexture;   // Output texture
    
    [Header("Materials")]
    public Material sphereMaterial;       // For sphere rendering
    public Material skyboxMaterialMono;   // Mono skybox
    public Material skyboxMaterialSBS;    // Side-by-side skybox
    public Material skyboxMaterialOU;     // Over-under skybox
    
    [Header("Playback Settings")]
    public bool autoPlay = true;          // Auto-start playback
    public bool isLooping = true;         // Loop video
    [Range(0f, 1f)]
    public float volume = 1f;             // Audio volume
}
```

### Creating Custom Configurations

#### 1. Create New Config Asset
```csharp
// Right-click in Project window:
Create → Video360 → Config

// Configure for your specific video:
- Set videoURL or assign videoClip
- Choose appropriate render settings
- Assign materials and render texture
```

#### 2. Multiple Video Setup
```csharp
// Create configs for different videos:
Config_Underwater.asset    → Underwater 360° experience
Config_Architecture.asset → Architectural walkthrough  
Config_Concert.asset       → Concert footage

// Switch between them at runtime:
VideoController.SetConfig(newConfig);
```

#### 3. Platform-Specific Configs
```csharp
// Desktop config (high quality):
- renderTexture: RT_360_4K
- videoURL: http://localhost:8080/video_4K.mp4

// Mobile config (optimized):
- renderTexture: RT_360_2K  
- videoURL: https://cdn.example.com/video_2K.mp4
```

---

## 🖥️ Platform Support

### Desktop (Windows, macOS, Linux)

#### Features
- ✅ Full HTTP server support
- ✅ Mouse and keyboard controls
- ✅ High-resolution video (4K+)
- ✅ All render modes
- ✅ Debug tools and editor integration

#### Setup
```csharp
// Recommended settings:
Render Texture: RT_360_4K
Controls: Mouse + keyboard
Video Source: HTTP server (development) or VideoClip (production)
```

#### Performance Tips
```csharp
// For smooth 4K playback:
- Use SSD storage for video files
- Ensure GPU supports hardware video decoding
- Close unnecessary applications
- Use H.264 codec for best compatibility
```

### Mobile (iOS, Android)

#### Features
- ✅ Touch controls with drag gestures
- ✅ Optional gyroscope/accelerometer input
- ✅ Optimized 2K video support
- ✅ Battery-efficient rendering
- ⚠️ No HTTP server (Editor-only)

#### Setup
```csharp
// Mobile-optimized settings:
Render Texture: RT_360_2K           // Reduce resolution
Touch Sensitivity: 2.0              // Comfortable touch response
Enable Gyroscope: true              // Device orientation
Volume: 0.8                         // Battery consideration
```

#### Gyroscope Calibration
```csharp
// Add calibration UI button:
public void CalibrateGyroscope()
{
    PlayerLookController controller = FindObjectOfType<PlayerLookController>();
    controller.CalibrateGyroscope();
    
    // Show user feedback
    ShowMessage("Gyroscope calibrated! Hold device normally.");
}
```

### WebGL (Web Browsers)

#### Features
- ✅ Touch controls in mobile browsers
- ✅ Mouse controls on desktop
- ✅ Streaming video support
- ⚠️ No HTTP server (use external hosting)
- ⚠️ Video format limitations

#### Setup
```csharp
// WebGL configuration:
Video Source: External URL (CORS-enabled)
Format: MP4 with H.264 codec
Max Resolution: 2K (browser dependent)
```

#### CORS Configuration
Your external video server needs CORS headers:
```http
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, HEAD, OPTIONS
Access-Control-Allow-Headers: Range
```

#### WebGL-Specific Code
```csharp
#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL-specific optimizations
    config.renderTexture = RT_360_2K;  // Lower resolution
    config.volume = 0.7f;              // Prevent audio issues
#endif
```

### VR Platforms

#### Oculus/Meta Quest
```csharp
// VR-optimized settings:
Render Mode: Skybox              // Better for VR
Stereo Layout: SideBySide        // If source is 3D
Enable Gyroscope: false          // VR headset handles rotation
Render Texture: RT_360_2K        // Performance optimization
```

#### OpenXR Compatibility
```csharp
// Works with OpenXR-compatible headsets:
- HTC Vive
- Valve Index  
- Windows Mixed Reality
- Pico devices
```

---

## 🔧 Troubleshooting

### Common Issues

#### Black Video Screen
**Symptoms**: Video player shows black screen
```csharp
✅ Check render texture assignment:
   VideoController → config → renderTexture should not be null

✅ Verify video file path:
   File must exist in StreamingAssets/Videos/ folder

✅ Test with known working video:
   Try the generated test video first

✅ Check video format:
   MP4 with H.264 codec recommended
```

#### HTTP Server Not Starting
**Symptoms**: "Connection refused" or server unreachable
```csharp
✅ Check port availability:
   Another application might be using port 8080
   Change port in VideoServerManager component

✅ Verify StreamingAssets folder:
   Assets/StreamingAssets/Videos/ must exist
   Create folder if missing

✅ Check Unity Console for errors:
   Look for [VideoServer] messages

✅ Firewall/antivirus blocking:
   Allow Unity Editor through firewall
```

#### Controls Not Responding  
**Symptoms**: Mouse/touch input not working
```csharp
✅ Check PlayerLookController settings:
   Enable Mouse Look: true
   Enable Touch Look: true
   Sensitivity > 0

✅ Verify camera setup:
   PlayerLookController must be on camera GameObject
   No other scripts interfering with camera rotation

✅ Test input method:
   Mouse: Click and drag to rotate
   Touch: Single finger drag gesture
```

#### Poor Performance
**Symptoms**: Low framerate, stuttering video
```csharp
✅ Reduce render texture resolution:
   Use RT_360_2K instead of RT_360_4K on mobile

✅ Optimize video file:
   Lower bitrate, use H.264 codec
   Ensure video dimensions match render texture

✅ Close unnecessary applications:
   Free up RAM and GPU memory

✅ Check Unity Profiler:
   Identify performance bottlenecks
```

#### Audio Issues
**Symptoms**: No audio or audio sync problems
```csharp
✅ Check AudioSource component:
   Must be attached to VideoPlayer GameObject
   Volume > 0, not muted

✅ Verify audio output mode:
   VideoPlayer → Audio Output Mode: Audio Source
   Target Audio Sources: Assigned to AudioSource

✅ Test audio file:
   Play video in external player to verify audio track exists
```

### Platform-Specific Issues

#### WebGL Issues
```csharp
Problem: Video doesn't load in web browser
Solution: 
- Use external video hosting with CORS
- Check browser developer console for errors
- Test with different video codecs (H.264, VP8)

Problem: Poor performance in browser
Solution:
- Use 2K or lower resolution
- Enable browser hardware acceleration
- Close other browser tabs
```

#### Mobile Issues
```csharp
Problem: Gyroscope not working
Solution:
- Check device orientation permissions
- Calibrate gyroscope using CalibrateGyroscope()
- Some devices don't support gyroscope

Problem: Touch controls unresponsive
Solution:
- Increase Touch Sensitivity value
- Disable conflicting UI elements
- Test on different devices
```

#### Unity Editor Issues
```csharp
Problem: Scripts won't compile
Solution:
- Check Unity Console for specific errors
- Verify all script references are intact
- Reimport Video360 folder if needed

Problem: Prefabs appear broken
Solution:
- Check all GUID references are valid
- Reimport prefab files
- Recreate prefabs if necessary
```

### Debug Tools

#### Enable Debug Logging
```csharp
// In VideoController:
public bool enableDebugLogs = true;

// In SimpleVideoServer:
public bool enableDebugLogs = true;

// Console output:
[VideoController] Video preparation completed
[VideoServer] Request: GET /sample360.mp4
[PlayerLookController] Gyroscope initialized
```

#### Component Validation
```csharp
// Add Video360Summary to scene:
1. Create empty GameObject
2. Add Video360Summary component
3. Right-click component → "Validate Components"
4. Check Console for validation results
```

#### Manual Testing
```csharp
// Test video playback manually:
VideoController controller = FindObjectOfType<VideoController>();
controller.PlayVideo();  // Should start playback
Debug.Log($"Video length: {controller.Length} seconds");
Debug.Log($"Is playing: {controller.IsPlaying}");
```

---

## 📚 API Reference

### Video360Config

```csharp
public class Video360Config : ScriptableObject
{
    // Video source properties
    public string videoURL { get; set; }
    public VideoClip videoClip { get; set; }
    
    // Render configuration
    public RenderMode renderMode { get; set; }
    public StereoLayout stereoLayout { get; set; }
    public RenderTexture renderTexture { get; set; }
    
    // Material references
    public Material sphereMaterial { get; set; }
    public Material skyboxMaterialMono { get; set; }
    public Material skyboxMaterialSBS { get; set; }
    public Material skyboxMaterialOU { get; set; }
    
    // Playback settings
    public bool autoPlay { get; set; }
    public bool isLooping { get; set; }
    public float volume { get; set; }
    
    // Methods
    public bool HasValidSource();              // Check if video source configured
    public Material GetSkyboxMaterial();       // Get material for current stereo layout
}
```

### VideoController

```csharp
public class VideoController : MonoBehaviour
{
    // Configuration
    public Video360Config config { get; set; }
    public bool enableDebugLogs { get; set; }
    
    // Events
    public System.Action OnVideoReady;         // Video prepared and ready
    public System.Action OnVideoStarted;      // Playback started
    public System.Action<string> OnVideoError; // Error occurred
    
    // Methods
    public void SetConfig(Video360Config newConfig);  // Apply new configuration
    public void PlayVideo();                          // Start playback
    public void PauseVideo();                         // Pause playback
    public void StopVideo();                          // Stop playback
    
    // Properties
    public bool IsPlaying { get; }             // Current playback status
    public bool IsPrepared { get; }            // Video preparation status
    public double Length { get; }              // Video length in seconds
    public double Time { get; }                // Current playback time
}
```

### PlayerLookController

```csharp
public class PlayerLookController : MonoBehaviour
{
    // Sensitivity settings
    public float mouseSensitivity { get; set; }
    public float touchSensitivity { get; set; }
    public float gyroSensitivity { get; set; }
    
    // Look constraints
    public float minPitch { get; set; }        // Minimum vertical angle
    public float maxPitch { get; set; }        // Maximum vertical angle
    
    // Input options
    public bool enableMouseLook { get; set; }
    public bool enableTouchLook { get; set; }
    public bool enableGyroscope { get; set; }
    
    // Smoothing
    public bool enableSmoothing { get; set; }
    public float smoothingFactor { get; set; }
    
    // Methods
    public void ResetRotation();                       // Reset to center view
    public void SetRotation(float yaw, float pitch);  // Set specific rotation
    public Vector2 GetCurrentRotation();              // Get current yaw/pitch
    public void CalibrateGyroscope();                 // Recalibrate gyro baseline
}
```

### SimpleVideoServer (Editor Only)

```csharp
#if UNITY_EDITOR
public class SimpleVideoServer : MonoBehaviour
{
    // Configuration
    public int port { get; set; }             // Server port
    public string videoFolder { get; set; }   // Video directory path
    public bool enableDebugLogs { get; set; }
    
    // Properties
    public bool IsServerRunning { get; }      // Server status
    public string GetServerURL();             // Get server URL
    public string GetVideoPath();             // Get video directory path
    
    // Server automatically starts in Start() and stops in OnDestroy()
}
#endif
```

### Video360Tests

```csharp
public class Video360Tests : MonoBehaviour
{
    public TestResult[] testResults;          // Array of test results
    
    [ContextMenu("Run All Tests")]
    public void RunAllTests();                // Execute all validation tests
    
    // Individual test methods (examples)
    TestResult Test_Video360Config_HasValidSource_WithURL();
    TestResult Test_Video360Config_HasValidSource_WithoutSource();
    TestResult Test_Video360Config_GetSkyboxMaterial();
    TestResult Test_SimpleVideoServer_Properties();
}
```

---

## 🛠️ Development Tools

### Test Video Generator

Generate procedural 360° test videos for development and testing:

#### Access the Tool
```csharp
// Unity menu bar:
Video360 → Generate Test Video

// Or create editor window:
Window → General → Test Video Generator
```

#### Generator Options
```csharp
Video Width: 2048              // Horizontal resolution
Video Height: 1024             // Vertical resolution  
Duration (seconds): 10         // Video length
Output Filename: test360.mp4   // Output file name
Use FFmpeg: true               // Use FFmpeg or Unity textures
```

#### FFmpeg Integration
```csharp
// If FFmpeg is installed:
- Generates high-quality MP4 videos
- Uses testsrc2 filter for animated patterns
- Supports custom resolution and duration

// Without FFmpeg:
- Creates PNG frame sequence
- Provides batch scripts for conversion
- Manual FFmpeg command included
```

#### Generated Test Patterns
```csharp
// Test video includes:
- Animated color gradients based on spherical coordinates
- Grid lines for orientation reference  
- Time-based animation for motion testing
- Proper equirectangular projection mapping
```

### Validation Tests

Comprehensive testing system to verify component functionality:

#### Running Tests
```csharp
// Method 1: Context menu (recommended)
1. Find Video360Tests component in scene
2. Right-click component in Inspector
3. Select "Run All Tests" from context menu
4. Check Console for results

// Method 2: Script access
Video360Tests tester = FindObjectOfType<Video360Tests>();
tester.RunAllTests();
```

#### Test Categories
```csharp
Configuration Tests:
✅ Video360Config.HasValidSource() with URL
✅ Video360Config.HasValidSource() without source  
✅ Video360Config.GetSkyboxMaterial() for all layouts

Component Tests:
✅ VideoController initialization
✅ PlayerLookController rotation clamping
✅ SimpleVideoServer properties (Editor only)

Asset Tests:
✅ RenderTexture configuration
✅ Material shader assignments
✅ Prefab component validation
```

#### Custom Test Creation
```csharp
// Add custom tests to Video360Tests class:
TestResult Test_MyCustomFeature()
{
    try
    {
        // Your test logic here
        bool testPassed = /* test condition */;
        
        return new TestResult
        {
            testName = "My Custom Feature",
            passed = testPassed,
            message = testPassed ? "Success" : "Failed"
        };
    }
    catch (System.Exception e)
    {
        return new TestResult
        {
            testName = "My Custom Feature",
            passed = false,
            message = $"Exception: {e.Message}"
        };
    }
}
```

### Debug Logging System

Comprehensive logging for development and troubleshooting:

#### Enable Debug Logs
```csharp
// VideoController logging:
[SerializeField] private bool enableDebugLogs = true;

// SimpleVideoServer logging:
[SerializeField] private bool enableDebugLogs = true;

// Runtime control:
VideoController controller = FindObjectOfType<VideoController>();
controller.enableDebugLogs = true;  // Enable at runtime
```

#### Log Categories
```csharp
Video Playback Logs:
[VideoController] Initializing video player...
[VideoController] Setting video URL: http://localhost:8080/sample360.mp4
[VideoController] Video preparation completed
[VideoController] Starting video playback

HTTP Server Logs:
[VideoServer] Started at http://localhost:8080/
[VideoServer] Serving videos from: /path/to/videos
[VideoServer] Request: GET /sample360.mp4
[VideoServer] Serving sample360.mp4: bytes 0-2047999/2048000

Input Control Logs:
[PlayerLookController] Gyroscope initialized
[PlayerLookController] Gyroscope recalibrated  
[PlayerLookController] Mouse sensitivity: 3.0

Error Logs:
[VideoController] Video error: Cannot open file
[VideoServer] HTTP Listener error: Access denied
[PlayerLookController] Gyroscope not supported on this device
```

### Performance Monitoring

Tools for monitoring and optimizing performance:

#### Unity Profiler Integration
```csharp
// Key metrics to monitor:
- CPU: VideoPlayer update time
- GPU: Video texture upload time  
- Memory: RenderTexture memory usage
- Audio: AudioSource processing time

// Profile with different settings:
RT_360_2K vs RT_360_4K performance
Sphere vs Skybox render mode impact
Video codec performance comparison
```

#### Custom Performance Metrics
```csharp
// Add to VideoController for performance tracking:
public class VideoPerformanceMetrics
{
    public float averageFrameTime;
    public long videoMemoryUsage;
    public int droppedFrames;
    public float audioLatency;
    
    [ContextMenu("Log Performance Stats")]
    void LogPerformanceStats()
    {
        Debug.Log($"Avg Frame Time: {averageFrameTime:F2}ms");
        Debug.Log($"Video Memory: {videoMemoryUsage / 1024 / 1024}MB");
        Debug.Log($"Dropped Frames: {droppedFrames}");
    }
}
```

---

## ⚡ Performance Tips

### Video Optimization

#### Recommended Video Settings
```csharp
Desktop (High Quality):
- Resolution: 4096×2048 (4K)
- Codec: H.264 High Profile
- Bitrate: 20-50 Mbps
- Frame Rate: 30 FPS
- Audio: AAC 48kHz Stereo

Mobile (Optimized):
- Resolution: 2048×1024 (2K)  
- Codec: H.264 Main Profile
- Bitrate: 8-15 Mbps
- Frame Rate: 30 FPS
- Audio: AAC 48kHz Stereo

WebGL (Web Optimized):
- Resolution: 1920×960 or 2048×1024
- Codec: H.264 Baseline Profile
- Bitrate: 5-10 Mbps
- Frame Rate: 24-30 FPS
- Audio: AAC 44.1kHz Stereo
```

#### Video Encoding Tips
```bash
# FFmpeg command for optimal encoding:
ffmpeg -i input.mp4 \
  -c:v libx264 \
  -profile:v high \
  -level:v 4.0 \
  -pix_fmt yuv420p \
  -crf 20 \
  -maxrate 20M \
  -bufsize 40M \
  -c:a aac \
  -b:a 192k \
  -movflags +faststart \
  output.mp4

# For mobile optimization:
-crf 23 -maxrate 10M -bufsize 20M

# For WebGL optimization:  
-crf 25 -maxrate 8M -bufsize 16M
```

### Unity Optimization

#### Render Settings
```csharp
// Optimal render pipeline settings:
Quality Settings:
- Anti Aliasing: Disabled or 2x (mobile)
- Anisotropic Filtering: Per Texture
- Shadow Quality: Hard Shadows Only
- Texture Quality: Full Resolution (desktop)

Camera Settings:
- Clear Flags: Solid Color (black)
- Culling Mask: Video objects only
- Allow HDR: Disabled (unless required)
- Allow MSAA: Disabled (mobile)
```

#### Memory Management
```csharp
// Efficient memory usage:
public class VideoMemoryManager
{
    void OptimizeMemory()
    {
        // Use appropriate render texture size
        if (SystemInfo.systemMemorySize < 4096) // < 4GB RAM
        {
            config.renderTexture = RT_360_2K;
        }
        
        // Release unused textures
        Resources.UnloadUnusedAssets();
        
        // Force garbage collection if needed
        System.GC.Collect();
    }
}
```

#### Platform-Specific Optimizations
```csharp
#if UNITY_ANDROID
    // Android optimizations
    config.renderTexture = RT_360_2K;
    videoPlayer.skipOnDrop = true;
    QualitySettings.vSyncCount = 0; // Disable VSync on mobile
#elif UNITY_IOS
    // iOS optimizations  
    config.renderTexture = RT_360_2K;
    videoPlayer.skipOnDrop = false; // iOS handles this better
#elif UNITY_WEBGL
    // WebGL optimizations
    config.renderTexture = RT_360_2K;
    config.volume = 0.8f; // Prevent audio issues
#endif
```

### Network Optimization

#### HTTP Server Tuning
```csharp
// Optimize server performance:
public class ServerOptimizer
{
    void OptimizeServerSettings()
    {
        // Increase buffer size for large videos
        int bufferSize = 128 * 1024; // 128KB chunks
        
        // Enable compression for metadata requests
        response.Headers.Add("Accept-Encoding", "gzip, deflate");
        
        // Set appropriate cache headers
        response.Headers.Add("Cache-Control", "public, max-age=3600");
        
        // Optimize for video streaming
        response.Headers.Add("Accept-Ranges", "bytes");
    }
}
```

#### CDN Configuration
```csharp
// For production deployment:
Video CDN Settings:
- Enable HTTP/2 for better performance
- Use geographic distribution
- Enable byte-range request support
- Configure proper CORS headers
- Set appropriate cache TTL (1-24 hours)

Example CDN URLs:
- AWS CloudFront: https://d1234567890.cloudfront.net/videos/
- Google Cloud CDN: https://storage.googleapis.com/bucket-name/
- Azure CDN: https://cdnendpoint.azureedge.net/videos/
```

---

## ❓ FAQ

### General Questions

**Q: What video formats are supported?**
```
A: MP4 (H.264), WebM (VP8/VP9), and MOV are supported. 
   MP4 with H.264 codec is recommended for best compatibility.
   Video must be in equirectangular projection (2:1 aspect ratio).
```

**Q: Can I use this for VR applications?**
```
A: Yes! Set renderMode to "Skybox" for VR compatibility.
   The package works with OpenXR and platform-specific VR SDKs.
   Use stereo video formats (SideBySide/OverUnder) for 3D content.
```

**Q: Is the HTTP server secure for production?**
```
A: No, the HTTP server is designed for development only.
   It's automatically excluded from builds and should not be used in production.
   For production, use external CDNs or VideoClip assets.
```

### Technical Questions

**Q: Why is my 4K video running slowly?**
```
A: 4K video requires significant GPU memory and processing power.
   Try these solutions:
   1. Use RT_360_2K render texture instead of RT_360_4K
   2. Reduce video bitrate during encoding
   3. Ensure hardware video decoding is available
   4. Close other GPU-intensive applications
```

**Q: How do I add custom controls?**
```
A: Extend PlayerLookController or create a custom input handler:
   
   public class CustomInputHandler : MonoBehaviour
   {
       void Update()
       {
           if (Input.GetKey(KeyCode.W)) // Custom key binding
           {
               // Add your control logic here
           }
       }
   }
```

**Q: Can I play multiple 360° videos simultaneously?**
```
A: Yes, but each requires its own VideoController and RenderTexture.
   Performance will be significantly impacted with multiple 4K videos.
   Consider using lower resolutions or switching between videos instead.
```

**Q: How do I customize the sphere material?**
```
A: Create a custom material with Unlit/Texture shader:
   1. Create new Material
   2. Set Shader to "Unlit/Texture"
   3. Set Cull mode to "Front" (to see inside the sphere)
   4. Assign your custom material to sphereMaterial in Video360Config
```

### Platform-Specific Questions

**Q: WebGL build shows "video format not supported"**
```
A: WebGL has limited codec support. Try these solutions:
   1. Use H.264 Baseline profile instead of High profile
   2. Ensure video is properly encoded for web playback
   3. Test with different browsers (Chrome, Firefox, Safari)
   4. Use WebM format as fallback option
```

**Q: Mobile gyroscope controls feel wrong**
```
A: This is usually a calibration issue:
   1. Call CalibrateGyroscope() when the user is holding device normally
   2. Adjust gyroSensitivity (try values between 0.5-2.0)
   3. Some devices have inverted gyroscope axes - may need custom handling
```

**Q: Audio is out of sync with video**
```
A: Audio sync issues can have several causes:
   1. Check video file - test in external player first
   2. Ensure AudioSource.playOnAwake = false
   3. VideoPlayer should handle audio timing automatically
   4. Try different video codecs (H.264 generally works best)
```

### Development Questions

**Q: How do I debug video loading issues?**
```
A: Enable debug logging and check these areas:
   1. Set enableDebugLogs = true on VideoController
   2. Check Unity Console for [VideoController] messages
   3. Verify file path in StreamingAssets/Videos/
   4. Test URL directly in web browser
   5. Use Unity Profiler to check for performance bottlenecks
```

**Q: Can I integrate this with other Unity packages?**
```
A: Yes, the system is designed to be modular:
   - Video360Config is a ScriptableObject (easily integrated)
   - VideoController is a standard MonoBehaviour
   - PlayerLookController can be replaced with custom input systems
   - HTTP server is optional and can be disabled
```

**Q: How do I create custom video transitions?**
```
A: Implement custom transition logic:

   public class VideoTransition : MonoBehaviour
   {
       public void CrossfadeToNewVideo(Video360Config newConfig)
       {
           // Fade out current video
           StartCoroutine(FadeOutCurrentVideo());
           
           // Load new video
           VideoController controller = FindObjectOfType<VideoController>();
           controller.SetConfig(newConfig);
           
           // Fade in new video
           StartCoroutine(FadeInNewVideo());
       }
   }
```

---

## 📄 License

This Unity 360° Video Player package is provided for **educational and development purposes**. 

### Usage Rights
- ✅ Use in personal and commercial Unity projects
- ✅ Modify and customize for your needs
- ✅ Distribute as part of your Unity applications
- ✅ Use for learning and educational purposes

### Restrictions
- ❌ Do not redistribute as a standalone package
- ❌ Do not sell as a Unity Asset Store package
- ❌ Video content may have separate licensing requirements

### Attribution
Attribution is appreciated but not required. If you find this package useful, consider:
- ⭐ Starring the repository
- 🐛 Reporting issues and bugs
- 💡 Contributing improvements and features

### Third-Party Components
This package uses Unity's built-in video systems and may include:
- Unity VideoPlayer component (Unity Technologies)
- Unity Audio system (Unity Technologies)
- System.Net.HttpListener (.NET Framework)

---

## 🤝 Support & Contributing

### Getting Help

1. **Documentation**: Check this README and inline code comments
2. **Unity Console**: Enable debug logging for detailed error messages
3. **Unity Forums**: Search for Unity VideoPlayer and 360° video topics
4. **GitHub Issues**: Report bugs and request features

### Contributing

We welcome contributions! Here's how to help:

#### Reporting Bugs
```
1. Check existing GitHub issues first
2. Include Unity version and platform details
3. Provide sample video file if possible
4. Include console logs with debug logging enabled
5. Describe steps to reproduce the issue
```

#### Feature Requests
```
1. Describe the use case clearly
2. Explain how it would benefit other users
3. Consider implementation complexity
4. Provide mockups or examples if helpful
```

#### Code Contributions
```
1. Fork the repository
2. Create a feature branch: git checkout -b feature-name
3. Make your changes with clear comments
4. Test on multiple platforms if possible
5. Submit a pull request with detailed description
```

### Roadmap

Future enhancements being considered:
- 🎬 **Playlist Support**: Queue and transition between multiple videos
- 🔊 **Spatial Audio**: 3D positional audio for immersive experiences
- 🎨 **Custom Shaders**: Advanced visual effects and post-processing
- 📊 **Analytics**: Video engagement and viewing statistics
- 🌐 **Streaming Protocols**: RTMP, HLS, and DASH support
- 🎮 **VR Interaction**: Hand tracking and gaze-based controls

---

## 📞 Contact

For questions, suggestions, or collaboration opportunities:

- **GitHub**: [Repository Issues](https://github.com/your-repo/unity-360-video/issues)
- **Unity Forums**: Search for "Unity 360 Video Player"
- **Discord**: Join Unity development communities

---

**Happy 360° Video Development! 🎥✨**

*Built with ❤️ for the Unity community*