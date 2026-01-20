@echo off
echo ============================================
echo   OracleDBManager - Compilacion para IIS
echo ============================================
echo.

REM Limpiar carpeta de publicacion anterior
if exist ".\publish\OracleDBManager" (
    echo Limpiando carpeta de publicacion anterior...
    rmdir /s /q ".\publish\OracleDBManager"
)

echo.
echo Compilando proyecto...
echo.

dotnet publish OracleDBManager\OracleDBManager.Web\OracleDBManager.Web.csproj -c Release -o .\publish\OracleDBManager --self-contained false -r win-x64

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: La compilacion fallo.
    pause
    exit /b 1
)

REM Crear carpeta de logs
if not exist ".\publish\OracleDBManager\logs" (
    mkdir ".\publish\OracleDBManager\logs"
)

echo.
echo ============================================
echo   Compilacion exitosa!
echo ============================================
echo.
echo Los archivos estan en: %CD%\publish\OracleDBManager
echo.
echo Copia la carpeta 'OracleDBManager' a tu servidor IIS
echo Ejemplo: C:\inetpub\wwwroot\OracleDBManager
echo.
pause
