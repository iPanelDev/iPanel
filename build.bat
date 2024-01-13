@echo off
set the7zpath="C:\Program Files\7-Zip\7z.exe"
set thepubpath=iPanel\bin\Debug\net6.0\publish\
set thepubprofilespath=iPanel\Properties\PublishProfiles
set theprojectname=iPanel
set thereleasepathname=%~dp0release_full_platform\

rem 获取所有的pubxml文件
setlocal enabledelayedexpansion
set directory="%~dp0%thepubprofilespath%"
for /r %directory% %%F in (*.pubxml) do (
  echo 正在编译%%~nxF
  dotnet publish iPanel/iPanel.csproj --no-self-contained /p:PublishProfile=%%~nxF
)

echo 编译完成，正在打包

:tpack
if exist %the7zpath% (
  goto dopack
) else (
  echo 7z不存在，无法打包！请安装7-zip的64位版本到C盘！
  pause
  exit
)

:dopack
echo 正在删除以前release的包
rd /s /q "%thereleasepathname%"

set /p thever=请输入本次构建的版本号: 

rem 获取所有的文件夹
set directory=%~dp0%thepubpath%
for /d %%I in ("%directory%*") do (
  %the7zpath% a -tzip "%thereleasepathname%%theprojectname%_%thever%_%%~nxI.zip" "%directory%%%~nxI\*"
  rd /s /q "%directory%%%~nxI"
)

echo 打包完成
pause
start "" "%thereleasepathname%"