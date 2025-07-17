# Test script to send a chat message to the API
$body = @{
    Sender = "TestUser"
    Message = "Hello from Azure Table Storage!"
} | ConvertTo-Json

$headers = @{
    "Content-Type" = "application/json"
}

# Disable certificate validation for testing
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

# Try ports that are actually listening
$ports = @("58080", "58081", "59090", "59091", "44394", "44399", "17185", "15066")

foreach ($port in $ports) {
    $serverUrl = "https://localhost:$port/chat/messages"
    
    try {
        Write-Host "Trying HTTPS port $port..."
        $response = Invoke-RestMethod -Uri $serverUrl -Method Post -Body $body -Headers $headers
        Write-Host "Message sent successfully on port $port!"
        Write-Host "Response: $($response | ConvertTo-Json -Depth 10)"
        break
    } catch {
        Write-Host "HTTPS Port $port failed: $($_.Exception.Message)"
        
        # Try HTTP instead of HTTPS
        $serverUrlHttp = "http://localhost:$port/chat/messages"
        try {
            Write-Host "Trying HTTP port $port..."
            $response = Invoke-RestMethod -Uri $serverUrlHttp -Method Post -Body $body -Headers $headers
            Write-Host "Message sent successfully on port $port (HTTP)!"
            Write-Host "Response: $($response | ConvertTo-Json -Depth 10)"
            break
        } catch {
            Write-Host "HTTP Port $port also failed: $($_.Exception.Message)"
        }
    }
}
