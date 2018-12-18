## Grpc.Extesions
一个基于GRPC的简单微服务框架 

## 功能
- 服务注册和发现
- 服务自动负载均衡(轮询，随机)
- 服务端中件间(性能监控[日志],全局错误处理,手动熔断)
- 客户端中件间(认证，超时时间设置)
- Grpc DashBoard(Http远程调用，手动熔断，日志输出控制)
- Grpc CodeFirst

## 待完善
- 使用Polly实现重试，降级，熔断

## 依赖的技术栈
-  [dotnet standard 2.0]()
-  [gRPC - An RPC library and framework](https://github.com/grpc/grpc)
-  [Consul - Service Discovery and Configuration Made Easy](https://consul.io)
-  [Polly - Polly is a .NET resilience and transient-fault-handling library](https://github.com/App-vNext/Polly)
-  [OpenTracing(Jaeger) - a Distributed Tracing System](https://github.com/jaegertracing/jaeger)

## 感谢
感谢以下的项目,排名不分先后