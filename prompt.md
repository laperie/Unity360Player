# Unity 360° Video Playback Implementation - Agent Instructions

## PRIORITY: Execute tasks in the exact order listed below

### TASK 1: Create Project Structure

```
Assets/Video360/
├── Prefabs/
├── Materials/
├── Scripts/
├── Resources/Config/
├── RenderTextures/
├── Scenes/
├── Server/          [NEW]
└── SampleVideos/    [NEW]
```

### TASK 2: Implement Core Scripts

#### 2.1 Create `Video360Config.cs`

```csharp
// Location: Assets/Video360/Scripts/Video360Config.cs
// Purpose: Central configuration ScriptableObject
// Requirements:
// - Support URL and VideoClip sources
// - RenderMode enum: Sphere, Skybox
// - StereoLayout enum: None, SideBySide, OverUnder
// - RenderTexture reference field
// - Material references for skybox modes
```

#### 2.2 Create `VideoController.cs`

```csharp
// Location: Assets/Video360/Scripts/VideoController.cs
// Requirements:
// - RequireComponent: VideoPlayer, AudioSource
// - Implement Prepare() -> Play() workflow
// - Handle prepareCompleted and errorReceived events
// - Properly unsubscribe events in OnDestroy()
// - Support both URL and VideoClip sources from config
```

#### 2.3 Create `PlayerLookController.cs`

```csharp
// Location: Assets/Video360/Scripts/PlayerLookController.cs
// Requirements:
// - Mouse input: GetAxis("Mouse X/Y")
// - Touch input: Touch.deltaPosition scaled by Screen.width
// - Gyroscope: Optional, with baseline calibration
// - Pitch clamping: [-85°, +85°]
// - Yaw: Unbounded 360°
```

### TASK 3: Create Unity Assets

#### 3.1 RenderTextures

- `RT_360_2K.renderTexture`: 2048x1024, Default format
- `RT_360_4K.renderTexture`: 4096x2048, Default format

#### 3.2 Materials

- `Mat_360_Unlit.mat`: Unlit/Texture shader, Cull Front
- `Mat_Panoramic_Skybox_Mono.mat`: Skybox/Panoramic, Layout: None
- `Mat_Panoramic_Skybox_SBS.mat`: Skybox/Panoramic, Layout: Side by Side
- `Mat_Panoramic_Skybox_OU.mat`: Skybox/Panoramic, Layout: Over Under

#### 3.3 Configuration Asset

- `DefaultVideoConfig.asset`: Instance of Video360Config ScriptableObject

### TASK 4: Create Prefabs

#### 4.1 `PF_VideoPlayer.prefab`

Components:

- VideoPlayer (playOnAwake: false, isLooping: true)
- AudioSource (playOnAwake: false)
- VideoController script

#### 4.2 `PF_360SphereRig.prefab`

Hierarchy:

```
360SphereRig
├── Main Camera (Position: 0,0,0)
│   └── PlayerLookController
├── Sphere (Scale: -1,1,1)
│   └── MeshRenderer with Mat_360_Unlit
└── VideoPlayerObject (instance of PF_VideoPlayer)
```

### TASK 5: HTTP Video Server Implementation

#### 5.1 Create `SimpleVideoServer.cs`

```csharp
// Location: Assets/Video360/Server/SimpleVideoServer.cs
// Editor-only HTTP server for testing
// Requirements:
// - Use HttpListener (System.Net)
// - Serve video files from StreamingAssets folder
// - Support byte-range requests for video streaming
// - CORS headers for Unity WebGL compatibility
// - Port: 8080 (configurable)
// - Auto-start in Editor play mode
```

#### 5.2 Server Implementation Details

