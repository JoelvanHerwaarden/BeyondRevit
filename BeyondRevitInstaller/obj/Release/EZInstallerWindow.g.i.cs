﻿#pragma checksum "..\..\EZInstallerWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "FEC65C91D6C55597A7AB991002966E232B0D6D06A422B81D20D332B1634ABE39"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using EZInstaller;
using EZInstaller.Views;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace EZInstaller {
    
    
    /// <summary>
    /// EZInstallerWindow
    /// </summary>
    public partial class EZInstallerWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 29 "..\..\EZInstallerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label startLabel;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\EZInstallerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label licenseLabel;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\EZInstallerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label installationLabel;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\EZInstallerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label finishLabel;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\EZInstallerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button NextButton;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\EZInstallerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button KillProgramsButton;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\EZInstallerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal EZInstaller.Views.StartView StartView;
        
        #line default
        #line hidden
        
        
        #line 58 "..\..\EZInstallerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal EZInstaller.Views.LicenseView LicenseView;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\EZInstallerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal EZInstaller.Views.InstallationView InstallationView;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/BeyondRevitInstaller;component/ezinstallerwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\EZInstallerWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.startLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.licenseLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 3:
            this.installationLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.finishLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 5:
            this.NextButton = ((System.Windows.Controls.Button)(target));
            
            #line 44 "..\..\EZInstallerWindow.xaml"
            this.NextButton.Click += new System.Windows.RoutedEventHandler(this.NextButton_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.KillProgramsButton = ((System.Windows.Controls.Button)(target));
            
            #line 56 "..\..\EZInstallerWindow.xaml"
            this.KillProgramsButton.Click += new System.Windows.RoutedEventHandler(this.KillProgramsButton_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.StartView = ((EZInstaller.Views.StartView)(target));
            return;
            case 8:
            this.LicenseView = ((EZInstaller.Views.LicenseView)(target));
            return;
            case 9:
            this.InstallationView = ((EZInstaller.Views.InstallationView)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

