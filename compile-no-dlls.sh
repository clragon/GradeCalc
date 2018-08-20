#!/bin/bash

cd "$(dirname "$0")"
clear

find . -type d -name \* -exec chmod 775 {} \;
find . -type f -name \* -exec chmod 644 {} \;
find . -type f -iname "*.sh" -exec chmod +x {} \;

msbuild /nologo /p:Configuration=Release /p:Platform="Any CPU" /p:DebugSymbols=true proj/GradeCalc_no_dll.csproj
if [ $? -eq 0 ]; then
    read -n1 -r -p "Compilation successful. Press anything..." key
fi