```csharp
#if UNITY_EDITOR
using System;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

public class SimpleVideoServer : MonoBehaviour
{
    private HttpListener listener;
    private Thread serverThread;
    private bool isRunning;
    public int port = 8080;
    public string videoFolder = "StreamingAssets/Videos";

    void Start()
    {
        StartServer();
    }

    void StartServer()
    {
        listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();
        isRunning = true;

        serverThread = new Thread(ServerLoop);
        serverThread.Start();

        Debug.Log($"[VideoServer] Started at http://localhost:{port}/");
    }

    void ServerLoop()
    {
        while (isRunning)
        {
            try
            {
                var context = listener.GetContext();
                ProcessRequest(context);
            }
            catch (Exception e)
            {
                if (isRunning) Debug.LogError($"[VideoServer] {e.Message}");
            }
        }
    }

    void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // Add CORS headers
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Range");

        // Handle preflight
        if (request.HttpMethod == "OPTIONS")
        {
            response.StatusCode = 200;
            response.Close();
            return;
        }

        // Parse file request
        string fileName = request.Url.AbsolutePath.TrimStart('/');
        string filePath = Path.Combine(Application.dataPath, videoFolder, fileName);

        if (!File.Exists(filePath))
        {
            response.StatusCode = 404;
            response.Close();
            return;
        }

        // Handle byte-range requests
        var fileInfo = new FileInfo(filePath);
        long totalLength = fileInfo.Length;
        long startByte = 0;
        long endByte = totalLength - 1;

        string rangeHeader = request.Headers["Range"];
        if (!string.IsNullOrEmpty(rangeHeader))
        {
            var range = rangeHeader.Replace("bytes=", "").Split('-');
            startByte = long.Parse(range[0]);
            if (range.Length > 1 && !string.IsNullOrEmpty(range[1]))
                endByte = long.Parse(range[1]);

            response.StatusCode = 206; // Partial Content
            response.Headers.Add("Content-Range", $"bytes {startByte}-{endByte}/{totalLength}");
        }
        else
        {
            response.StatusCode = 200;
        }

        // Set content headers
        response.ContentType = "video/mp4";
        response.ContentLength64 = endByte - startByte + 1;
        response.Headers.Add("Accept-Ranges", "bytes");

        // Stream the video
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            fs.Seek(startByte, SeekOrigin.Begin);
            byte[] buffer = new byte[64 * 1024]; // 64KB chunks
            long bytesRemaining = endByte - startByte + 1;

            while (bytesRemaining > 0)
            {
                int bytesToRead = (int)Math.Min(buffer.Length, bytesRemaining);
                int bytesRead = fs.Read(buffer, 0, bytesToRead);

                response.OutputStream.Write(buffer, 0, bytesRead);
                bytesRemaining -= bytesRead;
            }
        }

        response.Close();
    }

    void OnDestroy()
    {
        StopServer();
    }

    void StopServer()
    {
        isRunning = false;
        listener?.Stop();
        serverThread?.Join(1000);
        Debug.Log("[VideoServer] Stopped");
    }
}
#endif
```

#### 5.3 Create `VideoServerManager.prefab`

- GameObject with SimpleVideoServer component
- Default port: 8080
- Auto-added to demo scene

### TASK 6: Sample Scene Setup

#### 6.1 Create `Video360_Demo.unity`

1. Add `PF_360SphereRig` to scene at (0,0,0)
2. Add `VideoServerManager` prefab
3. Configure `DefaultVideoConfig`:
   - URL: `http://localhost:8080/sample360.mp4`
   - RenderTexture: RT_360_2K
   - RenderMode: Sphere
4. Place sample video in `StreamingAssets/Videos/sample360.mp4`

### TASK 7: Create Test Video Generator

```csharp
// Location: Assets/Video360/Editor/TestVideoGenerator.cs
// Create a simple 360° test pattern video using FFmpeg
[MenuItem("Video360/Generate Test Video")]
static void GenerateTestVideo()
{
    string outputPath = "Assets/StreamingAssets/Videos/test360.mp4";
    // Use Unity's Texture2D to create equirectangular test pattern
    // Export as MP4 using FFmpeg command line
}
```

### TASK 8: Validation Tests

Create `Assets/Video360/Tests/Video360Tests.cs`:

```csharp
// Test cases:
// 1. Server starts and responds to requests
// 2. Video loads from local file
// 3. Video loads from HTTP URL
// 4. Camera rotation stays within bounds
// 5. RenderTexture properly assigned
// 6. Audio plays synchronized with video
```

### TASK 9: Documentation

Create `Assets/Video360/README.md`:

```markdown
# Unity 360° Video Player

## Quick Start

1. Import package to Unity 2022.3+
2. Open Scenes/Video360_Demo
3. Press Play
4. Video streams from local HTTP server at http://localhost:8080

## Using Your Own Videos

1. Place MP4 files in StreamingAssets/Videos/
2. Update DefaultVideoConfig with filename
3. Server automatically serves all videos in folder

## Deployment

- Desktop: Server component is Editor-only
- Mobile: Use direct file URLs or external CDN
- Configure in Video360Config asset

## Troubleshooting

- Black video: Check RenderTexture assignment
- No video: Check server is running (see Console)
- Poor performance: Use RT_360_2K on mobile
```

### EXECUTION NOTES FOR AGENT

1. **Create all files in order** - Dependencies exist between scripts
2. **Test after each major component** - Use Debug.Log liberally
3. **Unity-specific requirements**:
   - All ScriptableObjects need [CreateAssetMenu] attribute
   - Prefabs must be created through Unity Editor workflow
   - Materials need manual shader assignment
4. **Server component is Editor-only** - Use #if UNITY_EDITOR
5. **Validate each step** before proceeding to next

### SUCCESS METRICS

- [ ] All scripts compile without errors
- [ ] HTTP server starts and serves video
- [ ] Demo scene plays video immediately on Play
- [ ] Camera controls work on all platforms
- [ ] No memory leaks (events properly unsubscribed)
- [ ] Package is self-contained and portable
