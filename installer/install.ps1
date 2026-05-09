param(
    [string]$InstallDir = "$env:ProgramFiles\ABDS",
    [string]$AppVersion = "0.1.0",
    [switch]$AllowDowngrade
)

$ErrorActionPreference = "Stop"

$serviceName = "ABDS (AutoBackup & DataSynchronize)"
$displayName = "ABDS (AutoBackup & DataSynchronize)"
$startupValueName = "ABDS Tray Agent"
$runKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
$uninstallKey = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\ABDS"

function Test-Admin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)
}

if (-not (Test-Admin)) {
    $args = @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", "`"$PSCommandPath`"",
        "-InstallDir", "`"$InstallDir`"",
        "-AppVersion", "`"$AppVersion`""
    )
    if ($AllowDowngrade) {
        $args += "-AllowDowngrade"
    }
    Start-Process -FilePath "powershell.exe" -ArgumentList $args -Verb RunAs
    exit
}

function Stop-ProcessIfRunning([string[]]$names) {
    foreach ($name in $names) {
        Get-Process -Name $name -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    }
}

function Invoke-Sc([string[]]$arguments) {
    $output = & sc.exe @arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host $output
    }
}

function Get-ServiceOrNull([string]$name) {
    return Get-Service -Name $name -ErrorAction SilentlyContinue
}

function Create-Shortcut([string]$path, [string]$target, [string]$arguments, [string]$description, [string]$icon) {
    $folder = Split-Path -Parent $path
    New-Item -ItemType Directory -Force -Path $folder | Out-Null
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($path)
    $shortcut.TargetPath = $target
    $shortcut.Arguments = $arguments
    $shortcut.WorkingDirectory = Split-Path -Parent $target
    $shortcut.Description = $description
    $shortcut.IconLocation = $icon
    $shortcut.Save()
}

function Convert-ToComparableVersion([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $null
    }

    $core = ($value.Trim() -split '[-+]')[0]
    $parts = @($core.Split('.') | ForEach-Object {
        $n = 0
        if ([int]::TryParse($_, [ref]$n)) { $n } else { 0 }
    })
    while ($parts.Count -lt 4) {
        $parts += 0
    }

    return [Version]::new($parts[0], $parts[1], $parts[2], $parts[3])
}

$payloadZip = Join-Path $PSScriptRoot "payload.zip"
if (-not (Test-Path -LiteralPath $payloadZip)) {
    throw "Nie znaleziono payload.zip obok instalatora."
}

$installedVersionText = $null
if (Test-Path -LiteralPath $uninstallKey) {
    $installedVersionText = (Get-ItemProperty -Path $uninstallKey -Name "DisplayVersion" -ErrorAction SilentlyContinue).DisplayVersion
}

$installedVersion = Convert-ToComparableVersion $installedVersionText
$newVersion = Convert-ToComparableVersion $AppVersion
if ($installedVersion -and $newVersion -and $newVersion -lt $installedVersion -and -not $AllowDowngrade) {
    throw "Zainstalowana wersja ABDS ($installedVersionText) jest nowsza niż uruchomiony instalator ($AppVersion). Instalacja starszej wersji została zablokowana."
}

