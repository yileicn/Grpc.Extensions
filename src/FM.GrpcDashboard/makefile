#sln init
PROJECT = FM.GrpcDashboard
SOLUTION_NAME = ${PROJECT_NAME}.sln

#service build
SRV_NAME = cs-grpc_dashboard-web
CURRENT_VERSION := `head -1 CHANGELOG `
TIME = $(shell date +%y%m%d%H%M)

SRV_FULL_NAME := $(SRV_NAME):$(CURRENT_VERSION).$(TIME)

build:clean
	dotnet publish ./src/$(PROJECT)/$(PROJECT).csproj -c Release -o ./dist

clean:
	rm -rf dist/

# 仅编译镜像，不推
build-image:build
	cp Dockerfile ./dist/Dockerfile 
	docker build -f ./dist/Dockerfile --no-cache --rm -t $(SRV_FULL_NAME) ./dist

# 发布项目
publish:build-image
	docker tag $(SRV_FULL_NAME) $(DH_URL)$(SRV_FULL_NAME)
	docker login -u $(DH_USER) -p $(DH_PASS) $(DH_URL) && docker push $(DH_URL)$(SRV_FULL_NAME)

test:	
	echo $(DH_USER)
	echo $(DH_PASS)
	echo $(DH_URL)

.PHONY: gen build build-image publish clean test

