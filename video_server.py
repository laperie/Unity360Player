#!/usr/bin/env python3
"""
360° Video HTTP Server

A simple Python HTTP server for serving 360° video files to Unity applications.
Supports byte-range requests for efficient video streaming.

Usage:
    python video_server.py [port] [directory]

Default:
    - Port: 8080
    - Directory: ./videos (will be created if it doesn't exist)

Example:
    python video_server.py 8080 ./my_videos

License:
    MIT License - free for commercial and personal use.
    See LICENSE file for details.
"""

import os
import sys
import socket
import threading
from http.server import HTTPServer, SimpleHTTPRequestHandler
from urllib.parse import unquote
import mimetypes
import time


class VideoHTTPRequestHandler(SimpleHTTPRequestHandler):
    """HTTP request handler with enhanced video streaming support."""
    
    def __init__(self, *args, video_directory=None, **kwargs):
        self.video_directory = video_directory or "./videos"
        super().__init__(*args, **kwargs)

    def translate_path(self, path):
        """Translate URL path to local file path within video directory."""
        # Remove query parameters and decode URL
        path = path.split('?', 1)[0]
        path = path.split('#', 1)[0]
        path = unquote(path)
        
        # Remove leading slash and join with video directory
        path = path.lstrip('/')
        return os.path.join(self.video_directory, path)

    def guess_type(self, path):
        """Guess content type with video-specific handling."""
        mimetype, _ = mimetypes.guess_type(path)
        if mimetype is None:
            # Default video types
            ext = os.path.splitext(path)[1].lower()
            video_types = {
                '.mp4': 'video/mp4',
                '.webm': 'video/webm',
                '.avi': 'video/avi',
                '.mov': 'video/quicktime',
                '.wmv': 'video/x-ms-wmv'
            }
            mimetype = video_types.get(ext, 'application/octet-stream')
        return mimetype

    def do_GET(self):
        """Handle GET requests with byte-range support for video streaming."""
        file_path = self.translate_path(self.path)
        
        # Security check - ensure path is within video directory
        try:
            real_path = os.path.realpath(file_path)
            real_video_dir = os.path.realpath(self.video_directory)
            if not real_path.startswith(real_video_dir):
                self.send_error(403, "Access denied")
                return
        except (OSError, ValueError):
            self.send_error(404, "File not found")
            return

        if not os.path.exists(file_path):
            self.send_error(404, "File not found")
            return

        if os.path.isdir(file_path):
            # List directory contents
            self.list_directory(file_path)
            return

        try:
            file_size = os.path.getsize(file_path)
            range_header = self.headers.get('Range')
            
            if range_header:
                # Handle byte-range request
                self.handle_range_request(file_path, file_size, range_header)
            else:
                # Handle normal request
                self.handle_full_request(file_path, file_size)
                
        except (OSError, IOError) as e:
            try:
                self.send_error(500, f"Server error: {str(e)}")
            except (BrokenPipeError, ConnectionResetError):
                pass  # Client already disconnected

    def handle_range_request(self, file_path, file_size, range_header):
        """Handle HTTP byte-range requests for efficient video streaming."""
        try:
            # Parse Range header (e.g., "bytes=0-1024")
            range_match = range_header.replace('bytes=', '').split('-')
            start = int(range_match[0]) if range_match[0] else 0
            end = int(range_match[1]) if range_match[1] else file_size - 1
            
            # Validate range
            if start >= file_size or end >= file_size or start > end:
                self.send_error(416, "Range not satisfiable")
                return

            content_length = end - start + 1
            
            # Send headers
            self.send_response(206, "Partial Content")
            self.send_header("Content-Type", self.guess_type(file_path))
            self.send_header("Content-Length", str(content_length))
            self.send_header("Content-Range", f"bytes {start}-{end}/{file_size}")
            self.send_header("Accept-Ranges", "bytes")
            self.send_header("Access-Control-Allow-Origin", "*")
            self.end_headers()

            # Send file content with connection error handling
            try:
                with open(file_path, 'rb') as f:
                    f.seek(start)
                    remaining = content_length
                    while remaining > 0:
                        chunk_size = min(8192, remaining)
                        chunk = f.read(chunk_size)
                        if not chunk:
                            break
                        try:
                            self.wfile.write(chunk)
                            self.wfile.flush()  # Ensure data is sent immediately
                        except (BrokenPipeError, ConnectionResetError):
                            # Client disconnected - this is normal for video streaming
                            break
                        remaining -= len(chunk)
            except (BrokenPipeError, ConnectionResetError):
                # Client disconnected before we could start sending - ignore
                pass

        except ValueError:
            try:
                self.send_error(400, "Invalid range header")
            except (BrokenPipeError, ConnectionResetError):
                pass  # Client already disconnected
        except (OSError, IOError) as e:
            try:
                self.send_error(500, f"Server error: {str(e)}")
            except (BrokenPipeError, ConnectionResetError):
                pass  # Client already disconnected

    def handle_full_request(self, file_path, file_size):
        """Handle full file requests."""
        try:
            self.send_response(200)
            self.send_header("Content-Type", self.guess_type(file_path))
            self.send_header("Content-Length", str(file_size))
            self.send_header("Accept-Ranges", "bytes")
            self.send_header("Access-Control-Allow-Origin", "*")
            self.end_headers()

            try:
                with open(file_path, 'rb') as f:
                    while True:
                        chunk = f.read(8192)
                        if not chunk:
                            break
                        try:
                            self.wfile.write(chunk)
                            self.wfile.flush()  # Ensure data is sent immediately
                        except (BrokenPipeError, ConnectionResetError):
                            # Client disconnected - this is normal
                            break
            except (BrokenPipeError, ConnectionResetError):
                # Client disconnected before we could start sending
                pass

        except (OSError, IOError) as e:
            try:
                self.send_error(500, f"Server error: {str(e)}")
            except (BrokenPipeError, ConnectionResetError):
                pass  # Client already disconnected

    def list_directory(self, path):
        """List directory contents with video file highlighting."""
        try:
            file_list = os.listdir(path)
            file_list.sort()
        except OSError:
            self.send_error(404, "No permission to list directory")
            return

        self.send_response(200)
        self.send_header("Content-Type", "text/html; charset=utf-8")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()

        # Generate HTML directory listing
        html = f"""<!DOCTYPE html>
<html>
<head>
    <title>360° Video Server - Directory Listing</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .video {{ color: #0066cc; font-weight: bold; }}
        .directory {{ color: #666; }}
        a {{ text-decoration: none; margin: 5px 0; display: block; }}
        a:hover {{ text-decoration: underline; }}
    </style>
</head>
<body>
    <h1>360° Video Server</h1>
    <h2>Directory: {path}</h2>
    <hr>
"""

        # Add parent directory link
        if path != self.video_directory:
            html += '<a href="../" class="directory">📁 .. (Parent Directory)</a>\n'

        # List files and directories
        for name in file_list:
            full_path = os.path.join(path, name)
            if os.path.isdir(full_path):
                html += f'<a href="{name}/" class="directory">📁 {name}/</a>\n'
            else:
                ext = os.path.splitext(name)[1].lower()
                if ext in ['.mp4', '.webm', '.avi', '.mov', '.wmv']:
                    html += f'<a href="{name}" class="video">🎥 {name}</a>\n'
                else:
                    html += f'<a href="{name}">📄 {name}</a>\n'

        html += """
    <hr>
    <p><em>360° Video HTTP Server - Ready for Unity streaming</em></p>
</body>
</html>
"""
        
        self.wfile.write(html.encode('utf-8'))

    def log_message(self, format, *args):
        """Log requests with timestamp."""
        try:
            timestamp = time.strftime("%Y-%m-%d %H:%M:%S")
            print(f"[{timestamp}] {format % args}")
        except (BrokenPipeError, ConnectionResetError):
            # Don't log if client disconnected
            pass
    
    def log_error(self, format, *args):
        """Override to suppress broken pipe errors from logs."""
        if args and "Broken pipe" in str(args[0]):
            # Suppress broken pipe errors - these are normal for video streaming
            return
        super().log_error(format, *args)


