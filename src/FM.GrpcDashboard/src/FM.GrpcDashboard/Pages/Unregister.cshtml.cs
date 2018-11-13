using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FM.GrpcDashboard.Pages
{
    public class UnregisterModel : PageModel
    {
        ConsulService _consulSrv;
        public UnregisterModel(ConsulService consulSrv)
        {
            _consulSrv = consulSrv;
        }

        public IActionResult OnGetAsync()
        {
            return Page();
        }
        /// <summary>
        /// 服务反注册
        /// </summary>
        public IActionResult OnPostAsync(string serviceName)
        {
            ViewData["HttpMethod"] = "post";
            ViewData["ServiceName"] = serviceName;
            _consulSrv.UnRegService(serviceName).GetAwaiter().GetResult();
            return Page();
        }
    }
}
