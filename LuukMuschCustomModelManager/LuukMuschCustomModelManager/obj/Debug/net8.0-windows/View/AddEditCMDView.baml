   M S B A M L   `   `   `  ����V  RLuukMuschCustomModelManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullY1clr-namespace:LuukMuschCustomModelManager.Helpers#LuukMuschCustomModelManager.Helpers  5      5    /LuukMuschCustomModelManager.View.AddEditCMDView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      _ [MaterialDesignThemes.Wpf, Version=5.1.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5      =local1clr-namespace:LuukMuschCustomModelManager.Helpers  5        � 	Resources  5	         GroupedParentItems �� Source �� GroupDescriptions2  ,System.Windows.Data.PropertyGroupDescription   PropertyName  GroupHeaderStyle �� 
TargetType �� Property �� Value �� 
TargetType �� Orientation	 ��Name
 d� IsThreeState��%  &	        &	 `     �� 5   
    6:   �� *ParentItems+ 5        5      $
 Type��6+   5      �� 5   
   " ��6)   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      �� .+�5      �� $ 
Horizontal=�6)   ���	#FFE0E0E0.+�5   "   �� $	 GroupHeaderCheckBox��6+   -   5        $
��Bold"�5   +   $��Center=�5   +   $

 True��5   +   ��5   +   �� *Name+5   +   5       �� 5"      $��10,0,0,0q�6-   5#      5$      5&      5'      5(      .��5*      � $��20q�6    � RowDefinitions 5+   
   �� 5,      $��* �6   �� 5-      $
��Auto �6   .+�5/   
   �� $
��Auto=�6   .��50      �� .+�52      �� )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ 
Model Name��53       ! MaterialDesignOutlinedTextBox#��[ 54      $
��0,10q�55      $	��300��6(   ��52      �� *Name+55   (   �� 56      $ 
Model Path��57      #��[ 58      $
��0,10q�59      $	��300��6(   ��56      �� *	ModelPath+59   (   �� 5:      $ Custom Model Number��5;      #��[ 5<      $
��0,10q�5=      $	��150��6(   ��5:      �� *CustomModelNumber+5=   (   �� 5@      $
��Used��5A      $
��0,10q�5B       d� 	IsChecked 5@      �� *Status+{�5C      �� *CanChangeStatus+�� 5F      $��Select Parent Items��6   $
��0,10q�68   .��5G      �� .+�5I      �� -   5         ��Name$ ParentSearchBox��5I   "   $��5q�5J   "   $	��250��5K   "   $ Search Parent Items��5L   "   5M   "   �� 5N       ! PrimaryHueDarkForegroundBrush#���  6!   $��1q�5O   !    �� Padding$ 5q�5P   !    �� CornerRadius$ 2��5Q   !   $��5q�62   ._�5R      �� $	��150��6+   $
��Auto=�68   .��5S   "   �� ��6/   ��  �� Source# [   �� 
GroupStyle 5T   &   (  "System.Windows.Controls.GroupStyle  5U   *      ContainerStyle# [ 65   ��5W   &   �� 5X   *   .R�5Z   .   �� $��5q�6P   ��67   �� *Name+ 5[   7   �� *
IsSelected+ �� Mode$ TwoWay=�5\   +   5^   #   5_      5`      5a      5b      �� 5e      $
��0,20q�6   ��5f      �� 5g      $ 
Horizontal=�6%   $��Center=�6>   .+�5h      �� $��
Block Info��6(   $��Center=�6:   $��0,0,5,0q�6U   
�������6f   �� 5i       � ToolTip$ Clear Block Info��5j   %     MaterialDesignToolButton#��[ 5k   %   ��5i   %   �� *ClearBlockInfoCommand+.��5l   "   '  !MaterialDesignThemes.Wpf.PackIcon     Kind$ Cleaning=�6:   
�������6J   5m      5n      .��5p      �� .+�5q      �� $ 
Horizontal=�6%   .+�5r      �� $ 
Block Data��5s   &   $	��200��5t   &   $
��0,10q�62   ��5r   &   �� *NewBlockData+5t   2   �� 5u      $ Variation Number��5v   &   $	��100��5w   &   $��
10,10,0,10q�62   ��5u   &   �� *NewVariationNumber+5w   2   �� 5x       �� DisplayMemberPath$
 Name��5z   '   $ 
Block Type��5{   '   $	��150��5|   '   $��
10,10,0,10q�63   ��5x   '   �� *
BlockTypes+ �� SelectedItem 5y   '   �� *SelectedBlockType+5|   3   5}      5~      5      �� 5�      $
��0,20q�6   ��5�      �� 5�      $ 
Horizontal=�6%   $��Center=�6>   .+�5�      �� $��Shader Armor Info��6(   $��Center=�6A   $��0,0,5,0q�6\   
�������6m   �� 5�      $ Clear Selection��5�   %   #��[ 5�   %   ��5�   %   �� *ClearArmorInfoCommand+.��5�   "     $ Cleaning=�6:   
�������6J   5�      5�      .��5�      �� .+�5�      �� $
 Name��5�   #   $ Select Shader Armor Info��5�   #   $
��0,10q�5�   #   $	��300��61   ��5�   #   �� *ShaderArmorColorInfos+ 5�   #   �� *SelectedShaderArmorColorInfo+5�   1   5�      5�      5�      5�      �� 5�   
   $��1��6   $ 
Horizontal=�6"   $��Right=�6;   $
��0,10q�6W   .+�5�      �� $��Cancel��6   $��75��6&   $��5q�61   ��6<   �� *CancelCommand+�� 5�      $
��Save��6   $��75��6$   $��5q�6/   ��6:   �� *SaveCommand+5�      5�      5�      