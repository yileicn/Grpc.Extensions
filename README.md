## Grpc.Extesions
一个基于GRPC的简单微服务框架

## 功能
- 服务注册和发现
    - 默认使用Consul实现
    - 实现IServiceDiscovery,IServiceRegister可扩展zk等
- 服务自动负载均衡
    - 默认使用轮询实现,可切换随机算法
    - 实现ILoadBalancer可扩展
- 服务容错
    - 服务连接报错，自动切换节点重试3次
    - 可在DashBoard上手动熔断
- 调用链跟踪
    - 根据配制来启用Jaeger调用链跟踪
- 服务网关
    - 使用Kong网关可实现服务的认证授权，协议转换(grpc转http)，服务路由，服务限流和熔断等
    - 使用Ocelot网关也可实现上述功能 
- 服务配制
    - 使用Apollo配制中心
- Grpc DashBoard监控
    - Http远程调用,手动熔断,日志输出控制
    - 待实现服务统计数据来监控
- 服务端中件间
    - 性能监控[日志,分布式调用链],全局错误处理,手动熔断
    - 实现ServerInterceptor可扩展	
- 客户端中件间
    - 性能监控[日志,分布式调用链],超时时间设置
    - 实现ClientInterceptor可扩展
- Grpc ProtoFirst
    - 通过Proto生成代码和注释
- Grpc CodeFirst
    - 通过代码生成proto和注释给第三方语言使用(GrpcMethod自动注册)
    - 代码更干净且方便扩展，例如可以在ProtoMessage上打验证特性来统一处理验证逻辑等
    - 更方便拆分GrpcService到多个类，而不是使用partial class

### NuGet Package
支持NetFramework4.6,NetCore2.1
- [Grpc.Extensions 服务端](https://www.nuget.org/packages/FM.Grpc.Extensions/)
- [Grpc.Extensions.Client 客户端](https://www.nuget.org/packages/FM.Grpc.Extensions.Client/)
- [Grpc.Extensions.Discovery 服务注册和发现](https://www.nuget.org/packages/FM.Grpc.Extensions.Discovery/)

支持asp.netcore3.0
- [Grpc.Extensions.AspNetCore 服务端](https://www.nuget.org/packages/FM.Grpc.Extensions.AspNetCore/)
- [Grpc.Extensions.Client 客户端](https://www.nuget.org/packages/FM.Grpc.Extensions.Client/)
- [Grpc.Extensions.Discovery 服务注册和发现](https://www.nuget.org/packages/FM.Grpc.Extensions.Discovery/)

### Documentation
- [Grpc ProtoFirst Demo](https://github.com/yileicn/Grpc.Extensions/tree/master/examples/Greeter)
- [Grpc CodeFirst Demo](https://github.com/yileicn/Grpc.Extensions/tree/master/examples/CodeFirst)



## 待完善
- 使用Polly实现重试，降级，熔断

## 依赖的技术栈
-  [dotnet standard 2.0]()
-  [gRPC - An RPC library and framework](https://github.com/grpc/grpc)
-  [gRPC-dotnet - gRPC for .NET](https://github.com/grpc/grpc-dotnet)
-  [Protobuf-net - Protocol Buffers library for idiomatic .NET](https://github.com/protobuf-net/protobuf-net)
-  [Consul - Service Discovery and Configuration Made Easy](https://consul.io)
-  [Polly - Polly is a .NET resilience and transient-fault-handling library](https://github.com/App-vNext/Polly)
-  [OpenTracing(Jaeger) - a Distributed Tracing System](https://github.com/jaegertracing/jaeger)

## 感谢
感谢以下的项目,排名不分先后
