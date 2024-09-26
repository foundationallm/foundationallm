# Utility to run certbot in a container

## Synopsis

```bash
make && \
    docker run \
        -v ./config:/app/config \
        -it certbot-app \
        -baseDomain <your zone name> \
        -email <your email> \
        -subdomainPrefix <optional prefix for hostnames>
```

You must replace `<your zone name>` and `<your email>` with the appropriate values.  And optionally, you can replace `<optional prefix for hostnames>` with a prefix for the hostnames or leave this parameter off the list.

Example:

```bash
make && \
    docker run \
        -v ./config:/app/config \
        -it certbot-app \
        -baseDomain example.com \
        -email email@example.com \
        -subdomainPrefix test
```

## Before you begin

Create `certbot.ini` in the `config` directory with the following content:

```ini
dns_azure_use_cli_credentials = true

dns_azure_environment = "AzurePublicCloud"

dns_azure_zone1 = <your zone name>:<your zone resource id>
```

You must replace  `<your zone name>` and `<your zone resource id>` with the appropriate values.