<#
.SYNOPSIS
    Publishes PharmacySystem in Release and assembles a distribution package under dist\.

.DESCRIPTION
    Run from the repository root:
        powershell -ExecutionPolicy Bypass -File deploy\package.ps1

    Produces:
        dist\PharmacySystem-<version>\        (deployable folder)
        dist\PharmacySystem-<version>.zip

    The package is a framework-dependent .NET 10 app: the client machine needs the
    ".NET Desktop Runtime 10" installed. For a fully standalone package (no runtime
    install on the client) pass -SelfContained.
#>

param(
    [switch] $SelfContained
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot   = Split-Path -Parent $PSScriptRoot
$appProject = Join-Path $repoRoot 'PharmacySystem\PharmacySystem.csproj'
$publishDir = Join-Path $repoRoot 'PharmacySystem\bin\Release\net10.0-windows\publish'
$distRoot   = Join-Path $repoRoot 'dist'

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }

Write-Host "Publicando en Release..."
$publishArgs = @(
    'publish', $appProject,
    '-c', 'Release',
    '--nologo',
    '-p:PublishSingleFile=false'
)
if ($SelfContained) {
    $publishArgs += @('-r', 'win-x64', '--self-contained', 'true')
} else {
    $publishArgs += @('--self-contained', 'false')
}
& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) { throw "La publicacion fallo (codigo $LASTEXITCODE)." }

$exe = Join-Path $publishDir 'PharmacySystem.exe'
if (-not (Test-Path $exe)) { throw "No se encontro $exe tras publicar." }

$version = (Get-Item $exe).VersionInfo.FileVersion
if ([string]::IsNullOrWhiteSpace($version)) { $version = '0.0.0.0' }
$pkgName = "PharmacySystem-$version"
$pkgDir  = Join-Path $distRoot $pkgName

if (Test-Path $pkgDir) { Remove-Item $pkgDir -Recurse -Force }
New-Item -ItemType Directory -Path $pkgDir -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $pkgDir 'Database\Migrations') -Force | Out-Null

# Application binaries (exclude dev-only artifacts).
$exclude = @('*.pdb', '*.xml', 'ConnectionStrings.config')
Get-ChildItem -Path $publishDir -File | Where-Object {
    $name = $_.Name
    -not ($exclude | Where-Object { $name -like $_ })
} | Copy-Item -Destination $pkgDir

# Database scripts.
Copy-Item (Join-Path $repoRoot 'Database\PharmacyDB.sql') (Join-Path $pkgDir 'Database')
Copy-Item (Join-Path $repoRoot 'Database\Migrations\*')    (Join-Path $pkgDir 'Database\Migrations')

# Config template + docs.
Copy-Item (Join-Path $repoRoot 'PharmacySystem\ConnectionStrings.config.example') $pkgDir
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
   - Ejecutar PharmacySystem.exe (requiere .NET Desktop Runtime 10 salvo que el
     paquete se haya armado con -SelfContained).
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
