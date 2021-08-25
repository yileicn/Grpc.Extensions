using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Grpc;
using FM.GrpcDashboard.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace FM.GrpcDashboard.Pages
{
    public class InvokeModel : PageModel
    {
        public MethodInfoRS MethodInfoRS { get; set; }

        [BindProperty]
        public string Endpoint { get; set; }

        [BindProperty]
        public string MethodName { get; set; }

        [BindProperty]
        public string RequestJson { get; set; }

        [BindProperty]
        public string CustomHeaders { get; set; }

        GrpcServiceProxy _grpcSrv;
        private IConfiguration _conf;
        public InvokeModel(GrpcServiceProxy grpcSrv, IConfiguration conf)
        {
            _grpcSrv = grpcSrv;
            _conf = conf;
        }

        public async Task<IActionResult> OnGet(string endpoint, string methodName)
        {
            Endpoint = endpoint?.Trim();
            MethodName = methodName?.Trim();
            if (string.IsNullOrWhiteSpace(Endpoint) || string.IsNullOrWhiteSpace(MethodName))
            {
                return RedirectToPage("Error", new { msg = "服务地址和要调用的服务方法名称不能为空" });
            }
            MethodInfoRS = await _grpcSrv.GetMethodInfo(Endpoint, MethodName);
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var dicCustomHeaders = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(CustomHeaders))
            {
                try
                {
                    dicCustomHeaders = CustomHeaders.FromJson<Dictionary<string, string>>();
                }
                catch (Exception ex)
                {
                    return RedirectToPage("Error", new { msg = $"自定义头json格式错误:{ex}" });
                }
            }
            var res = await _grpcSrv.MethodInvoke(Endpoint, MethodName, RequestJson, dicCustomHeaders);
            return new JsonResult(new { respJson = res });
        }
    }
}