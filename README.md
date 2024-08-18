# Remotely upload media to Jellyfin
With this plugin, you can remotely set an upload directory and upload files to it, all without touching the server.
How to install:
1. Add `https://raw.githubusercontent.com/GrandguyJS/media-upload-plugin/main/manifest.json` as a repository in Jellyfin
2. Install latest RemoteUpload plugin (1.1.1)
3. Reboot Jellyfin Server

Who should use this plugin?
1. 🏖️ Everybody who forgot to upload media to the server and will not have access to it for a period of time
2. 🤷 Family members who don't know how to use smb/other file upload systems

# Reverse proxy setup
If you are using a reverse-proxy, make sure, that you can upload 28MB chunks, by specifying the max allowed request body size.
If you do not have access to your reverse proxy at this time, run `let chunk_size = {file_size in bytes}`
## NGINX
```
location / {  
        client_max_body_size 30M; # You will need to upload 28MB chunks
        proxy_pass http://{jellyfin}:{port};
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
```

# Known errors
- Downloads from URL do not stop until the download is completed or you restart the server
- If the API cannot resolve a filename, the downloaded file will get named `download.dat`
- After updating the upload directory, please refresh the Upload page

Thank you for using this plugin
