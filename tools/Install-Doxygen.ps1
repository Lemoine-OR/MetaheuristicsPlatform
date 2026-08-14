$ErrorActionPreference = "Stop"

if (Get-Command doxygen -ErrorAction SilentlyContinue) {
    & doxygen --version
    exit 0
}

if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
    throw "Doxygen is missing and Chocolatey is unavailable."
}

choco install doxygen.install -y --no-progress

if (-not (Get-Command doxygen -ErrorAction SilentlyContinue)) {
    $possible =
        Get-ChildItem "C:\ProgramData\chocolatey\lib" -Recurse -File -Filter "doxygen.exe" -ErrorAction SilentlyContinue |
        Select-Object -First 1

    if ($null -eq $possible) {
        throw "Doxygen installation completed but doxygen.exe could not be found."
    }

    $env:PATH =
        "$($possible.Directory.FullName);$env:PATH"
}

& doxygen --version
