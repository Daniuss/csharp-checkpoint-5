@echo off
REM Script de configuracao e execucao rapida - Cadastro de Produtos (Checkpoint 5)
REM Requisito: .NET SDK 8.0 ou superior instalado (https://dotnet.microsoft.com/download)

echo ====================================
echo   Cadastro de Produtos - Setup
echo ====================================

echo.
echo [1/3] Restaurando dependencias (dotnet restore)...
dotnet restore
if %errorlevel% neq 0 goto :erro

echo.
echo [2/3] Compilando o projeto (dotnet build)...
dotnet build
if %errorlevel% neq 0 goto :erro

echo.
echo [3/3] Iniciando a aplicacao (dotnet run)...
echo (O banco de dados SQLite sera criado automaticamente em database\produtos.db)
echo.
dotnet run --project Checkpoint5.csproj
goto :fim

:erro
echo.
echo Ocorreu um erro durante a configuracao. Verifique se o .NET SDK esta instalado.
pause
exit /b 1

:fim
pause