$tempRoot = Join-Path $env:TEMP ("ABDS-Install-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

try {
    Expand-Archive -LiteralPath $payloadZip -DestinationPath $tempRoot -Force
    $appSource = Join-Path $tempRoot "App"
    if (-not (Test-Path -LiteralPath $appSource)) {
        throw "Payload nie zawiera katalogu App."
    }

    $service = Get-ServiceOrNull $serviceName
    if ($service -and $service.Status -ne "Stopped") {
        Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
        $service.WaitForStatus("Stopped", "00:00:20")
    }

    Stop-ProcessIfRunning @("ABDS.DesktopHost", "ABDS.TrayAgent", "ABDS.Web", "ABDS.Cli")

    New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
    Copy-Item -Path (Join-Path $appSource "*") -Destination $InstallDir -Recurse -Force

    $desktopHostExe = Join-Path $InstallDir "ABDS.DesktopHost.exe"
    $trayExe = Join-Path $InstallDir "ABDS.TrayAgent.exe"
    $serviceExe = Join-Path $InstallDir "ABDS.Service.exe"
    $icon = Join-Path $InstallDir "Resources\icon.ico"
    if (-not (Test-Path -LiteralPath $icon)) {
        $icon = $desktopHostExe
    }

    if (-not (Test-Path -LiteralPath $serviceExe)) {
        throw "Nie znaleziono ABDS.Service.exe w katalogu instalacji."
    }

    if (Get-ServiceOrNull $serviceName) {
        Invoke-Sc -arguments @("config", $serviceName, "binPath=", "`"$serviceExe`"", "start=", "auto", "DisplayName=", $displayName)
    }
    else {
        Invoke-Sc -arguments @("create", $serviceName, "binPath=", "`"$serviceExe`"", "start=", "auto", "DisplayName=", $displayName)
    }

    Invoke-Sc -arguments @("description", $serviceName, "AutoBackup & DataSynchronize background service")
    Start-Service -Name $serviceName -ErrorAction SilentlyContinue

    $programs = [Environment]::GetFolderPath("CommonPrograms")
    $desktop = [Environment]::GetFolderPath("CommonDesktopDirectory")
    $startMenuDir = Join-Path $programs "ABDS"

    Create-Shortcut `
        -path (Join-Path $desktop "ABDS.lnk") `
        -target $desktopHostExe `
        -arguments "" `
        -description "Open ABDS" `
        -icon $icon

    Create-Shortcut `
        -path (Join-Path $startMenuDir "ABDS.lnk") `
        -target $desktopHostExe `
        -arguments "" `
        -description "Open ABDS" `
        -icon $icon

    Create-Shortcut `
        -path (Join-Path $startMenuDir "ABDS Tray Agent.lnk") `
        -target $trayExe `
        -arguments "" `
        -description "Start ABDS tray status agent" `
        -icon $icon

    Create-Shortcut `
        -path (Join-Path $startMenuDir "Uninstall ABDS.lnk") `
        -target "powershell.exe" `
        -arguments "-NoProfile -ExecutionPolicy Bypass -File `"$InstallDir\uninstall.ps1`"" `
        -description "Uninstall ABDS" `
        -icon $icon

    New-Item -Path $runKey -Force | Out-Null
    New-ItemProperty -Path $runKey -Name $startupValueName -Value "`"$trayExe`"" -PropertyType String -Force | Out-Null

    New-Item -Path $uninstallKey -Force | Out-Null
    New-ItemProperty -Path $uninstallKey -Name "DisplayName" -Value "ABDS" -PropertyType String -Force | Out-Null
    New-ItemProperty -Path $uninstallKey -Name "DisplayVersion" -Value $AppVersion -PropertyType String -Force | Out-Null
    New-ItemProperty -Path $uninstallKey -Name "Publisher" -Value "ABDS" -PropertyType String -Force | Out-Null
    New-ItemProperty -Path $uninstallKey -Name "InstallLocation" -Value $InstallDir -PropertyType String -Force | Out-Null
    New-ItemProperty -Path $uninstallKey -Name "DisplayIcon" -Value $icon -PropertyType String -Force | Out-Null
    New-ItemProperty -Path $uninstallKey -Name "UninstallString" -Value "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$InstallDir\uninstall.ps1`"" -PropertyType String -Force | Out-Null

    if (Test-Path -LiteralPath $trayExe) {
        Start-Process -FilePath $trayExe -WorkingDirectory $InstallDir
    }

    if (Test-Path -LiteralPath $desktopHostExe) {
        Start-Process -FilePath $desktopHostExe -WorkingDirectory $InstallDir
    }
}
finally {
    Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
