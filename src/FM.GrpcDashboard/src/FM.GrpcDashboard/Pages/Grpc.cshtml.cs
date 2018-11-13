using Grpc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace FM.GrpcDashboard.Pages
{
    public class GrpcModel : PageModel
    {
        public string ServiceName { get; set; }

        public List<string> AddressInfos { get; set; }

        public string CurrentAddressInfo { get; set; }

        public InfoRS Info { get; set; }

        ConsulService _consulSrv;
        GrpcService _grpcSrv;

        public GrpcModel(ConsulService consulSrv, GrpcService grpcSrv)
        {
            _consulSrv = consulSrv;
            _grpcSrv = grpcSrv;
        }

        public IActionResult OnGet(string serviceName, string serverAddress = null)
        {
            ServiceName = serviceName?.Trim();
            CurrentAddressInfo = serverAddress?.Trim();
            AddressInfos = new List<string> { CurrentAddressInfo };

            if (string.IsNullOrWhiteSpace(ServiceName) && string.IsNullOrWhiteSpace(CurrentAddressInfo))
            {
                return RedirectToPage("Error", new { msg = "请指定consul服务名称或Grpc服务地址" });
            }

            if (!string.IsNullOrWhiteSpace(serverAddress))
            {
                var arr = serverAddress.Split(':', System.StringSplitOptions.RemoveEmptyEntries);
                var ip = arr[0];
                var port = int.Parse(arr[1]);

                Info = _grpcSrv.GetInfo(ip, port).Result;
            }
            else
            {
                var service = _consulSrv.GetService(ServiceName).Result;
                if (service == null || service.Count == 0)
                {
                    return RedirectToPage("Error", new { msg = $"consul中找不到服务{ServiceName}" });
                }
                AddressInfos = service.Select(q => $"{q.Address}:{q.Port}").Distinct().ToList();
                var ip = service.First().Address;
                var port = service.First().Port;
                CurrentAddressInfo = $"{ip}:{port}";

                Info = _grpcSrv.GetInfo(ip, port).Result;
            }

            if (Info == null)
            {
                return RedirectToPage("Error", new { msg = $"consul服务({ServiceName})未集成FM.GrpcExtensions" });
            }
            return Page();
        }
        /// <summary>
        /// 截流
        /// </summary>
        public IActionResult OnPostThrottle(string serviceName, string methodName, bool isThrottled)
        {
            var res = _grpcSrv.Throttle(serviceName, methodName, isThrottled);
            return new JsonResult(new { result = res.Item1, msg = res.Item2 });
        }
        /// <summary>
        /// 保持响应
        /// </summary>
        public IActionResult OnPostSaveResponse(string serviceName, string methodName, bool saveResponseEnable)
        {
            var res = _grpcSrv.SaveResponse(serviceName, methodName, saveResponseEnable);
            return new JsonResult(new { result = res.Item1, msg = res.Item2 });
        }
    }
}