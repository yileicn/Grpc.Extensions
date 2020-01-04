NUGET_ADDR = http://192.168.8.11:16969
NUGET_ADDR_PROD = https://api.nuget.org/v3/index.json
NUGET_FILES=`ls ./nupkgs/*.nupkg`

build:clean 
	-dotnet pack ./src/Grpc.Extension.Abstract/Grpc.Extension.Abstract.csproj -c Release -o ./../../nupkgs
	-dotnet pack ./src/Grpc.Extension.Discovery/Grpc.Extension.Discovery.csproj -c Release -o ./../../nupkgs
	-dotnet pack ./src/Grpc.Extension.Common/Grpc.Extension.Common.csproj -c Release -o ./../../nupkgs
	-dotnet pack ./src/Grpc.Extension.Client/Grpc.Extension.Client.csproj -c Release -o ./../../nupkgs
	-dotnet pack ./src/Grpc.Extension/Grpc.Extension.csproj -c Release -o ./../../nupkgs

push-dev:build
	#method one
	#-dotnet nuget push ./nupkgs/*.nupkg -k ${NUGET_KEY} -s ${NUGET_ADDR}
	#-dotnet nuget push ./nupkgs/**/*.nupkg -k ${NUGET_KEY} -s ${NUGET_ADDR}
	
	#method two
	for file in $(NUGET_FILES);\
	do \
	    echo $$file;\
	    dotnet nuget push $$file -k ${NUGET_KEY} -s ${NUGET_ADDR};\
    done;
	
push-prod:build
	for file in $(NUGET_FILES);\
	do \
	    echo $$file;\
	    dotnet nuget push $$file -k ${NUGET_KEY_PROD} -s ${NUGET_ADDR_PROD};\
    done;

clean:
	-rm -rf nupkgs

test:
	echo ${HOME_PATA}