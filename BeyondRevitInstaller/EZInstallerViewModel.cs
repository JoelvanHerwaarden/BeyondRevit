using EZInstaller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace EZInstaller
{
    public class EZInstallerViewModel
    {
        public List<string> ProgramsThatShouldBeClosed { get; set; }
        public string ProgramName { get; set; }
        public string Disclaimer { get; set; }
        public bool ShowDisclaimer { get; set; }
        public List<InternalAction> InternalActions { get; set; }
        public EZInstallerViewModel(string programName, string disclaimer, bool showDisclaimer, List<string> programsThatShouldBeClosed)
        {
            InternalActions = CreateInternalActions();
            ProgramName = programName;
            Disclaimer = disclaimer;
            ShowDisclaimer = showDisclaimer;
            ProgramsThatShouldBeClosed = programsThatShouldBeClosed;
        }
        private List<InternalAction> CreateInternalActions()
        {
            Dictionary<string, string> resourceNames = Utils.GetResourceNames();
            List<InternalAction> result = new List<InternalAction>();
            string[] lines = Utils.ReadInstallationConfiguration();
            foreach(string s in lines)
            {
                if (s != string.Empty)
                {
                    string[] attr = s.Split(new List<string>() { " => " }.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    string resourceName = attr[0];
                    string targetFolder = Path.GetDirectoryName(attr[1]);
                    string targetFileName = Path.GetFileName(attr[1]);
                    InternalAction action = new InternalAction(resourceNames[attr[0]], targetFolder, targetFileName);
                    result.Add(action);
                }
            }
            return result;
        }
        public async Task RunActions(IProgress<double> progress, IProgress<string> progressName)
        {
            double step = 100/this.InternalActions.Count;
            double percentage = 0;
            foreach(InternalAction action in InternalActions)
            {
                await Task.Run(() => action.Install());
                percentage += step;
                progress.Report(percentage);
                progressName.Report(action.TargetFileName);
                Thread.Sleep(10);
            }
        }
        public bool CanRunInstaller()
        {
            bool result = true;
            foreach(string program in this.ProgramsThatShouldBeClosed)
            {
                Process[] pname = Process.GetProcessesByName(program);
                if (pname.Length != 0)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }
        public bool KillPrograms()
        {
            bool result = true;
            foreach (string program in this.ProgramsThatShouldBeClosed)
            {
                Process[] pname = Process.GetProcessesByName(program);
                if (pname.Length != 0)
                {
                    foreach(Process p in pname)
                    {
                        p.Kill();
                    }
                }
            }
            return result;
        }
    }
}
