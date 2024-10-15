email="dan@solliance.net"
baseDomain="foundationallm.ai"
subdomainPrefix="internal"
make && \
    docker run \
        -v ./config:/app/config \
        -it certbot-app \
        -baseDomain $baseDomain \
        -email $email \
        -subdomainPrefix $subdomainPrefix