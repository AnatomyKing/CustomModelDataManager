   M S B A M L   `   `   `  ����V  RLuukMuschCustomModelManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullY1clr-namespace:LuukMuschCustomModelManager.Helpers#LuukMuschCustomModelManager.Helpers  5      5    /LuukMuschCustomModelManager.View.AddEditCMDView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      _ [MaterialDesignThemes.Wpf, Version=5.1.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5      =local1clr-namespace:LuukMuschCustomModelManager.Helpers  5        � 	Resources  5	         BoolToVisConverter  GroupedParentItems �� Source �� GroupDescriptions2  ,System.Windows.Data.PropertyGroupDescription   PropertyName  GroupHeaderStyle �� 
TargetType �� Property �� Value �� 
TargetType �� Orientation	 ��Name
 d� IsThreeState��%  &	        &	      &	 s     �� 5
   
   6'   �� 5   
    6:   �� *ParentItems+ 5        5      $
 Type��6+   5      �� 5   
   " ��6)   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      �� .+�5      �� $ 
Horizontal=�6)   ���	#FFE0E0E0.+�5   "   �� $	 GroupHeaderCheckBox��6+   -   5        $
��Bold"�5   +   $��Center=�5   +   $

 True��5   +   ��5   +   �� *Name+5   +   5      �� 5      $��10,0,0,0q�6-   5      5       5"      5#      5$      .��5%      � $��20q�6    � RowDefinitions 5&   
   �� 5'      $��* �6   �� 5(      $
��Auto �6   .+�5*   
   �� $
��Auto=�6   .��5+      �� .+�5,      �� )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ 
Model Name��5-       ! MaterialDesignOutlinedTextBox#��[ 5.      $
��0,10q�5/      $	��300��6(   ��5,      �� *Name+5/   (   �� 50      $ 
Model Path��51      #��[ 52      $
��0,10q�53      $	��300��6(   ��50      �� *	ModelPath+53   (   �� 54      $ Custom Model Number��55      #��[ 56      $
��0,10q�57      $	��150��6(   ��54      �� *CustomModelNumber+{�58      �� *IsCustomModelNumberEditable+�� 5:      $ 
Horizontal=�6   $
��0,10q�66   .+�5;      �� $��Center=�5<       $��0,0,5,0q�6;   ��5;       �� *ToggleLabelText+5<   ;   d� 5=       $  MaterialDesignSwitchToggleButton#��[ 5>   #    d� 	IsChecked 5=   #   �� *	UseUnused+ �� Mode$ TwoWay=�{�5?   #   �� *IsToggleEnabled+5@      �� 5B      $
��Used��5C      $
��0,10q�5D       5B      �� *Status+{�5E      �� *IsStatusEditable+�� 5F      $��Select Parent Items��6   $
��0,10q�68   .��5G      �� .+�5H      �� -   5         ��Name$ ParentSearchBox��5H   "   $��5q�5I   "   $	��250��5J   "   $ Search Parent Items��5K   "   5L   "   �� 5M       ! PrimaryHueDarkForegroundBrush#���  6!   $��1q�5N   !    �� Padding$ 5q�5O   !    �� CornerRadius$ 2��5P   !   $��5q�62   ._�5Q      �� $	��150��6+   $
��Auto=�68   .��5R   "   �� ��6/   ��  �� Source# [  �� 
GroupStyle 5S   &   (  "System.Windows.Controls.GroupStyle  5T   *      ContainerStyle# [ 65   ��5V   &   �� 5W   *   .R�5X   .   �� $��5q�6P   ��67   �� *Name+ 5Y   7   �� *
IsSelected+$ TwoWay=�5Z   +   5\   #   5]      5^      5_      5`      �� 5a      $
��0,20q�6   ��5b      �� 5c      $ 
Horizontal=�6%   $��Center=�6>   .+�5d      �� $��
Block Info��6(   $��Center=�6:   $��0,0,5,0q�6U   
�������6f   �� 5e       � ToolTip$ Clear Block Info��5f   %     MaterialDesignToolButton#��[ 5g   %   ��5e   %   �� *ClearBlockInfoCommand+.��5h   "   '  !MaterialDesignThemes.Wpf.PackIcon     Kind$ Cleaning=�6:   
�������6J   5i      5j      .��5l      �� .+�5m      �� $ 
Horizontal=�6%   .+�5n      �� $ 
Block Data��5o   &   $	��200��5p   &   $
��0,10q�62   ��5n   &   �� *NewBlockData+5p   2   �� 5q      $ Variation Number��5r   &   $	��100��5s   &   $��
10,10,0,10q�62   ��5q   &   �� *NewVariationNumber+5s   2   �� 5t       �� DisplayMemberPath$
 Name��5v   '   $ 
Block Type��5w   '   $	��150��5x   '   $��
10,10,0,10q�63   ��5t   '   �� *
BlockTypes+ �� SelectedItem 5u   '   �� *SelectedBlockType+5x   3   5y      �� 5z      $ Block Model Path��5{   '   #��[ 5|   '   $	��300��5}   '   $��	10,0,0,10q�63   ��5z   &   �� *NewBlockModelPath+5}   3   5~      5      �� 5�      $
��0,20q�6   ��5�      �� 5�      $ 
Horizontal=�6%   $��Center=�6>   .+�5�      �� $��Shader Armor Info��6(   $��Center=�6A   $��0,0,5,0q�6\   
�������6m   �� 5�      $ Clear Selection��5�   %   #��[ 5�   %   ��5�   %   �� *ClearArmorInfoCommand+.��5�   "     $ Cleaning=�6:   
�������6J   5�      5�      .��5�      �� .+�5�      �� $
 Name��5�   #   $ Select Shader Armor Info��5�   #   $
��0,10q�5�   #   $	��300��61   ��5�   #   �� *ShaderArmorColorInfos+ 5�   #   �� *SelectedShaderArmorColorInfo+5�   1   5�      5�      5�      5�      �� 5�   
   $��1��6   $ 
Horizontal=�6"   $��Right=�6;   $
��0,10q�6W   .+�5�      �� $��Cancel��6   $��75��6&   $��5q�61   ��6<   �� *CancelCommand+�� 5�      $
��Save��6   $��75��6$   $��5q�6/   ��6:   �� *SaveCommand+5�      5�      5�      