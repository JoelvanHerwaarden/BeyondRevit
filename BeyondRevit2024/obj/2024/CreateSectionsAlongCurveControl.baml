   M S B A M L   `   `   `  ����   BeyondRevit4    .BeyondRevit.UI.CreateSectionsAlongCurveControl   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     &localusing:BeyondRevit.UI     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       � 	Resources   �� MergedDictionaries �� Source��  �� $H Bpack://application:,,,/BeyondRevit;component/Resources/Styles.xaml?�%    .��� 
������� � RowDefinitions �� $��60 ��� $��40 ��� $��40 ��� $��40 ��� $��* ��� $��* ��� $��40 ��� $��40 ��� $��40 ��� $��60 � � ColumnDefinitions �� $��* ��� $��* �.+���    InstructionLabel#��[  $��Nothing Selected��$��1����   RoundButton#��[ $
��Left=�$��10q�$	��150��$��Select Section Path��$��0���� #��[  $��1��$��View name prefix���� #��[  $��2��$��Starting Number���� #��[  $��3��$��Section Frequency���� -    ��Name$ ViewNamePrefixTextbox��$��1��$��1�� �� MaxLines$ 1��$��	10,5,10,5q����� *ViewNamePrefix+�� -   $ StartingNumberTextbox��$��2��$��1��$ 1��$��	10,5,10,5q��� -   $ RequencyTextbox��$��3��$��1��$ 1��$��	10,5,10,5q�� $��	10,0,10,0q�$��4��$��2��$��2�� �� $��80 ��� $	��30* ��� $	��30* � �� $��35 ��� $��* ��� $��* �.+��� $��1�� �� Orientation$ 
Horizontal=�$��Center=�.+��� $��
Width Left��$��Center=�  	FontColor#��[ �� -   $ WidthLeftTextbox��$ 1��$	��100��$
��1000��$��20,5,0,5q�$��Center=��� $��2��$ 
Horizontal=�$��Center=�.+��� $��Width Right��$��Center=�#��[ �� -   $ WidthRightTextbox��$ 1��$	��100��$
��1000��$��20,5,0,5q�$��Center=��� $��1��$ 
Horizontal=�$��Center=�.+��� $��
Top Offset��$��Center=�#��[  � LayoutTransform �� 	 �� Angle$		 -90Q��� -   $ TopOffsetTextbox��$ 1��$	��100��$
��1000��$��0,0,0,0q�$��Center=�.�� �� $		 -90Q�.���� $��2��$ 
Horizontal=�$��Center=�.+��� $��Bottom Offset��$��Center=�#��[  �� $		 -90Q��� -   $ BottomOffsetTextbox��$ 1��$	��100��$
��1000��$��0,0,0,0q�$��Center=�.�� �� $		 -90Q�.���� $��1��$��1��
������$��3,3,1,1q��� $��1��$��2��
������$��1,3,3,1q��� $��2��$��1��
������$��3,1,1,3q��� $��2��$��2��
������$��1,1,3,3q��� $��1��$��2��$��1��$��2��
 K� ZIndex$
 5��
���  ��$��15��$��15�� �� CornerRadius$	 7.5��$��Center=�$��Center=��� $��Curve Center��$��1��$��1��$��Bottom=�$��Right=��� #��[  $��6��$��
View Depth���� -   $ ViewDepthTextbox��$��6��$��1��$ 1��$��	10,5,10,5q��� #��[  $��7��$��Flip Section Path���� -	    ��Name$ FlipSectionPath��$��7��$��1��$��Center=�$��10q��� #��[  $��8��$$��Flip Section viewing direction���� -
   $ FlipSectionDirection��$��8��$��1��$��Center=�$��10q��� -   #��[ $	��150��$��1��$��9��$��Right=�$��Create��$��10q