using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EZInstaller
{
    public class Utils
    {
        public  enum ActionType
        {
            Install = 0,
            Uninstall = 1,
        }
        public static string[] ReadInstallationConfiguration()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "EZInstaller.InstallationConfiguration.txt";
            string result = "";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd(); 
            }
            return result.Split( new string[] { Environment.NewLine }, StringSplitOptions.None);
        }

        public static Dictionary<string, string> GetResourceNames()
        {
            Dictionary<string, string> resources = new Dictionary<string, string>();
            foreach (string resource in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                string name = resource.Replace("EZInstaller.Resources.", "");
                resources.Add(name, resource);
            }
            return resources;
        }
    }
}
