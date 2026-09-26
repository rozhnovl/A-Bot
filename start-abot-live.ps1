param(
    [string]$RunProfile = 'Hawk_T4'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$abot = Join-Path $root 'src\AbotMcp\bin\Debug\net9.0\AbotMcp.dll'
$clients = @(Get-Process -Name exefile -ErrorAction Stop | Sort-Object Id)

if ($clients.Count -eq 0) {
    throw 'Не найден ни один запущенный клиент EVE (exefile.exe).'
}

$arguments = @('--live', '--profile', $RunProfile, '--port', '5030')
foreach ($client in $clients) {
    $arguments += @('--pid', $client.Id)
}

Write-Host "Запуск A-Bot от пользователя $env:USERNAME для PID: $($clients.Id -join ', ')"
& dotnet $abot @arguments
exit $LASTEXITCODE
