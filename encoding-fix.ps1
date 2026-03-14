$cp1251 = [System.Text.Encoding]::GetEncoding(1251)
$utf8 = New-Object System.Text.UTF8Encoding($false)

function Test-Utf8 {
    param([byte[]]$bytes)

    $utf8Strict = New-Object System.Text.UTF8Encoding($false, $true)

    try {
        $null = $utf8Strict.GetString($bytes)
        return $true
    }
    catch {
        return $false
    }
}

Get-ChildItem -Recurse -File -Include *.cs,*.cpp,*.h,*.hpp,*.c,*.txt | ForEach-Object {

    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)

    if (Test-Utf8 $bytes) {
        return
    }

    $text = $cp1251.GetString($bytes)
    [System.IO.File]::WriteAllText($_.FullName, $text, $utf8)

    Write-Host "Converted:" $_.FullName
}