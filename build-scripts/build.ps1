param(
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release',
    [ValidateSet('build','publish','final')]
    [string]$Target = 'final',
    [string]$Tag = 'aas-bike-showcase:local',
    [string]$Dockerfile = 'Dockerfile.Demoapp'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Determine repo root from script location (script is in build/)
$scriptDir = Split-Path -Parent $PSCommandPath
$repoRoot = Split-Path -Parent $scriptDir
Push-Location $repoRoot

try {
    Write-Host "==> Docker build started (Configuration: $Configuration, Target: $Target, Tag: $Tag)" -ForegroundColor Cyan

    $dockerfile = Join-Path 'build-scripts' $Dockerfile
    if (-not (Test-Path $dockerfile)) {
        throw "Dockerfile not found: $dockerfile"
    }

    # Map target to actual stage name in Dockerfile
    switch ($Target) {
        'build'   { $stage = 'build' }
        'publish' { $stage = 'publish' }
        'final'   { $stage = '' }
    }

    $args = @('build',
        '--file', $dockerfile,
        '--build-arg', "BUILD_CONFIGURATION=$Configuration",
        '--tag', $Tag
    )

    if ($stage) {
        $args += @('--target', $stage)
    }

    # Build context is repository root because Dockerfile does COPY of src/
    $args += '.'

    Write-Host ("-- Running: docker " + ($args -join ' ')) -ForegroundColor DarkCyan
    docker @args

    Write-Host "==> Docker image build completed: $Tag" -ForegroundColor Green
    exit 0
}
catch {
    Write-Error $_
    exit 1
}
finally {
    Pop-Location | Out-Null
}
