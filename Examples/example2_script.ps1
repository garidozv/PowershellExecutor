Write-Error -Message "Error Stream: An error occurred!"

"Output Stream: Processing..."

$name = Read-Host -Prompt 'Your name:'

Write-Warning -Message "Warning Stream: Something might be wrong..."

Write-Information -MessageData "Information Stream: Just some info" -InformationAction 'Continue'

Write-Error -Message "Error Stream: Another error occurred!"

Write-Host 'Your name is' $name  -Separator ' --> ' -BackgroundColor Green  -ForegroundColor White

Write-Verbose -Message "Verbose Stream: Some verbose details..." -Verbose

$DebugPreference = 'Continue'
Write-Debug -Message "Debug Stream: Debug information"
