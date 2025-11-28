# PowerShell script to validate the test harness setup
# and check the local virtual environment

param(
    [switch]$SkipValidation,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# Colors for output
$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"
$Blue = "Blue"

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    if ($Color -eq "*") {
        Write-Host $Message
    } else {
        Write-Host $Message -ForegroundColor $Color
    }
}

function Test-Environment {
    Write-ColorOutput "🔍 Checking FoundationaLLM test environment..." $Blue
    
    # Check if we're in the right directory pattern
    $currentDir = Get-Location
    $expectedDirPattern = "*\evaluation\python\agent-test-harness"
    
    if ($currentDir.Path -notlike $expectedDirPattern) {
        Write-ColorOutput "⚠️  Warning: Directory path doesn't match expected pattern" $Yellow
        Write-ColorOutput "   Current: $($currentDir.Path)" $Yellow
        Write-ColorOutput "   Expected pattern: $expectedDirPattern" $Yellow
    }
    
    # Check if local .venv exists
    $venvPath = ".\.venv"
    if (-not (Test-Path $venvPath)) {
        Write-ColorOutput "⚠️  Warning: Local .venv not found at: $venvPath" $Yellow
        Write-ColorOutput "   Consider running: .\create_venv.ps1" $Yellow
    }
    
    # Check if we're in a virtual environment
    $currentVenv = $env:VIRTUAL_ENV
    if ($currentVenv) {
        $expectedVenvPattern = "*\evaluation\python\agent-test-harness\.venv"
        if ($currentVenv -notlike $expectedVenvPattern) {
            Write-ColorOutput "⚠️  Warning: Different virtual environment active" $Yellow
            Write-ColorOutput "   Current: $currentVenv" $Yellow
            Write-ColorOutput "   Expected pattern: $expectedVenvPattern" $Yellow
        } else {
            Write-ColorOutput "   ✓ Correct virtual environment active" $Green
        }
    } else {
        Write-ColorOutput "⚠️  Warning: No virtual environment detected" $Yellow
        Write-ColorOutput "   Consider running: .\.venv\Scripts\Activate.ps1" $Yellow
    }
    
    # Check required files
    $requiredFiles = @(
        "test_harness.py",
        "requirements.txt",
        "sample.env"
    )
    
    $missingFiles = @()
    foreach ($file in $requiredFiles) {
        if (-not (Test-Path $file)) {
            $missingFiles += $file
        }
    }
    
    if ($missingFiles.Count -gt 0) {
        Write-ColorOutput "❌ Missing required files:" $Red
        foreach ($file in $missingFiles) {
            Write-ColorOutput "   - $file" $Red
        }
        return $false
    }
    
    return $true
}

function Test-PythonEnvironment {
    Write-ColorOutput "🐍 Checking Python environment..." $Blue
    
    # Check Python version
    try {
        $pythonVersion = python --version 2>&1
        Write-ColorOutput "   Python version: $pythonVersion" $Green
        
        # Check if version is 3.10+
        $versionMatch = $pythonVersion -match "Python (\d+)\.(\d+)"
        if ($versionMatch) {
            $major = [int]$matches[1]
            $minor = [int]$matches[2]
            if ($major -lt 3 -or ($major -eq 3 -and $minor -lt 10)) {
                Write-ColorOutput "⚠️  Warning: Python 3.10+ recommended" $Yellow
            }
        }
    }
    catch {
        Write-ColorOutput "❌ Python not found or not accessible" $Red
        return $false
    }
    
    # Check if we're in a virtual environment
    if (-not $env:VIRTUAL_ENV) {
        Write-ColorOutput "⚠️  No virtual environment detected" $Yellow
        Write-ColorOutput "   Run: .\.venv\Scripts\Activate.ps1" $Yellow
        return $false
    }
    
    return $true
}

function Test-RequiredPackages {
    Write-ColorOutput "📦 Checking required packages..." $Blue
    
    $requiredPackages = @(
        @{name="requests"; import="requests"},
        @{name="pandas"; import="pandas"},
        @{name="python-dotenv"; import="dotenv"},
        @{name="tqdm"; import="tqdm"},
        @{name="openai"; import="openai"},
        @{name="jinja2"; import="jinja2"},
        @{name="PyJWT"; import="jwt"},
        @{name="azure-identity"; import="azure.identity"}
    )
    
    $missingPackages = @()
    
    foreach ($package in $requiredPackages) {
        try {
            $result = python -c "import $($package.import); print('OK')" 2>&1
            if ($LASTEXITCODE -eq 0 -and $result -match "OK") {
                Write-ColorOutput "   ✓ $($package.name)" $Green
            } else {
                $missingPackages += $package.name
                Write-ColorOutput "   ✗ $($package.name)" $Red
                if ($result) {
                    Write-ColorOutput "     Error: $($result -join ' ')" $Yellow
                }
            }
        }
        catch {
            $missingPackages += $package.name
            Write-ColorOutput "   ✗ $($package.name)" $Red
        }
    }
    
    if ($missingPackages.Count -gt 0) {
        Write-ColorOutput "❌ Missing packages: $($missingPackages -join ', ')" $Red
        Write-ColorOutput "   Run: pip install -r requirements.txt" $Red
        return $false
    }
    
    return $true
}

