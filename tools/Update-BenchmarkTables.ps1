# Generates the "Results" section of docs/benchmarks.md from the exported BenchmarkDotNet reports.
# Usage: pwsh -File tools/Update-BenchmarkTables.ps1
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$results = Join-Path $root 'artifacts/benchmarks/results'
$doc = Join-Path $root 'docs/benchmarks.md'

if (-not (Test-Path $results)) { throw "No benchmark results in $results. Run the benchmarks first." }

$order = @(
    'ResultBenchmarks',
    'OptionBenchmarks',
    'EitherBenchmarks',
    'SequenceBenchmarks',
    'CollectionBenchmarks',
    'RopErrorBenchmarks',
    'ParsingBenchmarks',
    'AsyncBenchmarks',
    'EqualityBenchmarks'
)

$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine('<!-- RESULTS -->')
[void]$sb.AppendLine()

foreach ($name in $order) {
    $file = Join-Path $results "Sandlada.RopSharp.Benchmarks.$name-report-github.md"
    if (-not (Test-Path $file)) { continue }

    $lines = Get-Content $file
    $table = $lines | Where-Object { $_ -match '^\|' }
    if (-not $table) { continue }

    [void]$sb.AppendLine("### $name")
    [void]$sb.AppendLine()
    foreach ($line in $table) { [void]$sb.AppendLine($line) }
    [void]$sb.AppendLine()
}

$content = Get-Content $doc -Raw
$pattern = '(?s)<!-- RESULTS -->.*?(?=\r?\n## Interpretation)'
$replacement = $sb.ToString().TrimEnd()
if ($content -notmatch $pattern) { throw 'Could not find the RESULTS marker in docs/benchmarks.md.' }
$content = [regex]::Replace($content, $pattern, [System.Text.RegularExpressions.MatchEvaluator] { param($m) $replacement })
[System.IO.File]::WriteAllText($doc, $content, (New-Object System.Text.UTF8Encoding $false))

Write-Host "Updated $doc"
