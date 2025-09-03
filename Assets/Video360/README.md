# Unity 360° Video Player

A complete Unity package for playing 360° equirectangular videos with mouse, touch, and gyroscope controls. Features a built-in HTTP server for local video streaming during development.

## Features

- ✅ **360° Video Playback** - Supports equirectangular video format
- ✅ **Multiple Input Methods** - Mouse, touch, and gyroscope controls
- ✅ **Render Modes** - Sphere and Skybox rendering
- ✅ **Stereo Support** - Mono, Side-by-Side, and Over-Under layouts  
- ✅ **HTTP Server** - Built-in local video server for development
- ✅ **Editor Tools** - Test video generator and validation tests
- ✅ **Cross-Platform** - Works on Desktop, Mobile, and WebGL

## Quick Start

### 1. Import Package
- Copy the `Video360` folder to your Unity project's `Assets` directory
- Unity will automatically import all scripts, prefabs, and assets

### 2. Open Demo Scene
- Navigate to `Assets/Video360/Scenes/`
- Open `Video360_Demo.unity`
- Press Play to see the demo in action

### 3. Add Your Video
- Place your 360° MP4 video file in `Assets/StreamingAssets/Videos/`
- Rename it to `sample360.mp4` or update the config file
- The HTTP server will automatically serve it at `http://localhost:8080/`

## Project Structure

```
Assets/Video360/
├── Prefabs/           # Ready-to-use prefabs
│   ├── PF_VideoPlayer.prefab
│   ├── PF_360SphereRig.prefab
│   └── VideoServerManager.prefab
├── Materials/         # Rendering materials
│   ├── Mat_360_Unlit.mat
│   ├── Mat_Panoramic_Skybox_Mono.mat
│   ├── Mat_Panoramic_Skybox_SBS.mat
│   └── Mat_Panoramic_Skybox_OU.mat
├── Scripts/           # Core functionality
│   ├── Video360Config.cs
│   ├── VideoController.cs
│   └── PlayerLookController.cs
├── Resources/Config/  # Configuration assets
│   └── DefaultVideoConfig.asset
├── RenderTextures/    # Video output textures
│   ├── RT_360_2K.renderTexture
│   └── RT_360_4K.renderTexture
├── Scenes/           # Demo scene
│   └── Video360_Demo.unity
├── Server/           # HTTP server (Editor-only)
│   └── SimpleVideoServer.cs
├── Editor/           # Editor tools
│   └── TestVideoGenerator.cs
├── Tests/            # Validation tests
│   └── Video360Tests.cs
└── README.md         # This file
```

## Usage Guide

### Basic Setup

1. **Add 360° Sphere Rig to Scene:**
   ```
   Drag PF_360SphereRig prefab into your scene
   ```

2. **Configure Video Source:**
   ```
   Select DefaultVideoConfig asset in Resources/Config/
   Set videoURL or assign videoClip
   Choose renderMode (Sphere/Skybox) and stereoLayout
   ```

3. **Optional: Add HTTP Server:**
   ```
   Drag VideoServerManager prefab to scene (Editor-only)
   Server automatically starts and serves videos from StreamingAssets/Videos/
   ```

### Advanced Configuration

#### Video360Config Settings
- **Video Source**: URL (streaming) or VideoClip (local file)
- **Render Mode**: 
  - `Sphere` - Renders on inverted sphere geometry
  - `Skybox` - Uses Unity's skybox system
- **Stereo Layout**:
  - `None` - Standard mono 360° video
  - `SideBySide` - Left/right eye images side-by-side
  - `OverUnder` - Left/right eye images top/bottom
- **Render Texture**: Output resolution (2K/4K available)

#### PlayerLookController Settings
- **Mouse/Touch Sensitivity**: Control responsiveness
- **Pitch Constraints**: Limit vertical rotation (-85° to +85°)
- **Gyroscope**: Enable device orientation control
- **Smoothing**: Smooth rotation transitions

### Video Requirements

#### Supported Formats
- **Container**: MP4, WebM, MOV
- **Codec**: H.264, VP8/VP9
- **Projection**: Equirectangular (360°)
- **Aspect Ratio**: 2:1 (e.g., 4096x2048, 2048x1024)

#### Performance Recommendations
- **Mobile**: Use 2K textures (2048x1024)
- **Desktop**: 4K textures supported (4096x2048)
- **WebGL**: Test thoroughly, may need lower resolutions

