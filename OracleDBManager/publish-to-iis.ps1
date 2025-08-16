# Script de publicación para IIS
# Ejecutar como administrador

$projectPath = "OracleDBManager.Web\OracleDBManager.Web.csproj"
$publishPath = "C:\inetpub\wwwroot\OracleDBManager"
$appPoolName = "OracleDBManager"
$siteName = "OracleDBManager"
$port = 8080

Write-Host "=== Oracle Database Manager - Script de Publicación ===" -ForegroundColor Cyan

# 1. Compilar y publicar la aplicación
Write-Host "1. Compilando y publicando la aplicación..." -ForegroundColor Yellow
dotnet publish $projectPath -c Release -o $publishPath --runtime win-x64 --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al publicar la aplicación" -ForegroundColor Red
    exit 1
}

# 2. Crear el Application Pool si no existe
Write-Host "2. Configurando Application Pool..." -ForegroundColor Yellow
Import-Module WebAdministration

if (!(Test-Path "IIS:\AppPools\$appPoolName")) {
    New-WebAppPool -Name $appPoolName
    Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name processIdentity.identityType -Value ApplicationPoolIdentity
    Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name managedRuntimeVersion -Value ""
    Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name enable32BitAppOnWin64 -Value $false
    Write-Host "   Application Pool '$appPoolName' creado" -ForegroundColor Green
} else {
    Write-Host "   Application Pool '$appPoolName' ya existe" -ForegroundColor Gray
}

# 3. Crear el sitio web si no existe
Write-Host "3. Configurando sitio web..." -ForegroundColor Yellow
if (!(Get-Website -Name $siteName -ErrorAction SilentlyContinue)) {
    New-Website -Name $siteName -Port $port -PhysicalPath $publishPath -ApplicationPool $appPoolName
    Write-Host "   Sitio web '$siteName' creado en puerto $port" -ForegroundColor Green
} else {
    Write-Host "   Sitio web '$siteName' ya existe" -ForegroundColor Gray
}

# 4. Configurar permisos
Write-Host "4. Configurando permisos..." -ForegroundColor Yellow
$acl = Get-Acl $publishPath
$permission = "IIS_IUSRS", "ReadAndExecute,Write", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)

$permission2 = "IIS AppPool\$appPoolName", "ReadAndExecute,Write", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule2 = New-Object System.Security.AccessControl.FileSystemAccessRule $permission2
$acl.SetAccessRule($accessRule2)

Set-Acl $publishPath $acl
Write-Host "   Permisos configurados" -ForegroundColor Green

# 5. Crear carpeta de logs
$logsPath = Join-Path $publishPath "logs"
if (!(Test-Path $logsPath)) {
    New-Item -ItemType Directory -Path $logsPath -Force
    Write-Host "   Carpeta de logs creada" -ForegroundColor Green
}

# 6. Reiniciar IIS
Write-Host "5. Reiniciando IIS..." -ForegroundColor Yellow
iisreset /restart

Write-Host "`n=== Publicación completada ===" -ForegroundColor Green
Write-Host "La aplicación está disponible en: http://localhost:$port" -ForegroundColor Cyan
Write-Host "`nNOTA: Asegúrese de que:" -ForegroundColor Yellow
Write-Host "- ASP.NET Core Hosting Bundle está instalado" -ForegroundColor White
Write-Host "- El usuario del Application Pool tiene permisos para conectarse a Oracle" -ForegroundColor White
Write-Host "- La autenticación Windows está habilitada en IIS" -ForegroundColor White
