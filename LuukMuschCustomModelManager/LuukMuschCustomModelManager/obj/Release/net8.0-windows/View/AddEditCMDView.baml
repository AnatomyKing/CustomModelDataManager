   M S B A M L   `   `   `  ���� V  RLuukMuschCustomModelManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullY1clr-namespace:LuukMuschCustomModelManager.Helpers#LuukMuschCustomModelManager.Helpers  5    /LuukMuschCustomModelManager.View.AddEditCMDView   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     _ [MaterialDesignThemes.Wpf, Version=5.1.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  =local1clr-namespace:LuukMuschCustomModelManager.Helpers    � 	Resources     GroupedParentItems �� Source �� GroupDescriptions2  ,System.Windows.Data.PropertyGroupDescription   PropertyName  GroupHeaderStyle �� 
TargetType �� Property �� Value �� 
TargetType �� Orientation	 ��Name
 d� IsThreeState��%  &	        &	 2     ��  �� *ParentItems+   $
 Type���� " ��.���  � �� �� " ��.R��� .+��� $ 
Horizontal=����	#FFE0E0E0.+��� $	 GroupHeaderCheckBox��-   $
��Bold"�$��Center=�$

 True������ *Name+�� $��10,0,0,0q�.��� $��20q� � RowDefinitions �� $��* ��� $
��Auto �.+��� $
��Auto=�.���� .+��� )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ 
Model Name�� ! MaterialDesignOutlinedTextBox#��[ $
��0,10q�$	��300������ *Name+�� $ 
Model Path��#��[ $
��0,10q�$	��300������ *	ModelPath+�� $ Custom Model Number��#��[ $
��0,10q�$	��150������ *CustomModelNumber+�� $
��Used��$
��0,10q� d� 	IsChecked �� *Status+{��� *CanChangeStatus+�� $��Select Parent Items��$
��0,10q�.���� .+��� -    ��Name$ ParentSearchBox��$��5q�$	��250��$ Search Parent Items����  ! PrimaryHueDarkForegroundBrush#���  $��1q� �� Padding$ 5q� �� CornerRadius$ 2��$��5q�._��� $	��150��$
��Auto=�.���� ����  �� Source# [   �� 
GroupStyle (  "System.Windows.Controls.GroupStyle     ContainerStyle# [ ���� .R��� $��5q����� *Name+ �� *
IsSelected+ �� Mode$ TwoWay=��� $
��0,20q����� $ 
Horizontal=�$��Center=�.+��� $��
Block Info��$��Center=�$��0,0,5,0q�
���������  � ToolTip$ Clear Block Info��  MaterialDesignToolButton#��[ ���� *ClearBlockInfoCommand+.��'  !MaterialDesignThemes.Wpf.PackIcon     Kind$ Cleaning=�
�������.���� .+��� $ 
Horizontal=�.+��� $ 
Block Data��$	��200��$
��0,10q����� *NewBlockData+�� $ Variation Number��$	��100��$��
10,10,0,10q����� *NewVariationNumber+��  �� DisplayMemberPath$
 Name��$ 
Block Type��$	��150��$��
10,10,0,10q����� *
BlockTypes+ �� SelectedItem �� *SelectedBlockType+�� $
��0,20q����� $ 
Horizontal=�$��Center=�.+��� $��Shader Armor Info��$��Center=�$��0,0,5,0q�
��������� $ Clear Selection��#��[ ���� *ClearArmorInfoCommand+.��  $ Cleaning=�
�������.���� .+��� $
 Name��$ Select Shader Armor Info��$
��0,10q�$	��300������ *ShaderArmorColorInfos+ �� *SelectedShaderArmorColorInfo+�� $��1��$ 
Horizontal=�$��Right=�$
��0,10q�.+��� $��Cancel��$��75��$��5q����� *CancelCommand+�� $
��Save��$��75��$��5q����� *SaveCommand+