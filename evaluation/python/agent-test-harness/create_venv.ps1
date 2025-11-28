# PowerShell script to create a local virtual environment for the test harness

Push-Location $PSScriptRoot

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
    Write-Host $Message -ForegroundColor $Color
}

Write-ColorOutput "🚀 Creating local virtual environment for FoundationaLLM Agents Evaluation" $Blue
Write-ColorOutput ("=" * 50) $Blue

# Check Python version
try {
    $pythonVersion = python --version 2>&1
    Write-ColorOutput "Python version: $pythonVersion" $Green
    
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
    Write-ColorOutput "Please install Python 3.10+ and ensure it's in your PATH" $Red
    exit 1
}

# Create virtual environment
$venvPath = "pkg/.venv"
if (Test-Path $venvPath) {
    Write-ColorOutput "⚠️  Virtual environment already exists at: $venvPath" $Yellow
    $response = Read-Host "Do you want to recreate it? (y/n)"
    if ($response -eq "y") {
        Write-ColorOutput "Removing existing virtual environment..." $Blue
        Remove-Item -Recurse -Force $venvPath
    } else {
        Write-ColorOutput "Using existing virtual environment" $Green
        exit 0
    }
}

Write-ColorOutput "Creating virtual environment at: $venvPath" $Blue
python -m venv $venvPath

if (-not (Test-Path $venvPath)) {
    Write-ColorOutput "❌ Failed to create virtual environment" $Red
    exit 1
}

Write-ColorOutput "✅ Virtual environment created successfully!" $Green
Write-ColorOutput "`nNext steps:" $Blue
Write-ColorOutput "1. Activate the virtual environment:" $Blue
Write-ColorOutput "   .\.venv\Scripts\Activate.ps1" $Blue
Write-ColorOutput "`n2. Copy and configure environment file:" $Blue
Write-ColorOutput "   copy sample.env .env" $Blue
Write-ColorOutput "   # Edit .env with your values" $Blue
Write-ColorOutput "`n3.1 Install agent evaluation package:" $Blue
Write-ColorOutput "   pip install <path-to-package>" $Blue
Write-ColorOutput
Write-ColorOutput "   (or, if developing/contributing)" $Blue
Write-ColorOutput "`n3.2 Install agent evaluation package as editable (from pkg folder):" $Blue
Write-ColorOutput "   pip install -e ." $Blue
Write-ColorOutput "`n4. Run tests:" $Blue
Write-ColorOutput "   fllm-agent-eval --suite code-interpreter --agent MAA-02 --quick" $Blue

Pop-Location
