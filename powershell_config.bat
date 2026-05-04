@rem Check for admin rights
@fsutil dirty query %systemdrive% >nul 2>&1
@IF ERRORLEVEL 1 goto adminerror
@goto action

:adminerror
@echo The script must be run by an administrator
@pause
@goto end

:action
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell" /v ExecutionPolicy /t REG_SZ /d Unrestricted /f
reg add "HKLM\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell" /v ExecutionPolicy /t REG_SZ /d Unrestricted /f

:end