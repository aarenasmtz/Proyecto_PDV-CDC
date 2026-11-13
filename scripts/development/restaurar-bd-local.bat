@echo off
setlocal
title Restaurar Base de Datos PR a Local - WPC Bajio

echo =========================================================================
echo   Sincronizacion y Restauracion de BD de Produccion (PR) a Local (DEV)
echo =========================================================================
echo.

:: 1. Comprobar elevacion de Administrador
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [INFO] Solicitando permisos de Administrador para iniciar SQL Server...
    powershell -NoProfile -Command "Start-Process cmd -ArgumentList '/c \"\"%~f0\"\"' -Verb RunAs"
    exit /b
)

:: 2. Iniciar servicio SQL Server
echo [1/2] Iniciando servicio SQL Server (MSSQLSERVER)...
sc.exe config MSSQLSERVER obj= LocalSystem >nul 2>&1
net start MSSQLSERVER >nul 2>&1

:: 3. Ejecutar restauracion
echo [2/2] Restaurando base de datos PosLambrinDb desde el archivo local .bak...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0sync-db-from-vps.ps1" -SkipDownload

echo.
echo =========================================================================
echo   Proceso finalizado.
echo =========================================================================
pause