function Test-EnvironmentVariables {
    Write-ColorOutput "🔧 Checking environment variables..." $Blue
    
    # Load .env file if it exists
    $envFile = ".env"
    if (Test-Path $envFile) {
        Write-ColorOutput "   Loading .env file..." $Blue
        Get-Content $envFile | ForEach-Object {
            if ($_ -match "^([^#][^=]+)=(.*)$") {
                $key = $matches[1].Trim()
                $value = $matches[2].Trim()
                if ($value -and $key) {
                    [Environment]::SetEnvironmentVariable($key, $value, "Process")
                }
            }
        }
    }
    
    $requiredVars = @(
        "FLLM_ENDPOINT"
    )
    
    $optionalVars = @(
        "FLLM_ACCESS_TOKEN",
        "FLLM_MGMT_ENDPOINT",
        "FLLM_MGMT_BEARER_TOKEN",
        "AZURE_OPENAI_ENDPOINT",
        "AZURE_OPENAI_API_KEY",
        "AZURE_OPENAI_DEPLOYMENT"
    )
    
    $missingRequired = @()
    $missingOptional = @()
    
    foreach ($var in $requiredVars) {
        $envValue = [Environment]::GetEnvironmentVariable($var)
        if (-not $envValue) {
            $missingRequired += $var
            Write-ColorOutput "   ✗ $var (required)" $Red
        } else {
            Write-ColorOutput "   ✓ $var" $Green
        }
    }
    
    foreach ($var in $optionalVars) {
        $envValue = [Environment]::GetEnvironmentVariable($var)
        if (-not $envValue) {
            $missingOptional += $var
            Write-ColorOutput "   ⚠ $var (optional)" $Yellow
        } else {
            Write-ColorOutput "   ✓ $var" $Green
        }
    }
    
    if ($missingRequired.Count -gt 0) {
        Write-ColorOutput "❌ Missing required environment variables:" $Red
        foreach ($var in $missingRequired) {
            Write-ColorOutput "   - $var" $Red
        }
        Write-ColorOutput "   Please set these in your .env file" $Red
        return $false
    }
    
    if ($missingOptional.Count -gt 0) {
        Write-ColorOutput "⚠️  Missing optional environment variables:" $Yellow
        foreach ($var in $missingOptional) {
            Write-ColorOutput "   - $var" $Yellow
        }
        Write-ColorOutput "   Some features may not work without these" $Yellow
        Write-ColorOutput "   When FLLM_ACCESS_TOKEN is not set, ensure you've run 'az login' and have appropriate permissions" $Yellow
        Write-ColorOutput "   When FLLM_MGMT_BEARER_TOKEN is not set, ensure you've run 'az login' and have appropriate permissions" $Yellow
    }
    
    return $true
}

function Install-MissingPackages {
    Write-ColorOutput "📦 Installing missing packages..." $Blue
    
    try {
        Write-ColorOutput "   Running: pip install -r requirements.txt" $Blue
        pip install -r requirements.txt
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "   ✓ Package installation completed" $Green
            return $true
        } else {
            Write-ColorOutput "   ✗ Package installation failed" $Red
            return $false
        }
    }
    catch {
        Write-ColorOutput "   ✗ Package installation failed: $($_.Exception.Message)" $Red
        return $false
    }
}

function Show-SetupInstructions {
    Write-ColorOutput "`n📋 Setup Instructions:" $Blue
    Write-ColorOutput "1. Create a local virtual environment:" $Blue
    Write-ColorOutput "   .\create_venv.ps1" $Blue
    Write-ColorOutput "`n2. Activate the virtual environment:" $Blue
    Write-ColorOutput "   .\.venv\Scripts\Activate.ps1" $Blue
    Write-ColorOutput "`n3. Copy and configure environment file:" $Blue
    Write-ColorOutput "   copy sample.env .env" $Blue
    Write-ColorOutput "   # Edit .env with your values" $Blue
    Write-ColorOutput "`n4. Install required packages:" $Blue
    Write-ColorOutput "   pip install -r requirements.txt" $Blue
    Write-ColorOutput "`n5. Run tests:" $Blue
    Write-ColorOutput "   python run_tests.py --suite code-interpreter --agent MAA-02 --quick" $Blue
}

# Main execution
Write-ColorOutput "🚀 FoundationaLLM Test Harness Environment Setup" $Blue
Write-ColorOutput ("=" * 50) $Blue

$allChecksPassed = $true

# Check environment
if (-not (Test-Environment)) {
    $allChecksPassed = $false
}

# Check Python environment
if (-not (Test-PythonEnvironment)) {
    $allChecksPassed = $false
}

# Check required packages
if (-not (Test-RequiredPackages)) {
    Write-ColorOutput "`n🔧 Attempting to install missing packages..." $Blue
    if (Install-MissingPackages) {
        Write-ColorOutput "✓ Retrying package check..." $Green
        if (-not (Test-RequiredPackages)) {
            $allChecksPassed = $false
        }
    } else {
        $allChecksPassed = $false
    }
}

# Check environment variables
if (-not (Test-EnvironmentVariables)) {
    $allChecksPassed = $false
}

Write-ColorOutput ("`n" + ("=" * 50)) $Blue

if ($allChecksPassed) {
    Write-ColorOutput "✅ Environment setup is complete!" $Green
    Write-ColorOutput "You can now run tests with:" $Green
    Write-ColorOutput "   python run_tests.py --suite code-interpreter --agent MAA-02 --quick" $Green
} else {
    Write-ColorOutput "❌ Environment setup has issues" $Red
    Show-SetupInstructions
    exit 1
}

Write-ColorOutput "`n🎯 Ready to run FoundationaLLM agent tests!" $Green
