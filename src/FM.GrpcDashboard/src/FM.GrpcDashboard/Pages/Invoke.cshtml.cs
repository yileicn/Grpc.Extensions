using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Grpc;

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

        GrpcService _grpcSrv;

        public InvokeModel(GrpcService grpcSrv)
        {
            _grpcSrv = grpcSrv;
        }

        public IActionResult OnGet(string endpoint, string methodName)
        {
            Endpoint = endpoint?.Trim();
            MethodName = methodName?.Trim();
            if (string.IsNullOrWhiteSpace(Endpoint) || string.IsNullOrWhiteSpace(MethodName))
            {
                return RedirectToPage("Error", new { msg = "服务地址和要调用的服务方法名称不能为空" });
            }
            MethodInfoRS = _grpcSrv.GetMethodInfo(Endpoint, MethodName);
            return Page();
        }

        public IActionResult OnPost()
        {
            var res = _grpcSrv.MethodInvoke(Endpoint, MethodName, RequestJson);
            return new JsonResult(new { respJson = res });
        }
    }
}