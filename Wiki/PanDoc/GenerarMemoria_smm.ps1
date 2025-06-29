$outputFile = "PanDoc/MemoriaPowerPad.docx"
$memoriaPath = "_ Memoria.md"
$tempPath = "_ Memoria_temp.md"

cd ..

if (Test-Path $outputFile) {
    try {
        Remove-Item $outputFile -ErrorAction Stop
    } catch {
        Write-Error "No se puede borrar el archivo existente. Deteniendo el script."
	cd PanDoc
        exit
    }
}

# Lee el contenido de Memoria.md en UTF-8
$content = Get-Content -Path $memoriaPath -Raw -Encoding utf8

# Funcion de reemplazo
$pattern = '!\[\[(.+?)\]\]'
$content = [System.Text.RegularExpressions.Regex]::Replace($content, $pattern, {
    param($match)
    $fileName = "$($match.Groups[1].Value).md"
    $filePath = $fileName
    if (Test-Path $filePath) {
        # Leemos el archivo y nos aseguramos de quitar saltos de línea finales extra
        (Get-Content -Path $filePath -Raw -Encoding utf8).TrimEnd()
    }
    else {
        $match.Value
    }
})

# Reemplazo para imágenes con alt tipo "Figura X.Y."
$figuraPattern = '!\[(Figura\s\d+\.\d+)(\.?)\s'
$content = [System.Text.RegularExpressions.Regex]::Replace($content, $figuraPattern, '![**$1**$2 ')

# Escribe el resultado en el archivo temporal en UTF-8
Set-Content -Path $tempPath -Value $content -Encoding utf8

pandoc $tempPath --toc --lof --citeproc --bibliography=PanDoc/References.bib --csl PanDoc/Templates/ieee-with-url.csl --top-level-division=chapter -o $outputFile --reference-doc=PanDoc/Templates/article.docx

Remove-Item $tempPath


$word = New-Object -ComObject Word.Application
$word.Visible = $false
$doc = $word.Documents.Open((Resolve-Path $outputFile).Path)
$selection = $word.Selection
$selection.InsertFile((Resolve-Path "PanDoc/Templates/portada.docx").Path)
$doc.Save()
$doc.Close()
$word.Quit()


cd PanDoc