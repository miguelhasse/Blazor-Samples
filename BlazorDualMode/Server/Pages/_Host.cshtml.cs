using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlazorDualMode.Server.Pages
{
    public class HostModel : PageModel
    {
        public RenderMode RenderMode { get; private set; }

        public string HostingMode { get; private set; }

        public bool Prerendered => RenderMode == RenderMode.ServerPrerendered || RenderMode == RenderMode.WebAssemblyPrerendered;

        public void OnGet()
        {
            var mode = new[]
            {
                (string)Request.Query["blazor-mode"],
                Environment.GetEnvironmentVariable("ASPNETCORE_BLAZOR_MODE")
            }
            .Select(option => Enum.TryParse(option, true, out RenderMode mode) ? (RenderMode?)mode : null)
            .FirstOrDefault(option => option.HasValue);

            RenderMode = mode ?? RenderMode.WebAssemblyPrerendered;
            HostingMode = ((int)RenderMode < 4) ? "server" : "webassembly";
        }
    }
}
