   M S B A M L   `   `   `  ���� V  RLuukMuschCustomModelManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullk:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views,LuukMuschCustomModelManager.ViewModels.Views  :    4LuukMuschCustomModelManager.View.MainViewContentView   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     Q
viewmodels:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views     _ [MaterialDesignThemes.Wpf, Version=5.1.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes    � DataContext  K  ELuukMuschCustomModelManager.ViewModels.Views.MainViewContentViewModel  .��� $��20q� � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+�]� $��0��$��10q�.+��� $
��Left=�)  #MaterialDesignThemes.Wpf.HintAssist   Hint$ Search Custom Models�� !  MaterialDesignOutlinedTextBox#��[  $	��300��$��0,0,10,0q����� *
SearchText+ �� UpdateSourceTrigger$ PropertyChanged=��� $
��Left=�$
��Auto���� $��Right=� �� Orientation$ 
Horizontal=�$��Center=�$��0,0,20,0q�.+��� $��Highest CMD:��$��Center=�$��0,0,5,0q�$��14(� ! PrimaryHueDarkForegroundBrush#���  ��   PrimaryHueLightBrush#���   �� CornerRadius$ 4�� �� Padding$	 5,2q�$��Center=�._��� $
��Bold"�#���  $��16(����� *HighestCustomModelNumber+�� $��1��$��10q����� *GroupedCustomModels+ �� SelectedItem �� *SelectedFlattenedItem+ �� Mode$ TwoWay=�	 �� 
GroupStyle	 (  "System.Windows.Controls.GroupStyle  
   HeaderTemplate
 �� .R��� $
��Bold"�$��16(�#���  $	��0,5q����� *Name+���� .R�#  MaterialDesignThemes.Wpf.Card  $��10q�$��5q�#���  .���� $ 
Horizontal=�.+��� $
��Bold"�#���  ���� *
Value.Name+�� $�� =��$	��5,0q�#���  �� #���  ���� *Value.CustomModelNumber+�� $��10,0,0,0q����� *Value.CustomVariations+���� .R��� $ 
Horizontal=�.+��� $��#��#���  �� $	��5,0q�#���  ���� *BlockType.Name+�� $	��5,0q�#���  ���� *	Variation+�� $��2��$��+��$��Right=�$��Bottom=�$��50��$��50��$��10q� & "MaterialDesignFloatingActionButton#��[ ���� *
AddCommand+)  #MaterialDesignThemes.Wpf.DialogHost     
Identifier$ 
RootDialog��$��1��