#!/bin/bash

cd "$(dirname "$0")"
clear

find . -type d -name \* -exec chmod 775 {} \;
find . -type f -name \* -exec chmod 644 {} \;
chmod +x $0

msbuild /nologo /p:Configuration=Release /p:Platform="Any CPU" /p:DebugSymbols=false proj/LibGradesTable.csproj
if [ $? -eq 0 ]; then
    msbuild /nologo /p:Configuration=Release /p:Platform="Any CPU" /p:DebugSymbols=false proj/LibGradesCli.csproj
    if [ $? -eq 0 ]; then
        msbuild /nologo /p:Configuration=Release /p:Platform="Any CPU" /p:DebugSymbols=true proj/GradeCalc.csproj
        if [ $? -eq 0 ]; then
            read -n1 -r -p "Compilation successful. Press anything..." key
        fi
    fi
fi

