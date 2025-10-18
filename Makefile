# Makefile for building all NuGet packages (excluding .Test projects)

NUGET_OUT ?= ./nupkgs
VERSION ?=

ifeq ($(strip $(VERSION)),)
$(error VERSION is not set. Usage: make VERSION=1.2.3)
endif

all: restclient-nuget json-nuget newtonsoft-nuget xml-nuget

restclient-nuget:
	@echo "Packing Activout.RestClient..."
	dotnet pack \
		-p:PackageVersion="$(VERSION)" \
		--configuration=Release \
		--include-symbols \
		--include-source \
		-o $(NUGET_OUT) \
		Activout.RestClient/Activout.RestClient.csproj

json-nuget:
	@echo "Packing Activout.RestClient.Json..."
	dotnet pack \
		-p:PackageVersion="$(VERSION)" \
		--configuration=Release \
		--include-symbols \
		--include-source \
		-o $(NUGET_OUT) \
		Activout.RestClient.Json/Activout.RestClient.Json.csproj

newtonsoft-nuget:
	@echo "Packing Activout.RestClient.Newtonsoft.Json..."
	dotnet pack \
		-p:PackageVersion="$(VERSION)" \
		--configuration=Release \
		--include-symbols \
		--include-source \
		-o $(NUGET_OUT) \
		Activout.RestClient.Newtonsoft.Json/Activout.RestClient.Newtonsoft.Json.csproj

xml-nuget:
	@echo "Packing Activout.RestClient.Xml..."
	dotnet pack \
		-p:PackageVersion="$(VERSION)" \
		--configuration=Release \
		--include-symbols \
		--include-source \
		-o $(NUGET_OUT) \
		Activout.RestClient.Xml/Activout.RestClient.Xml.csproj

clean:
	rm -rf $(NUGET_OUT)

.PHONY: all clean restclient-nuget json-nuget newtonsoft-nuget xml-nuget
