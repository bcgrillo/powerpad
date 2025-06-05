Get-ChildItem ../*.md | ForEach-Object {
    (Get-Content $_ -Raw -Encoding UTF8) -replace '!\[\[(.*?)\]\]', '![image](../Pictures/$1)' | Set-Content "$($_.BaseName)_tmp.md" -Encoding UTF8
}

pandoc (Get-ChildItem *_tmp.md | Sort-Object Name | Select-Object -ExpandProperty Name) --citeproc --bibliography=References.bib --csl Templates/ieee-with-url.csl -o MemoriaPowerPad.docx --reference-doc=Templates/article.docx

Remove-Item *_tmp.md