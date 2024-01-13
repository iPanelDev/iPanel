@echo off
set the7zpath="C:\Program Files\7-Zip\7z.exe"
set thepubpath=iPanel\bin\Debug\net6.0\publish\
set thepubprofilespath=iPanel\Properties\PublishProfiles
set theprojectname=iPanel
set thereleasepathname=%~dp0release_full_platform\

rem ��ȡ���е�pubxml�ļ�
setlocal enabledelayedexpansion
set directory="%~dp0%thepubprofilespath%"
for /r %directory% %%F in (*.pubxml) do (
  echo ���ڱ���%%~nxF
  dotnet publish iPanel/iPanel.csproj --no-self-contained /p:PublishProfile=%%~nxF
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
rd /s /q "%thereleasepathname%"

set /p thever=�����뱾�ι����İ汾��: 

rem ��ȡ���е��ļ���
set directory=%~dp0%thepubpath%
for /d %%I in ("%directory%*") do (
  %the7zpath% a -tzip "%thereleasepathname%%theprojectname%_%thever%_%%~nxI.zip" "%directory%%%~nxI\*"
  rd /s /q "%directory%%%~nxI"
)

echo ������
pause
start "" "%thereleasepathname%"