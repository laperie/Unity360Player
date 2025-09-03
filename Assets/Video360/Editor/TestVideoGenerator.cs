#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

public class TestVideoGenerator : EditorWindow
{
    [MenuItem("Video360/Generate Test Video")]
    static void GenerateTestVideo()
    {
        var window = GetWindow<TestVideoGenerator>("Test Video Generator");
        window.Show();
    }

    private int videoWidth = 2048;
    private int videoHeight = 1024;
    private int videoDuration = 10;
    private string outputFileName = "test360.mp4";
    private bool useFFmpeg = true;

    void OnGUI()
    {
        EditorGUILayout.LabelField("360° Test Video Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        videoWidth = EditorGUILayout.IntField("Video Width", videoWidth);
        videoHeight = EditorGUILayout.IntField("Video Height", videoHeight);
        videoDuration = EditorGUILayout.IntField("Duration (seconds)", videoDuration);
        outputFileName = EditorGUILayout.TextField("Output Filename", outputFileName);
        useFFmpeg = EditorGUILayout.Toggle("Use FFmpeg (recommended)", useFFmpeg);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This will generate a test 360° equirectangular video pattern. " +
            "If FFmpeg is not available, it will create a texture sequence instead.", MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Test Video"))
        {
            if (useFFmpeg)
            {
                GenerateWithFFmpeg();
            }
            else
            {
                GenerateTextureSequence();
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Download Sample 360° Video"))
        {
            Application.OpenURL("https://sample-videos.com/zip/10/mp4/SampleVideo_360x240_1mb.mp4");
            EditorUtility.DisplayDialog("Sample Video", 
                "A browser window should open to download a sample 360° video. " +
                "Save it to Assets/StreamingAssets/Videos/ and rename to sample360.mp4", "OK");
        }
    }

    void GenerateWithFFmpeg()
    {
        string outputPath = Path.Combine(Application.dataPath, "StreamingAssets", "Videos", outputFileName);
        string outputDir = Path.GetDirectoryName(outputPath);
        
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // Create FFmpeg command for 360° test pattern
        string ffmpegArgs = $"-f lavfi -i \"testsrc2=size={videoWidth}x{videoHeight}:duration={videoDuration}:rate=30\" " +
                          "-vf \"hue=s=0.8,eq=brightness=0.1:contrast=1.2\" " +
                          "-c:v libx264 -pix_fmt yuv420p -preset fast " +
                          $"-y \"{outputPath}\"";

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    UnityEngine.Debug.Log($"[TestVideoGenerator] Successfully generated test video: {outputPath}");
                    EditorUtility.DisplayDialog("Success", 
                        $"Test video generated successfully!\nPath: {outputPath}", "OK");
                }
                else
                {
                    UnityEngine.Debug.LogError($"[TestVideoGenerator] FFmpeg error: {error}");
                    EditorUtility.DisplayDialog("FFmpeg Error", 
                        $"FFmpeg failed to generate video.\nError: {error}\n\nTry using texture sequence instead.", "OK");
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"[TestVideoGenerator] Exception: {e.Message}");
            EditorUtility.DisplayDialog("Error", 
                $"Could not run FFmpeg. Make sure FFmpeg is installed and in PATH.\n\n" +
                $"Error: {e.Message}\n\nTry using texture sequence instead.", "OK");
        }
    }

    void GenerateTextureSequence()
    {
        string outputDir = Path.Combine(Application.dataPath, "StreamingAssets", "Videos", "TestSequence");
        
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        int frameCount = videoDuration * 30; // 30 FPS
        
        EditorUtility.DisplayProgressBar("Generating Test Sequence", "Creating frames...", 0);

        try
        {
            for (int frame = 0; frame < frameCount; frame++)
            {
                float progress = (float)frame / frameCount;
                EditorUtility.DisplayProgressBar("Generating Test Sequence", 
                    $"Creating frame {frame + 1}/{frameCount}", progress);

                Texture2D frameTexture = GenerateEquirectangularFrame(frame, frameCount);
                
                byte[] pngData = frameTexture.EncodeToPNG();
                string framePath = Path.Combine(outputDir, $"frame_{frame:D6}.png");
                File.WriteAllBytes(framePath, pngData);

                DestroyImmediate(frameTexture);
            }

            // Create a simple batch file/shell script to convert to video
            string scriptPath = Path.Combine(outputDir, "convert_to_video.bat");
            string scriptContent = $"ffmpeg -framerate 30 -i frame_%06d.png -c:v libx264 -pix_fmt yuv420p -r 30 ../{outputFileName}";
            File.WriteAllText(scriptPath, scriptContent);

            // Also create shell script for Mac/Linux
            string shellScriptPath = Path.Combine(outputDir, "convert_to_video.sh");
            File.WriteAllText(shellScriptPath, "#!/bin/bash\n" + scriptContent);

            UnityEngine.Debug.Log($"[TestVideoGenerator] Generated {frameCount} frames in: {outputDir}");
            EditorUtility.DisplayDialog("Success", 
                $"Generated {frameCount} test frames!\n\n" +
                $"To convert to video, run convert_to_video.bat (Windows) or convert_to_video.sh (Mac/Linux) " +
                $"in the TestSequence folder.\n\nPath: {outputDir}", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    Texture2D GenerateEquirectangularFrame(int frameNumber, int totalFrames)
    {
        Texture2D texture = new Texture2D(videoWidth, videoHeight, TextureFormat.RGB24, false);
        Color[] pixels = new Color[videoWidth * videoHeight];

        float time = (float)frameNumber / totalFrames;
        
        for (int y = 0; y < videoHeight; y++)
        {
            for (int x = 0; x < videoWidth; x++)
            {
                // Convert pixel coordinates to spherical coordinates
                float u = (float)x / videoWidth;
                float v = (float)y / videoHeight;
                
                float longitude = (u - 0.5f) * 2f * Mathf.PI;
                float latitude = (0.5f - v) * Mathf.PI;

                // Create a test pattern based on spherical coordinates and time
                float pattern1 = Mathf.Sin(longitude * 4f + time * Mathf.PI * 2f) * 0.5f + 0.5f;
                float pattern2 = Mathf.Sin(latitude * 3f + time * Mathf.PI) * 0.5f + 0.5f;
                float pattern3 = Mathf.Sin((longitude + latitude) * 2f + time * Mathf.PI * 1.5f) * 0.5f + 0.5f;

                // Create color based on patterns
                Color pixelColor = new Color(
                    pattern1 * 0.8f + 0.2f,
                    pattern2 * 0.8f + 0.2f,
                    pattern3 * 0.8f + 0.2f,
                    1f
                );

                // Add some grid lines for reference
                float gridSpacing = 0.1f;
                if (Mathf.Abs(Mathf.Sin(longitude / gridSpacing)) > 0.95f || 
                    Mathf.Abs(Mathf.Sin(latitude / gridSpacing)) > 0.95f)
                {
                    pixelColor = Color.white;
                }

                pixels[y * videoWidth + x] = pixelColor;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
}
#endif