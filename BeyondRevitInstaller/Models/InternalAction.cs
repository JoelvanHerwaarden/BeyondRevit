using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EZInstaller.Models
{
    public class InternalAction
    {
        /// <summary>
        /// Name of the Resource
        /// </summary>
        public string ResourceName { get; private set; }
        public string TargetFolder { get; private set; }
        public string TargetFileName { get; private set; }
        public Utils.ActionType ActionType { get; private set; }

        public InternalAction(string resourceName, string targetFolder, string targetFileName, Utils.ActionType actionType = Utils.ActionType.Install)
        {
            ResourceName = resourceName;
            TargetFolder = targetFolder;
            TargetFileName = targetFileName;
            ActionType = actionType;
        }

        public void Install()
        {
            string resourceName = this.ResourceName;
            string fileName = Path.Combine(this.TargetFolder, this.TargetFileName);
            string directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }

    }
}
