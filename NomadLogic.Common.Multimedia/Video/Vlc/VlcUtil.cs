using System;
using System.IO;
using System.Reflection;
using NomadLogic.Common.Util;
using Vlc.DotNet.Forms;

namespace NomadLogic.Multimedia.Video.Vlc
{
    public static class VlcUtil
    {
        public static void GetLibDirectory(object sender, VlcLibDirectoryNeededEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();

            if (currentAssembly == null || currentAssembly.Location == null)
            {
                throw new SystemException("Could not get entry assembly.");
            }

            var basePath = DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.Nomad) +
                           "lib" + Path.DirectorySeparatorChar +
                           "vlc_dot_net" + Path.DirectorySeparatorChar;
            
            var result = AssemblyName.GetAssemblyName(currentAssembly.Location).ProcessorArchitecture == ProcessorArchitecture.X86 ? 
                new DirectoryInfo(Path.Combine(basePath, @"x86\")) : new DirectoryInfo(Path.Combine(basePath, @"x64\"));
            
            if (!result.Exists)
            {
                throw new SystemException("Could not find VLC libs directory: " + result.FullName);
            }

            if (e != null) e.VlcLibDirectory = result;
        }
    }
}