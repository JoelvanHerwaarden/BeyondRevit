   M S B A M L   `   `   `  ����F  BBeyondRevit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:BeyondRevit.UIBeyondRevit.UI  5      *    $BeyondRevit.UI.GenericDropdownWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      .localclr-namespace:BeyondRevit.UI     5      $	��500��5      $	��700��6.   $	��500��6;     � WindowStartupLocation$  CenterOwner=�6G    � Title$ Beyond Revit��6k      MainBackground#���   6�    � 	Resources 5       �� MergedDictionaries �� Source�� 5	   
    5
      �� 5      $H Bpack://application:,,,/BeyondRevit;component/Resources/Styles.xaml?�6%   %    5      .��5      �  � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6    � RowDefinitions 5   
   �� 5      $��50 �6   �� 5      $��5* �6   �� 5      $	��74* �6   �� 5      $��60 �6   .+�5   
   �� -   5         ��Name$ InstructionLabel��5      $��Instruction��6*   $��Center=�6@   $��10q�6[   
�������$��3��6z   �� 5   
   -   5         ��Name$ 	SearchBox��5      $��	Search...��6%   	 �� MaxLines$	 1��66   $��1��6C   $��	10,0,10,0q�6P   $��3��6c   6�   �� 5   
   -   5        
 ��Name$
 ItemNamesListBox��5   5   $��10,10,10,10q�6   $��14(�6'   $��2��6O   $��3��6\    �� SelectionMode$ Multiple=�6p   �� 5   
   -   5         ��Name$ SelectAllButton��5      $��3��6*     RoundButton#��[ 67   $��
Select All��6\   $��10q�6q   $��0��6�   �� 5   
   -   5        $ SelectNoneButton��5      $��3��6+   #��[ 68   $��Select None��6]   $��10q�6s   $��1��6�   �� 5    
   -   5        $ SubmitButton��5       $��3��6'   #��[ 64   $��Submit��6Y   $��10q�6j   $��2��6�   5!      5"      