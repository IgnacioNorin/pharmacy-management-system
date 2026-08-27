<#
.SYNOPSIS
    Builds PharmacySystem in Release and assembles a distribution package under dist\.

.DESCRIPTION
    Run from the repository root:
        powershell -ExecutionPolicy Bypass -File deploy\package.ps1

    Produces:
        dist\PharmacySystem-<version>\        (deployable folder)
        dist\PharmacySystem-<version>.zip
#>

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot   = Split-Path -Parent $PSScriptRoot
$solution   = Join-Path $repoRoot 'PharmacySystem.sln'
$appProject = Join-Path $repoRoot 'PharmacySystem'
$releaseDir = Join-Path $appProject 'bin\Release'
$distRoot   = Join-Path $repoRoot 'dist'

function Find-MSBuild {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path $vswhere) {
        $path = & $vswhere -latest -requires Microsoft.Component.MSBuild `
            -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
        if ($path -and (Test-Path $path)) { return $path }
    }
    $fallbacks = @(
        'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe'
    )
    foreach ($f in $fallbacks) { if (Test-Path $f) { return $f } }
    throw "No se encontro MSBuild. Instalar Visual Studio 2022 con '.NET desktop development'."
}

$msbuild = Find-MSBuild
Write-Host "MSBuild: $msbuild"

Write-Host "Compilando en Release..."
& $msbuild $solution /t:Restore,Build /p:Configuration=Release /m /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw "La compilacion fallo (codigo $LASTEXITCODE)." }

$exe = Join-Path $releaseDir 'PharmacySystem.exe'
if (-not (Test-Path $exe)) { throw "No se encontro $exe tras compilar." }

$version = (Get-Item $exe).VersionInfo.FileVersion
if ([string]::IsNullOrWhiteSpace($version)) { $version = '0.0.0.0' }
$pkgName = "PharmacySystem-$version"
$pkgDir  = Join-Path $distRoot $pkgName

if (Test-Path $pkgDir) { Remove-Item $pkgDir -Recurse -Force }
New-Item -ItemType Directory -Path $pkgDir -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $pkgDir 'Database\Migrations') -Force | Out-Null

# Application binaries (exclude dev-only artifacts).
$exclude = @('*.pdb', '*.xml', '*.vshost.*', 'ConnectionStrings.config')
Get-ChildItem -Path $releaseDir -File | Where-Object {
    $name = $_.Name
    -not ($exclude | Where-Object { $name -like $_ })
} | Copy-Item -Destination $pkgDir

# Database scripts.
Copy-Item (Join-Path $repoRoot 'Database\PharmacyDB.sql') (Join-Path $pkgDir 'Database')
Copy-Item (Join-Path $repoRoot 'Database\Migrations\*')    (Join-Path $pkgDir 'Database\Migrations')

# Config template + docs.
Copy-Item (Join-Path $appProject 'ConnectionStrings.config.example') $pkgDir
Copy-Item (Join-Path $repoRoot 'CHANGELOG.md') $pkgDir
Copy-Item (Join-Path $repoRoot 'DEPLOY.md')    $pkgDir

@"
PharmacySystem $version
=======================

1. Base de datos
   - Instalacion nueva: ejecutar Database\PharmacyDB.sql en SQL Server.
   - Actualizacion:      aplicar Database\Migrations\ en orden (backup primero).

2. Configuracion
   - Copiar ConnectionStrings.config.example a ConnectionStrings.config
     (misma carpeta que PharmacySystem.exe) y completar servidor/base/credenciales.

3. Primer arranque
   - Ejecutar PharmacySystem.exe
   - Ingresar con documento 1010101010 / contrasena 12345678
   - Cambiar esa contrasena de inmediato y crear los usuarios reales.

Detalle completo en DEPLOY.md.
"@ | Set-Content -Path (Join-Path $pkgDir 'LEEME.txt') -Encoding UTF8

# Zip.
$zipPath = Join-Path $distRoot "$pkgName.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $pkgDir '*') -DestinationPath $zipPath

Write-Host ""
Write-Host "Paquete listo:"
Write-Host "  $pkgDir"
Write-Host "  $zipPath"
