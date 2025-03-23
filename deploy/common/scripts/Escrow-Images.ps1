Install-Module -Name powershell-yaml -Force
Import-Module powershell-yaml

function Expand-Tar($tarFile, $dest) {

    if (-not (Get-Command Expand-7Zip -ErrorAction Ignore)) {
        Install-Package -Scope CurrentUser -Force 7Zip4PowerShell > $null
    }

    Expand-7Zip $tarFile $dest
}

$services = @{
    "version" = "0.9.2"
    "sourceRegistry" = "ghcr.io/solliancenet/foundationallm"
    "destRegistry" = "crsyn092.azurecr.io"
    "destUsername" = "crsyn092"
    "destPassword" = "<ACR password>"
    "service_matrix" = @(
        "agent-hub-api"
        "authorization-api"
        "chat-ui"
        "core-api"
        "core-job"
        "data-source-hub-api"
        "gatekeeper-api"
        "gatekeeper-integration-api"
        "gateway-api"
        "gateway-adapter-api"
        "langchain-api"
        "management-api"
        "management-ui"
        "orchestration-api"
        "prompt-hub-api"
        "semantic-kernel-api"
        "state-api"
        "vectorization-api"
        "vectorization-job"
        "context-api"
        "datapipeline-api"
        "datapipeline-frontendworker"
        "datapipeline-backendworker"
    )
}

foreach ($service in $($services.service_matrix)) {
    $srcChartUrl = "$($services.sourceRegistry)/helm/$service"
    $destChartUrl = "$($services.destRegistry)/helm"
    docker logout
    helm pull oci://$srcChartUrl --version $services.version
    echo $services.destPassword | docker login $services.destRegistry -u $services.destUsername --password-stdin
    helm push "$service-$($services.version).tgz" oci://$destChartUrl

    $imageWithTag = "$($service):$($services.version)"
    $srcImageUrl = "$($services.sourceRegistry)/$imageWithTag"
    $destImageUrl = "$($services.destRegistry)/$imageWithTag"
    docker logout
    docker pull $srcImageUrl
    docker tag $srcImageUrl $destImageUrl
    echo $services.destPassword | docker login $services.destRegistry -u $services.destUsername --password-stdin
    docker push $destImageUrl
}

$ingressChartRepo = "https://kubernetes.github.io/ingress-nginx"
$ingressChart = "ingress-nginx/ingress-nginx"
$ingressChartVersion = "4.10.0"
$destChartUrl = "$($services.destRegistry)/helm"
helm repo add ingress-nginx $ingressChartRepo
helm repo update
Remove-Item -Path ingress-nginx -Recurse
helm pull $ingressChart --version $ingressChartVersion --untar

$ingressValues = Get-Content "ingress-nginx/values.yaml" | ConvertFrom-Yaml
$ingressImages = @{
    "controller" = @{
        image = $ingressValues.controller.image.image
        registry = $ingressValues.controller.image.registry
        tag = $ingressValues.controller.image.tag
        suffix = "@" + $ingressValues.controller.image.digestChroot
    }
    "opentelemetry" = @{
        image = $ingressValues.controller.opentelemetry.image.image
        registry = $ingressValues.controller.opentelemetry.image.registry
        tag = $ingressValues.controller.opentelemetry.image.tag
        suffix = "@" + $ingressValues.controller.opentelemetry.image.digest
    }
    "certgen" = @{
        image = $ingressValues.controller.admissionWebhooks.patch.image.image
        registry = $ingressValues.controller.admissionWebhooks.patch.image.registry
        tag = $ingressValues.controller.admissionWebhooks.patch.image.tag
        suffix = "@" + $ingressValues.controller.admissionWebhooks.patch.image.digest
    }
    "defaultbackend" = @{
        image = $ingressValues.defaultBackend.image.image
        registry = $ingressValues.defaultBackend.image.registry
        tag = $ingressValues.defaultBackend.image.tag
        suffix = ""
    }
}

$ingressValues.controller.image.registry = $services.destRegistry
$ingressValues.controller.opentelemetry.image.registry = $services.destRegistry
$ingressValues.controller.admissionWebhooks.patch.image.registry = $services.destRegistry
$ingressValues.defaultBackend.image.registry = $services.destRegistry

$ingressValues.controller.image.Remove('digest')
$ingressValues.controller.image.Remove('digestChroot')
$ingressValues.controller.opentelemetry.image.Remove('digest')
$ingressValues.controller.admissionWebhooks.patch.image.Remove('digest')

Out-File -FilePath "ingress-nginx/values.yaml" -InputObject ($ingressValues | ConvertTo-Yaml) -Encoding utf8

foreach ($image in $ingressImages.GetEnumerator()) {
    $srcImageUrl = $image.Value.registry + "/" + $image.Value.image + ":" + $image.Value.tag + $image.Value.suffix
    $destImageUrl = "$($services.destRegistry)/$($image.Value.image):$($image.Value.tag)"
    Write-Host "Escrowing image $srcImageUrl to $destImageUrl" -ForegroundColor Green
    docker logout
    docker pull $srcImageUrl
    docker tag $srcImageUrl $destImageUrl
    echo $services.destPassword | docker login $services.destRegistry -u $services.destUsername --password-stdin
    docker push $destImageUrl
}

helm package ingress-nginx
echo $services.destPassword | docker login $services.destRegistry -u $services.destUsername --password-stdin
helm push "ingress-nginx-$($ingressChartVersion).tgz" oci://$destChartUrl
