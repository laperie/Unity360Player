#if UNITY_EDITOR
using System;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

public class SimpleVideoServer : MonoBehaviour
{
    [Header("Server Configuration")]
    [Tooltip("Port number for the HTTP server")]
    public int port = 8080;
    
    [Tooltip("Folder path relative to Assets for video files")]
    public string videoFolder = "StreamingAssets/Videos";
    
    [Header("Debug")]
    [Tooltip("Enable detailed logging")]
    public bool enableDebugLogs = true;

    private HttpListener listener;
    private Thread serverThread;
    private bool isRunning = false;
    private string fullVideoPath;

    void Start()
    {
        StartServer();
    }

    void StartServer()
    {
        if (isRunning)
        {
            LogDebug("Server is already running");
            return;
        }

        try
        {
            fullVideoPath = Path.Combine(Application.dataPath, videoFolder);
            
            if (!Directory.Exists(fullVideoPath))
            {
                Directory.CreateDirectory(fullVideoPath);
                LogDebug($"Created video directory: {fullVideoPath}");
            }

            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            
            isRunning = true;

            serverThread = new Thread(ServerLoop)
            {
                IsBackground = true
            };
            serverThread.Start();

            LogDebug($"Video server started at http://localhost:{port}/");
            LogDebug($"Serving videos from: {fullVideoPath}");
        }
        catch (Exception e)
        {
            LogDebug($"Failed to start server: {e.Message}", true);
        }
    }

    void ServerLoop()
    {
        while (isRunning && listener != null && listener.IsListening)
        {
            try
            {
                var context = listener.GetContext();
                ProcessRequest(context);
            }
            catch (HttpListenerException e)
            {
                if (isRunning)
                {
                    LogDebug($"HTTP Listener error: {e.Message}", true);
                }
            }
            catch (Exception e)
            {
                if (isRunning)
                {
                    LogDebug($"Server error: {e.Message}", true);
                }
            }
        }
    }

    void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            LogDebug($"Request: {request.HttpMethod} {request.Url.AbsolutePath}");

            // Add CORS headers for Unity WebGL compatibility
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Range, Content-Type");

            // Handle preflight requests
            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Close();
                return;
            }

            // Parse requested file
            string fileName = request.Url.AbsolutePath.TrimStart('/');
            
            if (string.IsNullOrEmpty(fileName))
            {
                ServeDirectoryListing(response);
                return;
            }

            string filePath = Path.Combine(fullVideoPath, fileName);

            if (!File.Exists(filePath))
            {
                LogDebug($"File not found: {filePath}");
                response.StatusCode = 404;
                byte[] notFoundData = System.Text.Encoding.UTF8.GetBytes("File not found");
                response.ContentLength64 = notFoundData.Length;
                response.OutputStream.Write(notFoundData, 0, notFoundData.Length);
                response.Close();
                return;
            }

            ServeVideoFile(filePath, request, response);
        }
        catch (Exception e)
        {
            LogDebug($"Error processing request: {e.Message}", true);
            
            try
            {
                response.StatusCode = 500;
                response.Close();
            }
            catch { }
        }
    }

    void ServeVideoFile(string filePath, HttpListenerRequest request, HttpListenerResponse response)
    {
        var fileInfo = new FileInfo(filePath);
        long totalLength = fileInfo.Length;
        long startByte = 0;
        long endByte = totalLength - 1;

        // Handle Range requests for video streaming
        string rangeHeader = request.Headers["Range"];
        if (!string.IsNullOrEmpty(rangeHeader))
        {
            LogDebug($"Range request: {rangeHeader}");
            
            try
            {
                var range = rangeHeader.Replace("bytes=", "").Split('-');
                if (range.Length > 0 && !string.IsNullOrEmpty(range[0]))
                {
                    startByte = long.Parse(range[0]);
                }
                if (range.Length > 1 && !string.IsNullOrEmpty(range[1]))
                {
                    endByte = long.Parse(range[1]);
                }

                response.StatusCode = 206; // Partial Content
                response.Headers.Add("Content-Range", $"bytes {startByte}-{endByte}/{totalLength}");
            }
            catch
            {
                // Invalid range, serve full file
                response.StatusCode = 200;
            }
        }
        else
        {
            response.StatusCode = 200;
        }

        // Set appropriate headers
        string contentType = GetContentType(Path.GetExtension(filePath));
        response.ContentType = contentType;
        response.ContentLength64 = endByte - startByte + 1;
        response.Headers.Add("Accept-Ranges", "bytes");
        response.Headers.Add("Cache-Control", "public, max-age=3600");

        LogDebug($"Serving {Path.GetFileName(filePath)}: bytes {startByte}-{endByte}/{totalLength}");

        // Stream the file data
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            fileStream.Seek(startByte, SeekOrigin.Begin);
            
            byte[] buffer = new byte[64 * 1024]; // 64KB chunks
            long bytesRemaining = endByte - startByte + 1;

            while (bytesRemaining > 0 && response.OutputStream.CanWrite)
            {
                int bytesToRead = (int)Math.Min(buffer.Length, bytesRemaining);
                int bytesRead = fileStream.Read(buffer, 0, bytesToRead);
                
                if (bytesRead == 0) break;

                response.OutputStream.Write(buffer, 0, bytesRead);
                bytesRemaining -= bytesRead;
            }
        }

        response.Close();
    }

    void ServeDirectoryListing(HttpListenerResponse response)
    {
        response.StatusCode = 200;
        response.ContentType = "text/html";

        string html = "<html><head><title>Video Server</title></head><body><h1>Available Videos</h1><ul>";
        
        if (Directory.Exists(fullVideoPath))
        {
            string[] files = Directory.GetFiles(fullVideoPath, "*.*");
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                html += $"<li><a href=\"/{fileName}\">{fileName}</a></li>";
            }
        }
        
        html += "</ul></body></html>";

        byte[] data = System.Text.Encoding.UTF8.GetBytes(html);
        response.ContentLength64 = data.Length;
        response.OutputStream.Write(data, 0, data.Length);
        response.Close();
    }

    string GetContentType(string extension)
    {
        switch (extension.ToLower())
        {
            case ".mp4":
                return "video/mp4";
            case ".webm":
                return "video/webm";
            case ".mov":
                return "video/quicktime";
            case ".avi":
                return "video/x-msvideo";
            case ".mkv":
                return "video/x-matroska";
            default:
                return "application/octet-stream";
        }
    }

    void StopServer()
    {
        if (!isRunning) return;

        LogDebug("Stopping video server...");
        
        isRunning = false;
        
        try
        {
            listener?.Stop();
            listener?.Close();
        }
        catch { }

        if (serverThread != null && serverThread.IsAlive)
        {
            if (!serverThread.Join(1000))
            {
                try
                {
                    serverThread.Abort();
                }
                catch { }
            }
        }

        LogDebug("Video server stopped");
    }

    void LogDebug(string message, bool isError = false)
    {
        if (!enableDebugLogs) return;

        string logMessage = $"[VideoServer] {message}";
        if (isError)
            Debug.LogError(logMessage);
        else
            Debug.Log(logMessage);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            StopServer();
        }
        else
        {
            StartServer();
        }
    }

    void OnDestroy()
    {
        StopServer();
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    // Public methods for external control
    public bool IsServerRunning => isRunning;
    public string GetServerURL() => $"http://localhost:{port}/";
    public string GetVideoPath() => fullVideoPath;
}
#endif