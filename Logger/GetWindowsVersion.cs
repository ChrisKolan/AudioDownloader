using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public static class GetWindowsVersion
    {
        public static string OsVersion 
        { 
            get
            {
                return Environment.OSVersion.ToString();
            }
        }
        public static string ReleaseId
        {
            get
            {
                return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString();
            }
        }
    }
}
