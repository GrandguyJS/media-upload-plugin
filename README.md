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
Your reverse-proxy should allow requests with a body size of around `30MB`, as the client will upload `28MB file chunks` at a time. You can set the `body_size` with the configuration below. If you do not have access to your reverse_proxy, you can set the `chunk_size` to be below 1MB (this is the default NGINX body size limit) by running `let chunk_size = 900000` (0.9MB) in the browser console. But make sure that your server can handle multiple requests a second.
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
- If the API cannot resolve a filename, the downloaded file will get named `download.dat`
- After updating the upload directory, please refresh the Upload page
- Make sure the server will have some downloaded data after 5 seconds, when inputing the download url

Thank you for using this plugin