## Development Tools

### HTTP Server (Editor-Only)
```csharp
// Auto-starts when VideoServerManager prefab is in scene
// Serves files from StreamingAssets/Videos/
// Supports byte-range requests for smooth streaming
// CORS-enabled for WebGL testing
```

### Test Video Generator
```
Menu: Video360 > Generate Test Video
- Creates animated 360° test patterns
- Uses FFmpeg if available, or generates frame sequence
- Helpful for testing without sample videos
```

### Validation Tests
```
Run tests in Unity Test Runner
- Component initialization tests
- Configuration validation
- Input handling tests
- Server functionality tests (Editor-only)
```

## Platform-Specific Notes

### Desktop (Windows/Mac/Linux)
- Full feature support
- HTTP server available in Editor
- Mouse and keyboard controls
- High-resolution video support

### Mobile (iOS/Android)
- Touch controls enabled by default
- Optional gyroscope support
- Use 2K textures for better performance
- Test video streaming bandwidth

### WebGL
- HTTP server not available (use external CDN)
- Touch controls work in mobile browsers
- May need video format optimization
- Test CORS settings for remote videos

## Troubleshooting

### Black Video Screen
```
✅ Check RenderTexture is assigned to VideoController
✅ Verify video file path/URL is correct
✅ Ensure video format is supported (MP4 recommended)
✅ Check Console for VideoPlayer error messages
```

### HTTP Server Issues
```
✅ VideoServerManager prefab must be in scene
✅ Check port 8080 is not in use by other applications  
✅ Verify StreamingAssets/Videos folder exists
✅ Look for server status in Console logs
```

### Performance Issues
```
✅ Use RT_360_2K instead of RT_360_4K on mobile
✅ Optimize video bitrate and codec
✅ Disable unnecessary Unity features (lighting, shadows)
✅ Profile with Unity Profiler to identify bottlenecks
```

### Controls Not Working
```
✅ Check PlayerLookController is attached to camera
✅ Verify input settings (mouse/touch enabled)
✅ Test with different sensitivity values
✅ Ensure camera is not constrained by other scripts
```

## API Reference

### Video360Config
```csharp
public string videoURL;                    // Stream URL
public VideoClip videoClip;                // Local video file
public RenderMode renderMode;              // Sphere/Skybox
public StereoLayout stereoLayout;          // Mono/SBS/OU
public RenderTexture renderTexture;        // Output texture

public bool HasValidSource();              // Check if source configured
public Material GetSkyboxMaterial();       // Get appropriate skybox material
```

### VideoController
```csharp
public void SetConfig(Video360Config config);  // Apply configuration
public void PlayVideo();                       // Start playback
public void PauseVideo();                      // Pause playback  
public void StopVideo();                       // Stop playback

public bool IsPlaying { get; }                 // Playback status
public bool IsPrepared { get; }                // Preparation status
```

### PlayerLookController
```csharp
public void ResetRotation();                   // Reset to center
public void SetRotation(float yaw, float pitch); // Set specific rotation
public Vector2 GetCurrentRotation();           // Get current yaw/pitch
public void CalibrateGyroscope();             // Recalibrate gyro baseline
```

### SimpleVideoServer (Editor-Only)
```csharp
public bool IsServerRunning { get; }           // Server status
public string GetServerURL();                  // Get server URL
public string GetVideoPath();                  // Get video directory path
```

## Deployment

### Desktop Build
```
1. Remove VideoServerManager from scenes (Editor-only)
2. Configure Video360Config to use VideoClip instead of URL
3. Place videos in StreamingAssets for runtime access
4. Build normally - server components are automatically excluded
```

### Mobile Build
```
1. Test touch controls thoroughly
2. Use 2K render textures for performance
3. Consider gyroscope calibration UI
4. Test video format compatibility on target devices
```

### WebGL Build
```
1. Host videos on external server with CORS enabled
2. Update Video360Config URLs to absolute paths
3. Test video codec compatibility in target browsers
4. Consider video compression for faster loading
```

## License

This package is provided as-is for educational and development purposes. 
Video content and external dependencies may have their own licenses.

## Support

For issues and feature requests:
1. Check the Troubleshooting section above
2. Review Unity Console for error messages  
3. Test with provided demo scene and sample videos
4. Verify Unity version compatibility (2022.3+)

---

**Unity 360° Video Player Package**  
Compatible with Unity 2022.3 and higher  
Created for immersive video experiences