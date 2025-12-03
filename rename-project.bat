@echo off
REM SmartWorkz.StarterKitMVC - Project Rename Script (Batch)
REM Usage: rename-project.bat NewCompany NewProject

if "%~1"=="" (
    echo Usage: rename-project.bat NewCompany NewProject
    exit /b 1
)
if "%~2"=="" (
    echo Usage: rename-project.bat NewCompany NewProject
    exit /b 1
)

set NEW_COMPANY=%~1
set NEW_PROJECT=%~2
set OLD_COMPANY=SmartWorkz
set OLD_PROJECT=StarterKitMVC

echo Renaming from %OLD_COMPANY%.%OLD_PROJECT% to %NEW_COMPANY%.%NEW_PROJECT%...

REM Call PowerShell script for actual work
powershell -ExecutionPolicy Bypass -File "%~dp0rename-project.ps1" -NewCompany "%NEW_COMPANY%" -NewProject "%NEW_PROJECT%"

echo Done!
pause
