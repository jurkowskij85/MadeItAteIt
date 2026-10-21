<#
.SYNOPSIS
  Builds the "Made It, Ate It" mod without Unity and packs it into dist~\.

.EXAMPLE
  .\build.ps1                 # build, pack into dist~\MadeItAteIt and dist~\MadeItAteIt-<version>.zip
  .\build.ps1 -Install        # ...and copy it to Documents\Timberborn\Mods\MadeItAteIt
  .\build.ps1 -GameDir "D:\SteamLibrary\steamapps\common\Timberborn"
#>
param(
  [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\Timberborn",
  [string]$HarmonyDll = "",
  [switch]$Install
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# 1. Game DLLs
$managed = Join-Path $GameDir "Timberborn_Data\Managed"
if (-not (Test-Path (Join-Path $managed "Timberborn.BaseComponentSystem.dll"))) {
  throw "Nie znaleziono bibliotek gry w '$managed'. Podaj -GameDir <folder instalacji Timberborn>."
}

# 2. Harmony (Steam Workshop mod 3284904751 or a manually installed copy)
if (-not $HarmonyDll) {
  $steamApps = Split-Path (Split-Path $GameDir -Parent) -Parent
  $candidates = @(
    (Join-Path $steamApps "workshop\content\1062090\3284904751"),
    (Join-Path ([Environment]::GetFolderPath("MyDocuments")) "Timberborn\Mods")
  ) | Where-Object { Test-Path $_ }
  $found = $candidates | ForEach-Object { Get-ChildItem $_ -Recurse -Filter "0Harmony.dll" -ErrorAction SilentlyContinue } |
           Sort-Object FullName -Descending | Select-Object -First 1
  if ($found) { $HarmonyDll = $found.FullName }
}
if (-not $HarmonyDll -or -not (Test-Path $HarmonyDll)) {
  throw "Nie znaleziono 0Harmony.dll. Zasubskrybuj mod Harmony w Steam Workshop albo podaj -HarmonyDll <ścieżka>."
}
Write-Host "Gra:     $managed"
Write-Host "Harmony: $HarmonyDll"

# 3. Compile
$project = Join-Path $root "Build~\MadeItAteIt.csproj"
$outDir = Join-Path $root "Build~\bin"
dotnet build $project -c Release -o $outDir "-p:GameManagedDir=$managed" "-p:HarmonyDll=$HarmonyDll"
if ($LASTEXITCODE -ne 0) { throw "Kompilacja nie powiodła się." }

# 4. Pack (same layout the Unity Mod Builder produces)
$manifest = Get-Content (Join-Path $root "manifest.json") -Raw | ConvertFrom-Json
$dist = Join-Path $root "dist~"
$modDir = Join-Path $dist "MadeItAteIt"
if (Test-Path $modDir) { Remove-Item $modDir -Recurse -Force }
New-Item -ItemType Directory -Path $modDir | Out-Null
Copy-Item (Join-Path $root "manifest.json") $modDir
Copy-Item (Join-Path $outDir "MadeItAteIt.dll") $modDir
Copy-Item (Join-Path $root "Data\*") $modDir -Recurse
Copy-Item (Join-Path $root "Root\*") $modDir -Recurse

$zip = Join-Path $dist "MadeItAteIt-$($manifest.Version).zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $modDir "*") -DestinationPath $zip
Write-Host "Gotowe: $modDir"
Write-Host "Paczka: $zip"

# 5. Install
if ($Install) {
  $target = Join-Path ([Environment]::GetFolderPath("MyDocuments")) "Timberborn\Mods\MadeItAteIt"
  # workshop_data.json holds the Steam Workshop item id. Without it the next upload would create
  # a brand new Workshop item instead of updating the existing one, so it must survive reinstalling.
  $workshopData = Join-Path $target "workshop_data.json"
  $savedWorkshopData = Join-Path ([IO.Path]::GetTempPath()) "MadeItAteIt_workshop_data.json"
  $hasWorkshopData = Test-Path $workshopData
  if ($hasWorkshopData) { Copy-Item $workshopData $savedWorkshopData -Force }
  if (Test-Path $target) { Remove-Item $target -Recurse -Force }
  Copy-Item $modDir $target -Recurse
  if ($hasWorkshopData) {
    Move-Item $savedWorkshopData $workshopData -Force
  } elseif (Test-Path (Join-Path $root "workshop_data.json")) {
    Copy-Item (Join-Path $root "workshop_data.json") $target
  }
  Write-Host "Zainstalowano w: $target"
}
