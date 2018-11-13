using Consul;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FM.GrpcDashboard.Pages
{
    public class IndexModel : PageModel
    {
        public List<AgentService> ConsulServices { get; set; }

        ConsulService _consulSrv;
        public IndexModel(ConsulService consulSrv)
        {
            _consulSrv = consulSrv;
        }

        public IActionResult OnGetAsync(string serviceName = null)
        {
            if (!string.IsNullOrWhiteSpace(serviceName) && Regex.IsMatch(serviceName.Trim(), @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}:[0-9]{1,6}$"))
            {
                return RedirectToPage("Grpc", new { serverAddress = serviceName });
            }
            else
            {
                ViewData["ServiceName"] = serviceName;
                ConsulServices = _consulSrv.GetAllServices().Result;
                if (ConsulServices != null && !string.IsNullOrWhiteSpace(serviceName))
                {
                    ConsulServices = ConsulServices.Where(q => q.Service.ToLower().Contains(serviceName.Trim().ToLower())).OrderBy(q => q.Service).ToList();
                }

                return Page();
            }
        }
    }
}
