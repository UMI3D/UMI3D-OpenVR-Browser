﻿using System;
using System.Runtime.InteropServices;

namespace WkHtmlToPdfDotNet
{
    internal static class ModuleFactory
    {
        public static IWkHtmlModule GetModule()
        {
#if NETSTANDARD2_0
            // Windows allows us to probe for either 64 or 86 bit versions
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
# endif
                try
                {
                    // Try 64-bit
                    VersionWin64();

                    return new WkHtmlModule();
                }
                catch
                {
                }

                try
                {
                    // Try 86-bit
                    VersionWin86();

                    return new WkHtmlModule();
                }
                catch
                {
                }
#if NETSTANDARD2_0
        }
            else
            {
                try
                {
                    return new WkHtmlModuleLinux64();
                }
                catch
                {
                }

                try
                {
                    return new WkHtmlModuleLinux86();
                }
                catch
                {
                }

                try
                {
                    return new WkHtmlModuleLinuxArm64();
                }
                catch
                {
                }
            }
#endif
            // Also try to load it with the method that should use the deps file
            try
            {
                return new WkHtmlModule();
            }
            catch
            {
            }

            throw new NotSupportedException("Unable to load native library. The platform may be missing native dependencies (libjpeg62, etc). Or the current platform is not supported.");
        }

        [DllImport(@"runtimes\win-x64\native\wkhtmltox", CharSet = CharSet.Unicode, EntryPoint = "wkhtmltopdf_version")]
        public static extern IntPtr VersionWin64();

        [DllImport(@"runtimes\win-x86\native\wkhtmltox", CharSet = CharSet.Unicode, EntryPoint = "wkhtmltopdf_version")]
        public static extern IntPtr VersionWin86();
    }
}
