param(
    [string]$KeePassPath,
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectFile = Join-Path $projectRoot "KPassPilot.csproj"

function Resolve-KeePassPath {
    param([string]$RequestedPath)

    if ($RequestedPath) { return (Resolve-Path $RequestedPath).Path }

    $candidates = @(
        (Join-Path $env:ProgramFiles "KeePass Password Safe 2\KeePass.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "KeePass Password Safe 2\KeePass.exe"),
        (Join-Path $projectRoot "lib\KeePass.exe")
    )

    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path $candidate)) { return (Resolve-Path $candidate).Path }
    }

    throw "KeePass.exe introuvable. Utilisez -KeePassPath 'C:\...\KeePass.exe'."
}

function Resolve-MSBuild {
    $command = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $found = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
        if ($found -and (Test-Path $found)) { return $found }
    }

    throw "MSBuild introuvable. Installez Visual Studio Build Tools."
}

$resolvedKeePass = Resolve-KeePassPath -RequestedPath $KeePassPath
$msbuild = Resolve-MSBuild

Write-Host "KeePass : $resolvedKeePass" -ForegroundColor Cyan
Write-Host "MSBuild : $msbuild" -ForegroundColor Cyan
Write-Host "Configuration : $Configuration" -ForegroundColor Cyan

& $msbuild $projectFile "/t:Rebuild" "/p:Configuration=$Configuration" "/p:Platform=AnyCPU" "/p:KeePassExe=$resolvedKeePass" "/v:minimal"
if ($LASTEXITCODE -ne 0) { throw "La compilation de KPassPilot a échoué." }

$output = Join-Path $projectRoot ("bin\" + $Configuration + "\KPassPilot.dll")
if (-not (Test-Path $output)) { throw "Compilation terminée mais KPassPilot.dll est introuvable." }

$versionInfo = (Get-Item $output).VersionInfo
if ($versionInfo.ProductName -ne "KeePass Plugin") {
    throw "DLL invalide pour KeePass : ProductName='$($versionInfo.ProductName)'. Attendu : 'KeePass Plugin'."
}

Write-Host ""
Write-Host "KPassPilot compilé : $output" -ForegroundColor Green
Write-Host "ProductName : $($versionInfo.ProductName)" -ForegroundColor Green
Write-Host "Version : $($versionInfo.FileVersion)" -ForegroundColor Green
Write-Host ""
Write-Host "Copiez KPassPilot.dll dans le dossier Plugins de KeePass puis redémarrez KeePass." -ForegroundColor Yellow
