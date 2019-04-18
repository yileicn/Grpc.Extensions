NUGET_KEY = joycoding!@12
NUGET_ADDR = http://192.168.8.11:16969/
PROJECT_NAME = Grpc.Extension

#TOOLS_PATH = find ~/.nuget/ -name "grpc_csharp_plugin*"
#PROTOC_CS_PLUGIN = ${HOME}/.nuget/packages/grpc.tools/1.17.1/tools/macosx_x64/grpc_csharp_plugin

build:clean 
	-dotnet build ./${PROJECT_NAME}.csproj -c Release
	-dotnet pack ./${PROJECT_NAME}.csproj -c Release

push:build
	-dotnet nuget push ./bin/Release/*.nupkg -k ${NUGET_KEY} -s ${NUGET_ADDR}

clean:
	-rm -rf bin
	-rm -rf obj

test:
	echo ${HOME_PATA}