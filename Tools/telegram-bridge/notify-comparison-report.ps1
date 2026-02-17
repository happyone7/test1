param(
  [string]$Message,
  [string]$ReportPath = (Join-Path $PSScriptRoot '..\..\Docs\Sprint6_Comparison_Report.md'),
  [string]$Token = $env:TELEGRAM_BOT_TOKEN,
  [string]$ChatId = $env:TELEGRAM_CHAT_ID,
  [string]$ConfigPath = (Join-Path $PSScriptRoot '.env.telegram')
)

function Get-ConfigValue {
  param([string]$Path, [string]$Key)
  if (-not (Test-Path $Path)) { return $null }
  $line = Get-Content $Path | Where-Object { $_ -match "^$Key=" } | Select-Object -First 1
  if (-not $line) { return $null }
  $value = $line.Substring($Key.Length + 1)
  if ($value -like '"*"') { return $value.Trim('"') }
  return $value
}

if (-not $Token) { $Token = Get-ConfigValue -Path $ConfigPath -Key 'TELEGRAM_BOT_TOKEN' }
if (-not $ChatId) { $ChatId = Get-ConfigValue -Path $ConfigPath -Key 'TELEGRAM_CHAT_ID' }

if (-not $Token -or -not $ChatId) {
  Write-Error 'TELEGRAM_BOT_TOKEN and TELEGRAM_CHAT_ID are required. Set env vars or .env.telegram.'
  exit 1
}

if (-not $Message -and (Test-Path $ReportPath)) {
  $ReportRaw = Get-Content -Raw $ReportPath
  $Message = if ($ReportRaw.Length -gt 3900) {
    $ReportRaw.Substring(0, 3900) + "`n... (truncated)"
  } else {
    $ReportRaw
  }
}

if (-not $Message) {
  Write-Error 'Message content is empty. Use -Message or provide existing ReportPath.'
  exit 1
}

$sendScript = Join-Path $PSScriptRoot 'send-telegram.ps1'
if (-not (Test-Path $sendScript)) {
  Write-Error "send-telegram.ps1 not found at $sendScript"
  exit 1
}

& $sendScript -Text $Message -Token $Token -ChatId $ChatId
