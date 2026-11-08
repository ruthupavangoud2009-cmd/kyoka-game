param(
    [string]$ManagedDir = "D:\bots updated\KYOKA_Full_Unity_Project\KYOKA_Full_Unity_Project\Builds\Windows\KYOKA_Data\Managed",
    [string]$ScriptsDir = "D:\bots updated\KYOKA_Full_Unity_Project\KYOKA_Full_Unity_Project\Assets\KYOKA\Scripts"
)

$ErrorActionPreference = "Stop"

$csc = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = (Get-Command csc.exe -ErrorAction SilentlyContinue).Source
}

if (-not (Test-Path $csc)) {
    Write-Error "Roslyn csc compiler not found!"
    exit 1
}

Write-Host "Compiling Kyoka.Runtime.dll with Roslyn csc..." -ForegroundColor Cyan

$csFiles = Get-ChildItem -Path $ScriptsDir -Recurse -Filter *.cs | Select-Object -ExpandProperty FullName
$refDlls = Get-ChildItem -Path $ManagedDir -Filter *.dll | Where-Object { $_.Name -ne "Kyoka.Runtime.dll" } | Select-Object -ExpandProperty FullName

$argsList = [System.Collections.Generic.List[string]]::new()
$argsList.Add("/target:library")
$argsList.Add("/nostdlib+")
$argsList.Add("/out:`"$ManagedDir\Kyoka.Runtime.dll`"")
$argsList.Add("/langversion:latest")
$argsList.Add("/nologo")
$argsList.Add("/nowarn:0168,0219,0414,0649")

foreach ($dll in $refDlls) {
    $argsList.Add("/r:`"$dll`"")
}

foreach ($f in $csFiles) {
    $argsList.Add("`"$f`"")
}

$rspFile = "$env:TEMP\kyoka_compile.rsp"
[System.IO.File]::WriteAllLines($rspFile, $argsList)

$process = Start-Process -FilePath $csc -ArgumentList "@`"$rspFile`"" -NoNewWindow -Wait -PassThru -RedirectStandardOutput "$env:TEMP\kyoka_stdout.log" -RedirectStandardError "$env:TEMP\kyoka_stderr.log"

if ($process.ExitCode -eq 0) {
    Write-Host "[SUCCESS] Kyoka.Runtime.dll compiled successfully into Managed folder!" -ForegroundColor Green
    Get-Item "$ManagedDir\Kyoka.Runtime.dll" | Select-Object FullName, LastWriteTime, Length
} else {
    Write-Host "[ERROR] Compilation failed with exit code $($process.ExitCode):" -ForegroundColor Red
    if (Test-Path "$env:TEMP\kyoka_stdout.log") { Get-Content "$env:TEMP\kyoka_stdout.log" }
    if (Test-Path "$env:TEMP\kyoka_stderr.log") { Get-Content "$env:TEMP\kyoka_stderr.log" }
    exit 1
}
