param(
    [string]$InstallDir = "$env:ProgramFiles\ABDS"
)

$ErrorActionPreference = "Stop"

$serviceName = "ABDS (AutoBackup & DataSynchronize)"
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
        "-InstallDir", "`"$InstallDir`""
    )
    Start-Process -FilePath "powershell.exe" -ArgumentList $args -Verb RunAs
    exit
}

Get-Process -Name "ABDS.DesktopHost","ABDS.TrayAgent","ABDS.Web","ABDS.Cli" -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue

$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($service) {
    if ($service.Status -ne "Stopped") {
        Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
        $service.WaitForStatus("Stopped", "00:00:20")
    }
    & sc.exe delete $serviceName | Out-Null
}

Remove-ItemProperty -Path $runKey -Name $startupValueName -ErrorAction SilentlyContinue
Remove-Item -Path $uninstallKey -Recurse -Force -ErrorAction SilentlyContinue

$programs = [Environment]::GetFolderPath("CommonPrograms")
$desktop = [Environment]::GetFolderPath("CommonDesktopDirectory")
Remove-Item -LiteralPath (Join-Path $desktop "ABDS.lnk") -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath (Join-Path $programs "ABDS") -Recurse -Force -ErrorAction SilentlyContinue

$escapedInstallDir = $InstallDir.Replace("'", "''")
Start-Process -FilePath "powershell.exe" -WindowStyle Hidden -ArgumentList @(
    "-NoProfile",
    "-Command",
    "Start-Sleep -Seconds 2; Remove-Item -LiteralPath '$escapedInstallDir' -Recurse -Force -ErrorAction SilentlyContinue"
)
