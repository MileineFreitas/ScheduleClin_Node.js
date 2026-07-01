@echo off
setlocal
set XAMPP=C:\Users\Acer\Documents\GitHub\xampp\xampp

call "%~dp0start-mysql.bat"
if errorlevel 1 exit /b 1

cd /d "%~dp0.."
echo.
echo Iniciando ScheduleClin...
npm run dev
