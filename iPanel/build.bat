@echo off
set the7zpath="C:\Program Files\7-Zip\7z.exe"
set thepubpath=bin\Debug\net6.0\publish\
set theprojectname=iPanel

rem ��ȡ���е�pubxml�ļ�
setlocal enabledelayedexpansion
set directory="%~dp0Properties\PublishProfiles"
for /r %directory% %%F in (*.pubxml) do (
  echo ���ڱ���%%~nxF
  dotnet publish /p:PublishProfile=%%~nxF
)

echo ������ɣ����ڴ��

:tpack
if exist %the7zpath% (
  goto dopack
) else (
  echo 7z�����ڣ��޷�������밲װ7-zip��64λ�汾��C�̣�
  pause
  exit
)

:dopack
echo ����ɾ����ǰrelease�İ�
rd /s /q release_full_platform

set /p thever=�����뱾�ι����İ汾��: 

rem ��ȡ���е��ļ���
set directory=%~dp0%thepubpath%
for /d %%I in ("%directory%*") do (
  %the7zpath% x -o"%directory%%%~nxI\dist" -y Sources/webconsole.zip
  %the7zpath% a -tzip "%~dp0release_full_platform\%theprojectname%_%thever%_%%~nxI.zip" "%directory%%%~nxI\*"
  rd /s /q "%directory%%%~nxI"
)

echo ������
pause
start "" "%~dp0release_full_platform"