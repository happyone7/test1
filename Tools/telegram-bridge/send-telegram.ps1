param(
  [Parameter(Mandatory = $true)]
  [string]$Text,
  [string]$Token = $env:TELEGRAM_BOT_TOKEN,
  [string]$ChatId = $env:TELEGRAM_CHAT_ID,
  [string]$ConfigPath = (Join-Path $PSScriptRoot '.env.telegram')
)

function Get-ConfigValue {
  param(
    [string]$Path,
    [string]$Key
  )

  if (-not (Test-Path $Path)) {
    return $null
  }

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

$uri = "https://api.telegram.org/bot$Token/sendMessage"
$body = @{
  chat_id = $ChatId
  text = $Text
}

try {
  $result = Invoke-RestMethod -Method Post -Uri $uri -Body $body
  if ($result.ok -eq $true) {
    Write-Output "ok=$($result.result.message_id)"
  } else {
    Write-Error "Telegram API Error: $($result | ConvertTo-Json -Depth 10)"
    exit 1
  }
} catch {
  Write-Error "send failed: $_"
  exit 1
}
