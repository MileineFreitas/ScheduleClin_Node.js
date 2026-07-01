@echo off
setlocal
set XAMPP=C:\Users\Acer\Documents\GitHub\xampp\xampp
set MYSQL=%XAMPP%\mysql\bin

netstat -an | findstr ":3307.*LISTENING" >nul 2>&1
if %errorlevel%==0 (
  echo MySQL/MariaDB ja esta rodando na porta 3307.
  exit /b 0
)

echo Iniciando MariaDB do XAMPP na porta 3307...
start "XAMPP MariaDB" /MIN cmd /c "cd /d %XAMPP% && mysql\bin\mysqld.exe --defaults-file=mysql\bin\my.ini --standalone"

timeout /t 4 /nobreak >nul

netstat -an | findstr ":3307.*LISTENING" >nul 2>&1
if %errorlevel%==0 (
  echo OK - MySQL rodando em localhost:3307
  exit /b 0
)

echo ERRO - MySQL nao iniciou. Verifique mysql\data\mysql_error.log
exit /b 1
