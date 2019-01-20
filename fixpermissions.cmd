@echo off
rem this is required so that .NET's permission system does not cause Builder.exe to crash.
rem http://superuser.com/questions/659787/reset-file-and-folder-permission-of-external-hard-drive-data-to-default-in-windo

takeown /F . /R /D Y
icacls "." /reset /T
