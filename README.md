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

# Error messages
1. Make sure the client_max_body_size is configured to 30MB
2. Directory doesn't exist Error
3. No permission error: Jellyfin User/Group should be able to write files in directory

Thanks for using this plugin!


