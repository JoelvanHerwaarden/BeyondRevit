﻿#pragma checksum "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "445F942AA89E46F0B8EBEAFD9D480320E585ED412FED0C52020C5544C8F98486"
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
    /// GenericDropdownWindow
    /// </summary>
    public partial class GenericDropdownWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 27 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label InstructionLabel;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox SearchBox;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox ItemNamesListBox;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SelectAllButton;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SelectNoneButton;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
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
            System.Uri resourceLocater = new System.Uri("/BeyondRevit;component/ui/genericdropdown/genericdropdownwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
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
            this.InstructionLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.SearchBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 28 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
            this.SearchBox.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.SearchBox_TextChanged);
            
            #line default
            #line hidden
            
            #line 28 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
            this.SearchBox.GotKeyboardFocus += new System.Windows.Input.KeyboardFocusChangedEventHandler(this.SearchBox_GotFocus);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ItemNamesListBox = ((System.Windows.Controls.ListBox)(target));
            return;
            case 4:
            this.SelectAllButton = ((System.Windows.Controls.Button)(target));
            
            #line 30 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
            this.SelectAllButton.Click += new System.Windows.RoutedEventHandler(this.SelectAll_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.SelectNoneButton = ((System.Windows.Controls.Button)(target));
            
            #line 31 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
            this.SelectNoneButton.Click += new System.Windows.RoutedEventHandler(this.SelectNone_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.SubmitButton = ((System.Windows.Controls.Button)(target));
            
            #line 32 "..\..\..\..\UI\GenericDropdown\GenericDropdownWindow.xaml"
            this.SubmitButton.Click += new System.Windows.RoutedEventHandler(this.SubmitButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
