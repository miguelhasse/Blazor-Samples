using System.Runtime.InteropServices;

namespace BlazorDualMode.Shared
{
    public static class RuntimeMode
    {
        public static bool IsWebAssembly = RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));
    }
}
