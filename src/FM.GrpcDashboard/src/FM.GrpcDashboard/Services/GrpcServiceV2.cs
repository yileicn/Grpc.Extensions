using Google.Protobuf.Reflection;
using Grpc;
using Grpc.Core;
using Grpc.Core.Utils;
using Grpc.Reflection.V1Alpha;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.GrpcDashboard.Services
{
    public class GrpcServiceV2 : IGrpcReflection
    {
        private static ConcurrentDictionary<string, MethodInfoRS> dicInfoMethods = new ConcurrentDictionary<string, MethodInfoRS>();
        private readonly ILogger<GrpcService> _logger;

        public GrpcServiceV2(ILogger<GrpcService> logger)
        {
            this._logger = logger;
        }
        public async Task<InfoRS> GetInfo(string address, int port)
        {
            var result = new InfoRS();
            var channel = new Channel(address, port, ChannelCredentials.Insecure);
            try
            {
                var client = new ServerReflection.ServerReflectionClient(channel);
                var resp = client.ServerReflectionInfo();
                await resp.RequestStream.WriteAsync(new ServerReflectionRequest() { ListServices = "" });
                await resp.ResponseStream.ForEachAsync(async res =>
                {
                    switch (res.MessageResponseCase.ToString())
                    {
                        case "ListServicesResponse":
                            await ListServices(resp.RequestStream, res.ListServicesResponse);
                            break;
                        case "FileDescriptorResponse":
                            ListMethods(res.FileDescriptorResponse, result);
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw ex;
            }
            finally
            {
                await channel.ShutdownAsync();
            }
            return result;
        }

        private async Task ListServices(IClientStreamWriter<ServerReflectionRequest> req, ListServiceResponse resp)
        {
            var services = resp.Service.Where(p => !p.Name.StartsWith("grpc."));
            foreach (var service in services)
            {
                await req.WriteAsync(new ServerReflectionRequest() { FileContainingSymbol = service.Name });
            }
            await req.CompleteAsync();
        }

        private void ListMethods(FileDescriptorResponse resp, InfoRS infoRs)
        {
            byte[] bytes = new byte[resp.FileDescriptorProto.First().Length];
            resp.FileDescriptorProto.First().CopyTo(bytes, 0);
            var fdp = ProtobufExtensions.Deserialize<FileDescriptorProto>(bytes);
            foreach (var service in fdp.Services)
            {
                foreach (var method in service.Methods)
                {
                    var name = $"{fdp.Package}.{service.Name}/{method.Name}";
                    infoRs.MethodInfos.Add(new MethodInfo()
                    {
                        Name = name
                    });
                    var inputTypeName = method.InputType.Split('.').Last();
                    var outputTypeName = method.OutputType.Split('.').Last();
                    var intputType = fdp.MessageTypes.First(p => p.Name == inputTypeName);
                    var outputType = fdp.MessageTypes.First(p => p.Name == outputTypeName);
                    var infoMethodRS = new MethodInfoRS()
                    {
                        RequestJson = intputType.Fields.ToDictionary(p => p.Name, p => GetFiledDefaultValue(p, fdp.MessageTypes)).ToJson(),
                        ResponseJson = outputType.Fields.ToDictionary(p => p.Name, p => GetFiledDefaultValue(p, fdp.MessageTypes)).ToJson()
                    };
                    dicInfoMethods.AddOrUpdate(name, infoMethodRS, (k, v) => infoMethodRS);
                }
            }
        }

        private dynamic GetFiledDefaultValue(FieldDescriptorProto field, List<DescriptorProto> messageTypes)
        {
            switch (field.type)
            {
                case FieldDescriptorProto.Type.TypeDouble:
                case FieldDescriptorProto.Type.TypeFloat:
                case FieldDescriptorProto.Type.TypeInt64:
                case FieldDescriptorProto.Type.TypeUint64:
                case FieldDescriptorProto.Type.TypeInt32:
                case FieldDescriptorProto.Type.TypeFixed64:
                case FieldDescriptorProto.Type.TypeFixed32:
                case FieldDescriptorProto.Type.TypeUint32:
                case FieldDescriptorProto.Type.TypeEnum:
                case FieldDescriptorProto.Type.TypeSfixed32:
                case FieldDescriptorProto.Type.TypeSfixed64:
                case FieldDescriptorProto.Type.TypeSint32:
                case FieldDescriptorProto.Type.TypeSint64:
                    return 0;
                case FieldDescriptorProto.Type.TypeBool:
                    return true;
                case FieldDescriptorProto.Type.TypeString:
                case FieldDescriptorProto.Type.TypeBytes:
                case FieldDescriptorProto.Type.TypeGroup:
                    return "";
                case FieldDescriptorProto.Type.TypeMessage:
                    var typeName = field.TypeName.Split('.').Last();
                    var type = messageTypes.FirstOrDefault(p => p.Name == typeName);
                    if (type != null)
                        return type.Fields.ToDictionary(p => p.Name, p => GetFiledDefaultValue(p, messageTypes));
                    else
                        return "";
                default:
                    return "";
            }
        }

        public async Task<MethodInfoRS> GetMethodInfo(string endpoint, string methodName)
        {
            if (!dicInfoMethods.TryGetValue(methodName, out var result))
            {
                var endpoints = endpoint.Split(':');
                await GetInfo(endpoints[0], endpoint[1]);
                dicInfoMethods.TryGetValue(methodName, out result);
            }
            return result;
        }

        public Task<string> MethodInvoke(string endpoint, string methodName, string requestJson, Dictionary<string, string> customHeaders)
        {
            var request = requestJson.Replace("\r\n", "");
            var metadata = new Metadata();
            foreach (var item in customHeaders)
            {
                if (!metadata.Any(p => p.Key == item.Key))
                    metadata.Add(new Metadata.Entry(item.Key, item.Value));
            }
            var sbHeader = new StringBuilder();
            foreach (var item in metadata)
            {
                sbHeader.Append($" -H '{item.Key}:{item.Value}'");
            }
            var cmd = $"grpcurl -d '{request}' {sbHeader} -plaintext {endpoint} {methodName}";
            _logger.LogInformation(cmd);
            var result = cmd.Bash();
            return Task.FromResult(result);
        }

        public Tuple<bool, string> SaveResponse(string serviceName, string methodName, bool isSaveResponse)
        {
            throw new NotImplementedException();
        }

        public Tuple<bool, string> Throttle(string serviceName, string methodName, bool isThrottle)
        {
            throw new NotImplementedException();
        }
    }
}
