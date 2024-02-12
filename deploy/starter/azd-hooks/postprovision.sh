#!/bin/bash

# Strip surrounding quotes from string [$1: variable name]
function strip_quotes() {
    local -n var="$1"
    [[ "${var}" == \"*\" || "${var}" == \'*\' ]] && var="${var:1:-1}"
}

echo "Loading azd .env file from current environment"

# Use the `get-values` azd command to retrieve environment variables from the `.env` file
while IFS='=' read -r key value; do
    value=$(echo "$value" | sed 's/^"//' | sed 's/"$//')
    export "$key=$value"
done <<EOF
$(azd env get-values) 
EOF

export VECTORIZATION_WORKER_CONFIG=`cat ./config/vectorization.json`

envsubst < ./config/appconfig.template.json > ./config/appconfig.json

jq -c '.[]' ./config/appconfig.json | while read i; do
    keyVault=`echo $i | jq '.keyVault'`
    featureFlag=`echo $i | jq '.featureFlag'`
    isJson=`echo $i | jq '.isJson'`
    key=`echo $i | jq -r '.key'`
    value=`echo $i | jq -r '.value'`

    if [ $keyVault == 'true' ]; then
        cmd="az appconfig kv set-keyvault --key $key --name $AZURE_APP_CONFIG_NAME --secret-identifier ${AZURE_KEY_VAULT_ENDPOINT}secrets/$value --yes"
    elif [ $featureFlag == 'true' ]; then
        cmd="az appconfig feature set --feature $value --key $key --name $AZURE_APP_CONFIG_NAME --yes"
    elif [ $isJson == 'true' ]; then
        cmd="az appconfig kv set --key $key --name $AZURE_APP_CONFIG_NAME  --content-type application/json --yes --value '$value'"
    else
        cmd="az appconfig kv set --key $key --name $AZURE_APP_CONFIG_NAME --value '$value' --yes"
    fi

    echo $cmd
    eval $cmd </dev/null
    sleep 2
done

az storage azcopy blob upload -c agents --account-name $AZURE_STORAGE_ACCOUNT_NAME -s "../common/data/agents/*" --recursive --only-show-errors
az storage azcopy blob upload -c data-sources --account-name $AZURE_STORAGE_ACCOUNT_NAME -s "../common/data/data-sources/*" --recursive --only-show-errors
az storage azcopy blob upload -c foundationallm-source --account-name $AZURE_STORAGE_ACCOUNT_NAME -s "../common/data/foundationallm-source/*" --recursive --only-show-errors
az storage azcopy blob upload -c prompts --account-name $AZURE_STORAGE_ACCOUNT_NAME -s "../common/data/prompts/*" --recursive --only-show-errors
az storage azcopy blob upload -c resource-provider --account-name $AZURE_STORAGE_ACCOUNT_NAME -s "../common/data/resource-provider/*" --recursive --only-show-errors
