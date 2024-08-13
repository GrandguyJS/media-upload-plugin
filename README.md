# Remotely upload media to Jellyfin
With this plugin, you can remotely set an upload directory and upload files to it, all without touching the server.
How to install:
1. Add `https://raw.githubusercontent.com/GrandguyJS/media-upload-plugin/main/manifest.json` as a repository in jellyfin.
2. Install latest RemoteUpload plugin
3. Reboot Jellyfin Server

Who should use this plugin?
1. Everybody who forgot to upload media to the server and will not have access to it for a period of time.
2. Family members who don't know how to use smb/other file upload systems

Before you continue:
All files get split in 28MB chunks and get uploaded to the server seperately, because Jellyfin server has a limit of 30MB per request.

Thanks for using!
