using System;
using System.Runtime.InteropServices;

namespace IitOtdrLibrary
{
    public static class Native
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string libname);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetDllDirectory(string lpPathName);
    }
}