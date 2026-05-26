# Debug script to check API response
$url = 'https://en.wikipedia.org/w/api.php?action=parse&page=Playwright_(software)&prop=sections&format=json'
$headers = @{'User-Agent' = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)'}

try {
  $resp = Invoke-WebRequest -Uri $url -Headers $headers -TimeoutSec 10
  $json = $resp.Content | ConvertFrom-Json
  
  # List all sections
  $sections = $json.parse.sections
  
  Write-Host "Available sections:"
  foreach ($section in $sections) {
    if ($section.line -match 'Debugging|Feature|Report') {
      Write-Host "Section $($section.number): [$($section.level)] $($section.line)"
    }
  }
} catch {
  Write-Host "Error: $($_.Exception.Message)"
}
