#!/bin/bash
echo "Compiling Velo Terminal for Linux-x64..."
dotnet publish VeloTerminal.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64
