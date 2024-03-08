﻿#pragma checksum "BreaklinesWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "D3A0F5CA946305961E5064C04ED6F9F96C0B31AABBAEF11C37DF8704A7819055"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using BeyondRevit.UI;
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


namespace BeyondRevit.UI {
    
    
    /// <summary>
    /// BreaklinesWindow
    /// </summary>
    public partial class BreaklinesWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 30 "BreaklinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox BreaklineFamiliesBox;
        
        #line default
        #line hidden
        
        
        #line 34 "BreaklinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox Top;
        
        #line default
        #line hidden
        
        
        #line 39 "BreaklinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox Bottom;
        
        #line default
        #line hidden
        
        
        #line 45 "BreaklinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox Left;
        
        #line default
        #line hidden
        
        
        #line 51 "BreaklinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox Right;
        
        #line default
        #line hidden
        
        
        #line 55 "BreaklinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border Viewport;
        
        #line default
        #line hidden
        
        
        #line 56 "BreaklinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SubmitButton;
        
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
            System.Uri resourceLocater = new System.Uri("/BeyondRevit;component/ui/breaklineswindow/breaklineswindow.xaml", System.UriKind.Relative);
            
            #line 1 "BreaklinesWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
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
            this.BreaklineFamiliesBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 2:
            this.Top = ((System.Windows.Controls.CheckBox)(target));
            
            #line 34 "BreaklinesWindow.xaml"
            this.Top.Checked += new System.Windows.RoutedEventHandler(this.CheckedChanged);
            
            #line default
            #line hidden
            
            #line 34 "BreaklinesWindow.xaml"
            this.Top.Unchecked += new System.Windows.RoutedEventHandler(this.CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Bottom = ((System.Windows.Controls.CheckBox)(target));
            
            #line 39 "BreaklinesWindow.xaml"
            this.Bottom.Checked += new System.Windows.RoutedEventHandler(this.CheckedChanged);
            
            #line default
            #line hidden
            
            #line 39 "BreaklinesWindow.xaml"
            this.Bottom.Unchecked += new System.Windows.RoutedEventHandler(this.CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.Left = ((System.Windows.Controls.CheckBox)(target));
            
            #line 45 "BreaklinesWindow.xaml"
            this.Left.Checked += new System.Windows.RoutedEventHandler(this.CheckedChanged);
            
            #line default
            #line hidden
            
            #line 45 "BreaklinesWindow.xaml"
            this.Left.Unchecked += new System.Windows.RoutedEventHandler(this.CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Right = ((System.Windows.Controls.CheckBox)(target));
            
            #line 51 "BreaklinesWindow.xaml"
            this.Right.Checked += new System.Windows.RoutedEventHandler(this.CheckedChanged);
            
            #line default
            #line hidden
            
            #line 51 "BreaklinesWindow.xaml"
            this.Right.Unchecked += new System.Windows.RoutedEventHandler(this.CheckedChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.Viewport = ((System.Windows.Controls.Border)(target));
            return;
            case 7:
            this.SubmitButton = ((System.Windows.Controls.Button)(target));
            
            #line 56 "BreaklinesWindow.xaml"
            this.SubmitButton.Click += new System.Windows.RoutedEventHandler(this.SubmitButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
