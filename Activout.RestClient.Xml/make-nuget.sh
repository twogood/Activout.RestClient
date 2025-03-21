#!/bin/sh
cd "$(dirname "$0")" || exit 1
VERSION=$1
if [ -z "$VERSION" ]; then
	echo "Missing version on command line!" >&2
	exit 1
fi
dotnet pack \
	-p:Title="Activout Rest Client - XML Support" \
	-p:Description="XML Support for Activout.RestClient." \
	-p:PackageVersion="$VERSION" \
	-p:PackageLicenseExpression="MIT" \
	-p:PackageProjectUrl="https://github.com/twogood/Activout.RestClient" \
	-p:RepositoryType="git" \
	-p:RepositoryUrl="https://github.com/twogood/Activout.RestClient.git" \
	--configuration=Release \
	--include-symbols \
	--include-source \
	./Activout.RestClient.Xml.csproj

echo "Upload:"
echo "https://www.nuget.org/packages/manage/upload"

ls -1 "$(pwd)"/bin/Release/*."$VERSION".*nupkg

