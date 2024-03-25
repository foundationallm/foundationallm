# Environment Setup Steps

1. Pre-Provision: Pre-Deploy.ps1
   1. Acquires certificates from LetsEncrypt.  Can be skipped if you have your own certificates.
2. Provision: psakefile.ps1
   1. Deploys the bicep templates to Azure
3. Post-Provision:
   1. Acquire VPN connection information and configure the VPN client.  Can be skipped if you have your own VPN or a hybrid network using Express Route, etc.
   2. Run `Post-Provision.1.ps1` - Could this be pushed into a deployment script?  Thinking no because we dont have real certs, but thats why we did Pre-Deploy.ps1.  Adding certs not supported natively in bicep.
      1. Generate a host file
      2. Concat host file with existing host file on your system
      3. Can be skipped if you have working DNS that forwards requests to the correct Private DNS zones
   3. Run `Post-Provision.2.ps1` after connecting to VPN and updating host files.
      1. To put the certificates into the ops key vault
4. Deploy: Deploy.ps1
   1. Generates the configurations for each cluster
   2. Loads the config into app configuration
   3. Loads default system files
   4. Backend Cluster
      1. FllM namespace
         1. Create the FLLM namespace in the backend cluster
         2. Deploy the backend services to the cluster
      2. Gateway-system namespace
         1. Creates the gateway-system namespace
         2. Deploys the secret class provider to gateway-system
         3. Deploy ingress-nginx
         4. Deploy Ingress Configurations and External Services
   5. Frontend Cluster
      1. FllM namespace
         1. Create the FLLM namespace in the frontend cluster
         2. Deploy the frontend services to the cluster
      2. Gateway-system namespace
         1. Creates the gateway-system namespace
         2. Deploys the secret class provider to gateway-system
         3. Deploy ingress-nginx
         4. Deploy Ingress Configurations and External Services