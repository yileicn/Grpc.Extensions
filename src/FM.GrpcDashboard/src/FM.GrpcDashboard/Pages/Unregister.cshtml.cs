using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

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
        public async Task<IActionResult> OnPostAsync(string serviceName)
        {
            ViewData["HttpMethod"] = "post";
            ViewData["ServiceName"] = serviceName;
            await _consulSrv.UnRegService(serviceName);
            return Page();
        }
    }
}
