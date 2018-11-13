using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Linq;

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

        public void OnGet()
        {
            ViewData["addr"] = _config["Consul"];
            ViewData["Nodes"] = GetNodes(_config["Consul"]);
        }

        public SelectList GetNodes(string selected)
        {
            var nodes = _consulSrv.GetAllNode().Result;
            var selectList = nodes.Select(p => new SelectListItem() { Value = "http://" + p.Address + ":8500/", Text = p.Name }).ToList();
            return new SelectList(selectList, "Value", "Text", selected);

        }

        public void OnPost(string addr=null)
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