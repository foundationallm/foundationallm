
$global:InstanceId = ""
$global:CoreAPIBaseUrl = ""
$global:ManagementAPIBaseUrl = ""

$global:CoreAPIAccessToken = ""
$global:ManagementAPIAccessToken = ""
$global:ACADynamicSessionsAccessToken = ""

function Test-JWTExpired {
    param(
        [string]$jwt
    )
    try {

        return $true

        if ($jwt -eq "") {
            return $true
        }
        $parts = $jwt.split(".")
        if ($parts.Length -ne 3) {
            Write-Warning "Invalid JWT format."
            return $true
        }
        $body = $parts[1]
        $decoded = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($body.Replace('-', '+').Replace('_', '/').PadRight(($body.Length / 4 * 4) + ($body.Length % 4 -eq 0 ? 0 : 4), '=')))
        $json = ConvertFrom-Json $decoded
        $expirationTime = [datetime]::UnixEpoch.AddSeconds($json.exp)
        $currentTime = [datetime]::UtcNow
        Write-Host "Current time: $currentTime | JWT expiration time: $expirationTime" -ForegroundColor Green
        return $expirationTime -lt $currentTime
    } catch {
        Write-Warning "Error decoding or parsing JWT: $($_.Exception.Message)"
        return $true
    }
}

function Get-ManagementAPIAccessToken {
    $accessToken = az account get-access-token `
        --scope "api://FoundationaLLM-Management/Data.Manage" `
        --output  tsv `
        --query accessToken
    return $accessToken
}

function Test-ManagementAPIAccessToken {
    if (Test-JWTExpired $global:ManagementAPIAccessToken) {
        $global:ManagementAPIAccessToken = Get-ManagementAPIAccessToken
    }
}

function Test-CoreAPIAccessToken {
    if (Test-JWTExpired $global:CoreAPIAccessToken) {
        $global:CoreAPIAccessToken = Get-CoreAPIAccessToken
    }
}

function Get-CoreAPIAccessToken {
    $accessToken = az account get-access-token `
        --scope "api://FoundationaLLM-Core/Data.Read" `
        --output  tsv `
        --query accessToken
    return $accessToken
}

function Test-CoreAPIAccessToken {
    if (Test-JWTExpired $global:CoreAPIAccessToken) {
        $global:CoreAPIAccessToken = Get-CoreAPIAccessToken
    }
}

