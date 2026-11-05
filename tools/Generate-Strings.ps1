param([string]$Tsv, [string]$OutRoot)
$rows = Get-Content $Tsv -Encoding UTF8 | Where-Object { $_.Trim() } | ForEach-Object { $x = $_ -split "`t"; [pscustomobject]@{ Key=$x[0]; Pt=$x[1]; En= if ($x.Count -gt 2 -and $x[2]) { $x[2] } else { $x[1] } } }
foreach ($lang in 'pt-BR','en-US') {
  $sb = [Text.StringBuilder]::new()
  [void]$sb.AppendLine('<?xml version="1.0" encoding="utf-8"?>'); [void]$sb.AppendLine('<root>')
  [void]$sb.AppendLine('  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>')
  [void]$sb.AppendLine('  <resheader name="version"><value>2.0</value></resheader>')
  [void]$sb.AppendLine('  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>')
  [void]$sb.AppendLine('  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>')
  foreach ($r in $rows) { $v = if ($lang -eq 'pt-BR') { $r.Pt } else { $r.En }; [void]$sb.AppendLine("  <data name=`"$($r.Key)`" xml:space=`"preserve`"><value>$([Security.SecurityElement]::Escape($v))</value></data>") }
  [void]$sb.AppendLine('</root>')
  [IO.File]::WriteAllText("$OutRoot\$lang\Resources.resw", $sb.ToString(), [Text.UTF8Encoding]::new($false))
}
"$($rows.Count) textos"
