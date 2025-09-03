#!/bin/bash
# Unix shell script to start the 360° Video HTTP Server

echo "Starting 360° Video HTTP Server..."
echo
echo "Place your video files in the 'videos' directory"
echo "Server will be available at: http://localhost:8080"
echo "Press Ctrl+C to stop the server"
echo

python3 video_server.py 8080 videos