function Get-ACADynamicSessionsAccessToken {
    $accessToken = az account get-access-token `
        --resource "https://dynamicsessions.io" `
        --output  tsv `
        --query accessToken
    return $accessToken
}

function Test-ACADynamicSessionsAccessToken {
    if (Test-JWTExpired $global:ACADynamicSessionsAccessToken) {
        $global:ACADynamicSessionsAccessToken = Get-ACADynamicSessionsAccessToken
    }
}

function Get-ManagementAPIBaseUri {
    return [System.Uri]::new([System.Uri]::new($ManagementAPIBaseUrl), $global:ManagementAPIInstanceRelativeUri)
}

function Get-CoreAPIBaseUri {
    return [System.Uri]::new([System.Uri]::new($CoreAPIBaseUrl), $global:CoreAPIInstanceRelativeUri)
}

function Get-ObjectId {
    param (
        [string]$Name,
        [string]$Type
    )

    return "/instances/$($global:InstanceId)/providers/$($Type)/$($Name)"
}

function Get-ResourceObjectIds {
    param (
        [array]$Resources
    )

    $resourceObjectIds = [ordered]@{}
    $Resources | ForEach-Object {
        $resource = $_
        $resourceObjectId = (Get-ObjectId -Name $resource.name -Type $resource.type)
        $resourceObjectIds[$resourceObjectId] = [ordered]@{}
        $resourceObjectIds[$resourceObjectId]["object_id"] = $resourceObjectId
        $resourceObjectIds[$resourceObjectId]["properties"] = [ordered]@{}
        if ($resource.Contains("role") ){
            $resourceObjectIds[$resourceObjectId]["properties"]["object_role"] = $resource.role
        }
        if ($resource.Contains("properties") ){
            $resource["properties"] | ForEach-Object {
                $propertyList = $_
                $currentDict = $resourceObjectIds[$resourceObjectId]["properties"]
                for ($i = 0; $i -lt $propertyList.Length - 2; $i++) {
                    if ($currentDict.Contains($propertyList[$i])) {
                        $currentDict = $currentDict[$propertyList[$i]]
                    } else {
                        $currentDict[$propertyList[$i].ToString()] = [ordered]@{}
                        $currentDict = $currentDict[$propertyList[$i]]
                    }
                }
                $currentDict[$propertyList[$i].ToString()] = $propertyList[$i + 1]
            }
        }
    }

    return $resourceObjectIds
}

function Invoke-ManagementAPI {
    param (
        [string]$Method,
        [string]$RelativeUri,
        [hashtable]$Body = $null,
        [hashtable]$Headers = $null,
        [hashtable]$Form = $null,
        [System.Net.Http.MultipartFormDataContent]$MultipartContent = $null,
        [switch]$BinaryOutput
    )

    Write-Host "Calling Management API:"

    Test-ManagementAPIAccessToken

    if ($Headers -eq $null) {
        $Headers = @{
            "Authorization" = "Bearer $($global:ManagementAPIAccessToken)"
            "Content-Type"  = "application/json"
        }
    } else {
        $Headers["Authorization"] = "Bearer $($global:ManagementAPIAccessToken)"
    }

    $baseUri = Get-ManagementAPIBaseUri

    $uri = "$($baseUri.AbsoluteUri)/$($RelativeUri)"

    Write-Host "$($Method) $($uri)"

    # Handle MultipartFormDataContent (in-memory multipart data)
    if ($MultipartContent) {
        $client = [System.Net.Http.HttpClient]::new()
        $client.DefaultRequestHeaders.Clear()
        $client.DefaultRequestHeaders.Add("Authorization", "Bearer $($global:ManagementAPIAccessToken)")

        $response = switch ($Method.ToUpper()) {
            "POST" { $client.PostAsync($uri, $MultipartContent).Result }
            "PUT" { $client.PutAsync($uri, $MultipartContent).Result }
            default { throw "Unsupported HTTP method for MultipartContent: $Method" }
        }

        $responseContent = $response.Content.ReadAsStringAsync().Result

        if (-not $response.IsSuccessStatusCode) {
            throw "API call failed with status $($response.StatusCode): $responseContent"
        }

        if ($BinaryOutput) {
            return $response
        }

        if ([string]::IsNullOrWhiteSpace($responseContent)) {
            return $null
        }

        return $responseContent | ConvertFrom-Json
    }

    if ($Form) {

        $Headers["Content-Type"] = "multipart/form-data;boundary=----WebKitFormBoundaryABCDEFGHIJKLMONOPQRSTUVWXYZ"
        if ($BinaryOutput) {
            return Invoke-WebRequest `
                -Method $Method `
                -Uri $uri `
                -Form $Form `
                -Headers $Headers
        } else {
            return Invoke-RestMethod `
                -Method $Method `
                -Uri $uri `
                -Form $Form `
                -Headers $Headers
        }
    }

    if ($BinaryOutput) {
        return Invoke-WebRequest `
            -Method $Method `
            -Uri $uri `
            -Body ($Body | ConvertTo-Json -Depth 20) `
            -Headers $Headers
    }

    return Invoke-RestMethod `
        -Method $Method `
        -Uri $uri `
        -Body ($Body | ConvertTo-Json -Depth 20) `
        -Headers $Headers
}

function Invoke-CoreAPI {
    param (
        [string]$Method,
        [string]$RelativeUri,
        [hashtable]$Body = $null,
        [hashtable]$Headers = $null,
        [string]$FilePath = $null,
        [string]$FileContentType = $null
    )

    Write-Host "Calling Core API:"

    Test-CoreAPIAccessToken

    if ($Headers -eq $null) {
        $Headers = @{
            "Authorization" = "Bearer $($global:CoreAPIAccessToken)"
            "Content-Type"  = "application/json"
        }
    } else {
        $Headers["Authorization"] = "Bearer $($global:CoreAPIAccessToken)"
    }

    $baseUri = Get-CoreAPIBaseUri

    $uri = "$($baseUri.AbsoluteUri)/$($RelativeUri)"

    Write-Host "$($Method) $($uri)"
    if ($FilePath) {

        $FileStream = [System.IO.File]::OpenRead($FilePath)
        $FileContent = New-Object System.Net.Http.StreamContent($FileStream)
        $FileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse($FileContentType)

        $Form = New-Object System.Net.Http.MultipartFormDataContent
        $Form.Add($FileContent, "file", [System.IO.Path]::GetFileName($FilePath))

        $Client = New-Object System.Net.Http.HttpClient
        $Client.DefaultRequestHeaders.Clear()
        foreach ($key in $Headers.Keys) {
            if ($key -ne 'Content-Type') {
                $Client.DefaultRequestHeaders.Add($key, $Headers[$key])
            }
        }

        $Response = $Client.PostAsync($uri, $Form).Result
        $ResponseContent = $Response.Content.ReadAsStringAsync().Result
        return $ResponseContent | ConvertFrom-Json
    }

    return Invoke-RestMethod `
        -Method $Method `
        -Uri  $uri `
        -Body ($Body | ConvertTo-Json -Depth 20) `
        -Headers $Headers
}

function Get-FileNameFromContentDisposition {
    param([string]$ContentDisposition)

    if ([string]::IsNullOrWhiteSpace($ContentDisposition)) { return $null }

    # Prefer RFC 5987 / 6266 filename*= (e.g. UTF-8''name%20here.pdf)
    if ($ContentDisposition -match '(?i)filename\*\s*=\s*(?<v>[^;]+)') {
        $v = $Matches.v.Trim()

        # Strip optional quotes around the whole value
        if ($v.StartsWith('"') -and $v.EndsWith('"')) { $v = $v.Trim('"') }

        # Split: charset'lang'value   (lang may be empty)
        $parts = $v -split "'", 3
        if ($parts.Count -eq 3) {
            $encoded = $parts[2]
            return [Uri]::UnescapeDataString($encoded)
        }

        # If it wasn't in charset'lang' format, still try decoding as-is
        return [Uri]::UnescapeDataString($v)
    }

    # Fallback to filename=
    if ($ContentDisposition -match '(?i)filename\s*=\s*(?<v>[^;]+)') {
        $v = $Matches.v.Trim()
        if ($v.StartsWith('"') -and $v.EndsWith('"')) { $v = $v.Trim('"') }
        return $v
    }

    return $null
}