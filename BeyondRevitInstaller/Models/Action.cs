using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows;
using System.Runtime.InteropServices;

namespace EZInstaller
{
    public class Action
    {
        /// <summary>
        /// The Name of the Source File
        /// </summary>
        public string SourceFile { get; set; }
        /// <summary>
        /// The Location Where to Copy the Source File To
        /// </summary>
        public string TargetDirectory { get; set; }
        /// <summary>
        /// The Target File Name
        /// </summary>
        public string TargetFileName { get; set; }
        /// <summary>
        /// Action Type. Install or Uninstall
        /// </summary>
        public string ActionType { get; set; }

        [JsonIgnore]
        public string SourceDirectory { get { return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); } }
        [JsonIgnore]
        public string SourceFilepath
        {
            get
            {
                return Path.Combine(SourceDirectory, SourceFile);
            }
        }
        [JsonIgnore]
        public string TargetFilepath
        {
            get
            {
                if(TargetFileName != null)
                {
                    return Environment.ExpandEnvironmentVariables(Path.Combine(TargetDirectory, TargetFileName));
                }
                else
                {
                    return Environment.ExpandEnvironmentVariables(Path.Combine(TargetDirectory, SourceFile));
                }
            }
        }

        public Action(string sourceFileName, string targetFolder, string actionType = "Install", string targetFileName = null)
        {
            SourceFile = sourceFileName;
            TargetDirectory = targetFolder;
            TargetFileName = targetFileName;
            ActionType = actionType;
        }

        public async Task Execute()
        {
            if(this.ActionType.ToLower() == "uninstall")
            {
                ExecuteDelete();

            }
            else if(this.ActionType.ToLower() == "install")
            {
                ExecuteCopy();

            }
            else if(this.ActionType == null)
            {
                this.ActionType = "Install";
                ExecuteCopy();
            }
            else
            {
                MessageBox.Show(string.Format("Invalid Action type: {0}", this.ActionType));
            }
        }
        public void ExecuteCopy()
        {
            if (IsFolder(SourceFilepath))
            {
                CopyFolder(SourceFilepath);
            }
            else
            {
                string directory = Path.GetDirectoryName(TargetFilepath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.Copy(SourceFilepath, TargetFilepath, true);
                Unblock(TargetFilepath);
            }
        }

        public void ExecuteDelete()
        {
            if (IsFolder(SourceFilepath))
            {
                Directory.Delete(TargetFilepath);
            }
            else
            {
                File.Delete(TargetFilepath);
            }
        }


        #region Unblock Files
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        private bool Unblock(string fileName)
        {
            return DeleteFile(fileName + ":Zone.Identifier");
        }
        #endregion
        public bool IsFolder(string path)
        {
            if (!Path.HasExtension(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CopyFolder(string path)
        {
            string newFolderPath = TargetDirectory + path.Replace(SourceDirectory, "");
            if (!Directory.Exists(newFolderPath))
            {
                Directory.CreateDirectory(newFolderPath);
            }
            string[] files = Directory.GetFiles(path);
            foreach(string file in files)
            {
                if (IsFolder(file))
                {
                    CopyFolder(file);
                }
                else
                {
                    string subPath = file.Replace(SourceDirectory, "");
                    string target = TargetDirectory+ subPath;
                    File.Copy(file, target, true);
                    Unblock(target);
                }
            }
            string[] folders = Directory.GetDirectories(path);
            foreach(string folderpath in folders)
            {
                CopyFolder(folderpath);
            }
        }

        public bool DoesSourceExist()
        {
            bool result = false;
            if (IsFolder(SourceFilepath))
            {
                if (Directory.Exists(SourceFilepath))
                {
                    result = true;
                }
            }
            else if (File.Exists(SourceFilepath))
            {
                result = true;

            }
            return result;
        }
        public bool DoesTargetDirectoryExist()
        {
            if (Directory.Exists(TargetDirectory))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}
