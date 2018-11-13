using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FM.GrpcDashboard.Pages
{
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string Message { get; set; }

        public void OnGet(string msg = null)
        {
            Message = msg;
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}
