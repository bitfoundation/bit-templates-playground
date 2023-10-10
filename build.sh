#!/bin/bash
curl -L https://download.visualstudio.microsoft.com/download/pr/764f2fec-710d-490d-a341-88636bce1a8d/35fc13fc20161a7d196200d9c2c6a8f0/dotnet-sdk-8.0.100-rc.1.23463.5-linux-x64.tar.gz > dotnet.tar.gz
export DOTNET_ROOT=$HOME/.dotnet
mkdir -p "$DOTNET_ROOT"
tar zxf dotnet.tar.gz -C "$DOTNET_ROOT"
export PATH=$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH
dotnet dev-certs https --trust
dotnet tool install --global dotnet-ef --version 8.0.0-rc.1.23419.6
dotnet ef database update --project src/Server/Api/Bit.TemplatePlayground.Server.Api.csproj
dotnet build src/Client/Web/Bit.TemplatePlayground.Client.Web.csproj