def create_video_directory(directory):
    """Create video directory if it doesn't exist."""
    if not os.path.exists(directory):
        os.makedirs(directory)
        print(f"Created video directory: {directory}")
        
        # Create a sample info file
        info_file = os.path.join(directory, "README.txt")
        with open(info_file, 'w') as f:
            f.write("""360° Video Server Directory

Place your 360° video files (.mp4, .webm, .avi, .mov, .wmv) in this directory.

Example usage in Unity:
1. Start this server: python video_server.py
2. Place video file: sample360.mp4
3. Use URL in Unity: http://localhost:8080/sample360.mp4

Supported formats:
- MP4 (recommended)
- WebM
- AVI
- MOV
- WMV

The server supports byte-range requests for efficient streaming.
""")


def find_free_port(preferred_port):
    """Find a free port starting from preferred port."""
    port = preferred_port
    while port < preferred_port + 100:
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
                s.bind(('', port))
                return port
        except OSError:
            port += 1
    raise OSError(f"No free ports found starting from {preferred_port}")


def main():
    """Main server function."""
    # Parse command line arguments
    port = 8080
    video_directory = "./videos"
    
    if len(sys.argv) > 1:
        try:
            port = int(sys.argv[1])
        except ValueError:
            print("Error: Port must be a number")
            sys.exit(1)
    
    if len(sys.argv) > 2:
        video_directory = sys.argv[2]

    # Create video directory
    create_video_directory(video_directory)
    
    # Find available port
    try:
        actual_port = find_free_port(port)
        if actual_port != port:
            print(f"Port {port} is busy, using port {actual_port} instead")
            port = actual_port
    except OSError as e:
        print(f"Error finding free port: {e}")
        sys.exit(1)

    # Create request handler with video directory
    def handler(*args, **kwargs):
        VideoHTTPRequestHandler(*args, video_directory=video_directory, **kwargs)

    # Start server
    try:
        server = HTTPServer(('', port), handler)
        print(f"""
╔══════════════════════════════════════════════════════════════════════════════╗
║                           360° Video HTTP Server                             ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  Server URL: http://localhost:{port:<8}                                      ║
║  Directory:  {video_directory:<60} ║
║  Status:     Running... (Press Ctrl+C to stop)                              ║
║                                                                              ║
║  Unity Setup:                                                                ║
║  1. Place video files in: {video_directory:<48} ║
║  2. Use URL format: http://localhost:{port}/filename.mp4                     ║
║  3. Configure in Unity Video360Config asset                                 ║
╚══════════════════════════════════════════════════════════════════════════════╝
""")
        
        server.serve_forever()
        
    except KeyboardInterrupt:
        print("\n\nShutting down server...")
        server.shutdown()
        print("Server stopped.")
        
    except Exception as e:
        print(f"Server error: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()