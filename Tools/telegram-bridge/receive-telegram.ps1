param(
  [int]$Count = 0,
  [int]$Timeout = 20,
  [string]$Token = $env:TELEGRAM_BOT_TOKEN,
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
if (-not $Token) {
  Write-Error 'TELEGRAM_BOT_TOKEN is required. Set env var or .env.telegram.'
  exit 1
}

$uri = "https://api.telegram.org/bot$Token/getUpdates"
$offset = 0
$received = 0

while ($true) {
  try {
    $reqUri = "$uri?timeout=$Timeout&offset=$offset"
    $result = Invoke-RestMethod -Method Get -Uri $reqUri

    if (-not $result.ok) {
      Write-Error "Telegram API Error: $($result | ConvertTo-Json -Depth 8)"
      exit 1
    }

    foreach ($update in $result.result) {
      $offset = [int64]$update.update_id + 1
      if (-not $update.message) { continue }

      $chatId = $update.message.chat.id
      $user = $update.message.from.username
      $text = $update.message.text
      $time = [DateTimeOffset]::FromUnixTimeSeconds([int64]$update.message.date).ToLocalTime().ToString('yyyy-MM-dd HH:mm:ss')

      [PSCustomObject]@{
        time = $time
        chatId = $chatId
        user = $user
        text = $text
      } | ConvertTo-Json -Compress

      $received++
      if ($Count -gt 0 -and $received -ge $Count) {
        return
      }
    }
  } catch {
    Write-Error "receive failed: $_"
    Start-Sleep -Seconds 3
  }
}
