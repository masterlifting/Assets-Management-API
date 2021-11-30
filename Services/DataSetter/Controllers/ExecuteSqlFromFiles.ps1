$files = Get-ChildItem -Filter *.sql
for ($i = 0; $i -lt $files.Count; $i++) {
    $file = $files[$i]
    $temp = get-content $file
    set-content $file -Value $temp -force -Encoding UTF8
    Invoke-Sqlcmd -ServerInstance "D5ZFRC-APC005WN" -Database "Depo" -InputFile $file.FullName
    Write-Host $file.FullName
}