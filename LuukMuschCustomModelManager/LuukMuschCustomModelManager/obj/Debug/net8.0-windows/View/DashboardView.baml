   M S B A M L   `   `   `  ����V  RLuukMuschCustomModelManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullk:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views,LuukMuschCustomModelManager.ViewModels.Views  5      :    4LuukMuschCustomModelManager.View.MainViewContentView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      Q
viewmodels:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views     5      _ [MaterialDesignThemes.Wpf, Version=5.1.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5        � DataContext  5      K  ELuukMuschCustomModelManager.ViewModels.Views.MainViewContentViewModel  5   
   .��5      � $��20q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   ]� $��0��6   $��10q�6!   .+�5      �� $
��Left=�6   )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ Search Custom Models��5       !  MaterialDesignOutlinedTextBox#��[  5      $	��300��5      $��0,0,10,0q�6"   ��5   ,   �� *
SearchText+ �� UpdateSourceTrigger$ PropertyChanged=�5   "   �� 5      $
��Left=�6   $
��Auto��6/   �� 5"      $��Right=�6    �� Orientation$ 
Horizontal=�60   $��Center=�6I   $��0,0,20,0q�6d   .+�5#      �� $��Highest CMD:��6   $��Center=�5$      $��0,0,5,0q�5%      $��14(�5&       ! PrimaryHueDarkForegroundBrush#���  5'      �� 5(        PrimaryHueLightBrush#���  6    �� CornerRadius$ 4��5)       �� Padding$	 5,2q�5*      $��Center=�5+      ._�5,      �� $
��Bold"�5-       #���  5.       $��16(�5/       ��5,       �� *HighestCustomModelNumber+5/       50      51      52      �� 55   
   $��1��6   $��10q�58      ��56      �� *GroupedCustomModels+ �� SelectedItem 57      �� *SelectedCustomModelData+ �� Mode$ TwoWay=�	 �� 
GroupStyle	 5:      (  "System.Windows.Controls.GroupStyle  5;      
   HeaderTemplate
 5<      �� 5=      .R�5?      �� $
��Bold"�5@   (   $��16(�5A   (   #���  5B   (   $	��0,5q�5C   (   ��5?   (   �� *Name+5C   (   5D      5F      ��5J      �� 5K      .R�5L      #  MaterialDesignThemes.Wpf.Card  $��10q�6*   $��5q�67   #���  6B   .��5M      �� $ 
Horizontal=�6%   .+�5O      �� $
��Bold"�6>   #���  6P   ��6(   �� *Name+6P   �� 5P      $�� =��6(   $	��5,0q�62   #���  6?   �� 5R      #���  6K   ��6(   �� *CustomModelNumber+6K   �� 5U      $��10,0,0,0q�6T   ��6+   �� *CustomVariations+��5V   "   �� 5W   &   .R�5X   *   �� $ 
Horizontal=�65   .+�5Y   .   �� $��#��68   #���  6A   �� 5Z   .   $	��5,0q�6X   #���  6e   ��68   �� *BlockType.Name+6e   �� 5[   .   $	��5,0q�6S   #���  6`   ��68   �� *	Variation+6`   5\   +   5]   '   5_      5`      5a      5b      5d      �� 5g   
   $��2��6   $��+��6   $��Right=�5h      $��Bottom=�6-   $��50��5i      $��50��6   $��10q�6(    & "MaterialDesignFloatingActionButton#��[ 5j      ��5g   *   �� *
AddCommand+5j      )  #MaterialDesignThemes.Wpf.DialogHost  5m   
      
Identifier$ 
RootDialog��6$   $��1��6<      DialogContent 5n      �� 5o      ��6!   �� *AddEditDialogViewModel+5q      5r      5s      