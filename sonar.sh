#!/bin/sh
set -e
cd `dirname $0`
. ./.sonar.env
dotnet /opt/sonar-scanner-msbuild-4.3.1.1372-netcoreapp2.0/SonarScanner.MSBuild.dll begin /k:"Activout.RestClient" /d:sonar.organization="twogood-github" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="$SONAR_LOGIN"
dotnet build
dotnet /opt/sonar-scanner-msbuild-4.3.1.1372-netcoreapp2.0/SonarScanner.MSBuild.dll end /d:sonar.login="$SONAR_LOGIN"
