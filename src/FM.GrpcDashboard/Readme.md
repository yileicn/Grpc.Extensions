# 使用两种方法对接DashBoard
## 方法1：实现FMGrpcExtensions.proto协议
### dotnet对接GrpcDashBoard
使用FM.Grpc.Extensions或者FM.Droplet

### 其它语言对接GrpcDashBoard
需要自己实现FMGrpcExtensions.proto协议

## 方法2：使用GrpcReflection
### dotnet对接GrpcDashBoard
netcore1.x-2.x
https://github.com/grpc/grpc/blob/master/doc/csharp/server_reflection.md
netcore3.x-5.x
https://docs.microsoft.com/en-us/aspnet/core/grpc/test-tools?view=aspnetcore-3.0

### java对接GrpcDashBoard
https://github.com/grpc/grpc-java/blob/master/documentation/server-reflection-tutorial.md

### golang对接GrpcDashBoard
https://github.com/grpc/grpc-go/blob/master/Documentation/server-reflection-tutorial.md

### python对接GrpcDashBoard
https://github.com/grpc/grpc/blob/master/doc/python/server_reflection.md
