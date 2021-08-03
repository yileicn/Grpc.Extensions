using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace FM.GrpcDashboard.Pages
{
    public class ConsulSetModel : PageModel
    {
        ConsulService _consulSrv;
        IConfiguration _config;
        public ConsulSetModel(ConsulService consulSrv, IConfiguration config)
        {
            _consulSrv = consulSrv;
            _config = config;
        }

        public async Task OnGet()
        {
            ViewData["addr"] = _config["Consul"];
            ViewData["Nodes"] = await GetNodes(_config["Consul"]);
        }

        public async Task<SelectList> GetNodes(string selected)
        {
            var nodes = await _consulSrv.GetAllNode();
            var selectList = nodes.Select(p => new SelectListItem() { Value = "http://" + p.Address + ":8500/", Text = p.Name }).ToList();
            return new SelectList(selectList, "Value", "Text", selected);

        }

        public void OnPost(string addr = null)
        {
            if (!string.IsNullOrWhiteSpace(addr))
            {
                ViewData["HttpMethod"] = "post";
                ViewData["addr"] = addr;
                ViewData["Nodes"] = GetNodes(addr);
                _config["Consul"] = addr;
            }
        }
    }
}