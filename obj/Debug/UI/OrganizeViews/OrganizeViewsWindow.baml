   M S B A M L   `   `   `  ����F  BBeyondRevit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:BeyondRevit.UIBeyondRevit.UI  5      (    "BeyondRevit.UI.OrganizeViewsWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      .localclr-namespace:BeyondRevit.UI     5      -   5        $
��1200��5      $	��600��6,     � WindowStartupLocation$  CenterOwner=�69    � Title$ Organize Views��6]      MainBackground#���   6�    � 	Resources 5       �� MergedDictionaries �� Source�� 5	       5
      �� 5      $H Bpack://application:,,,/BeyondRevit;component/Resources/Styles.xaml?�6   %    5      .��5      �  � RowDefinitions 5      �� 5      �� 5      $��60 �6   .+�5      &   System.Windows.Controls.DataGrid  $��0��6   $��10q�6      AutoGenerateColumns .    DataGrid#��[ 5      ��5      �� *Views+   Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ Original View Name��6   	   
IsReadOnly	 . 
   Width5  /System.Windows.Controls.DataGridLengthConverter$
 * 5   
      
Foreground
 �����1  +System.Windows.Controls.DataGridBoundColumn   Binding 5   
   ��  �� Path$ ViewName� �� Mode$ OneTime=�5   
     5      $ New View Name��6   	 .  $
 * 5"   
    5!   
   �� $ ViewName�$ TwoWay=�5"   
     5#      $ 
View Title��6   	 .  $
 * 5&   
    5%   
   �� $ 	ViewTitle�$ TwoWay=�5&   
     5'      $ View Number��6   	 .  $
 * 5*   
    5)   
   �� $ 
ViewNumber�$ TwoWay=�5*   
   5,      �� 5-      -   5        $��1��5-        RoundButton#��[ 6   $��Submit��6=   $��
0,10,10,10q�6N   $��Right=�6b   $	��120��6~   6�   �� 5.      -   5        $��1��5.      #��[ 6   $��Check Sheet��6=   $��
10,10,0,10q�6S   $
��Left=�6y   $y�Hidden=�6�   $	��120��6�   5/      50      