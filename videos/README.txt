360° Video Server Directory

Place your 360° video files (.mp4, .webm, .avi, .mov, .wmv) in this directory.

Example usage:
1. Start server: python video_server.py 8080 videos  
2. Place video file: sample360.mp4
3. Use URL in Unity: http://localhost:8080/sample360.mp4

Supported formats:
- MP4 (recommended)
- WebM  
- AVI
- MOV
- WMV

The Python server supports byte-range requests for efficient streaming.

Note: Video files are excluded from git commits to avoid large repository size.