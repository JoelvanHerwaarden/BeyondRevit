   M S B A M L   `   `   `  ����F  BBeyondRevit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:BeyondRevit.UIBeyondRevit.UI  5   	   &     BeyondRevit.UI.CommandLineWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   .localclr-namespace:BeyondRevit.UI     5   	   -   5          �Name$  
MainWindow��5       � Title$ Console��6	   $	��500��6-   $	��500��6;   $��131.333��6J    � WindowStartupLocation$ CenterScreen=�6Z      MainBackground#���   6�    � ShowInTaskbar .   � 	Resources 5	       �� MergedDictionaries �� Source�� 5
   
    5      �� 5      $H Bpack://application:,,,/BeyondRevit;component/Resources/Styles.xaml?�6%   %    5      .��5      �  � RowDefinitions 5   
   �� 5      $
��401* �6   �� 5      $	��83* �6   .+�5   
   �� -   5         ��Name$ 	SearchBox��5      
�������$��Center=�69   $��16(�6�   $��44��6�   $	��Top=�6�   6�   �� 5   
   -   5        	 ��Name$	 
SearchList��5      $��16(�6&   
 �� IsTextSearchEnabled
 .  
���OO/�$��0,44,0,0q�6k   $��2��6�   �� 5   
   -   5         ��Name$ WarningBorder��5       �� CornerRadius$ 5,5,0,0��6(   
���  ��$��3��6P   �� 5   
   -   5         ��Name$ WarningLabel��5      $��Center=�6&   
�������$-��'Use Shift or Click to Execute a command��6V   $��3��6�   $��20(�6�   $��DemiBold"�6�   $��Center=�6�   5      5      