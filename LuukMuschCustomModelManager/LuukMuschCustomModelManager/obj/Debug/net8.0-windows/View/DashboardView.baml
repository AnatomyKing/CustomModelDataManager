   M S B A M L   `   `   `  ����V  RLuukMuschCustomModelManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullk:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views,LuukMuschCustomModelManager.ViewModels.Views  5      4    .LuukMuschCustomModelManager.View.DashboardView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      Q
viewmodels:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views     5      _ [MaterialDesignThemes.Wpf, Version=5.1.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5        � DataContext  5      E  ?LuukMuschCustomModelManager.ViewModels.Views.DashboardViewModel  5   
   .��5      � $��20q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   ]� $��0��6   $��10q�6!   .+�5      �� $
��Left=�6   )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ Search Custom Models��5       !  MaterialDesignOutlinedTextBox#��[  5      $	��300��5      $��0,0,10,0q�6   ��5   ,   �� *
SearchText+ �� UpdateSourceTrigger$ PropertyChanged=�5      �� 5      $
��Left=�6   $
��Auto��6/   �� 5      $��Right=�6    �� Orientation$ 
Horizontal=�60   $��Center=�6I   $��0,0,20,0q�6d   .+�5      �� $��Highest CMD:��6   $��Center=�5      $��0,0,5,0q�5      $��14(�5        ! PrimaryHueDarkForegroundBrush#���  5!      �� 5"        PrimaryHueLightBrush#���  6    �� CornerRadius$ 4��5#       �� Padding$	 5,2q�5$      $��Center=�5%      ._�5&      �� $
��Bold"�5'      #���  5(      $��16(�5)      ��5&       �� *HighestCustomModelNumber+5)      5*      5+      5,      �� 5.   
   $��1��6   $��10q�51      ��5/      �� *GroupedCustomModels+ �� SelectedItem 50      �� *SelectedFlattenedItem+ �� Mode$ TwoWay=�	 �� 
GroupStyle	 52      (  "System.Windows.Controls.GroupStyle  53      
   HeaderTemplate
 54      �� 55      .R�56      �� $
��Bold"�57      $��16(�58      #���  59      $	��0,5q�5:      ��56   (   �� *Name+5:      5;      5=      ��5?      �� 5@      .R�5A      #  MaterialDesignThemes.Wpf.Card  $��10q�6*   $��5q�67   #���  6B   .��5B      �� $ 
Horizontal=�6%   .+�5D      �� $
��Bold"�6D   #���  6V   ��6(   �� *
Value.Name+6V   �� 5E      $�� =��6(   $	��5,0q�62   #���  6?   �� 5F      #���  6Q   ��6(   �� *Value.CustomModelNumber+6Q   �� 5H      $��10,0,0,0q�6Z   ��6+   �� *Value.CustomVariations+��5I   "   �� 5J   &   .R�5K   *   �� $ 
Horizontal=�65   .+�5L   .   �� $��#��68   #���  6A   �� 5M   .   $	��5,0q�6X   #���  6e   ��68   �� *BlockType.Name+6e   �� 5N   .   $	��5,0q�6S   #���  6`   ��68   �� *	Variation+6`   5O   +   5P   '   5R      5S      5T      5U      5W      �� 5Y   
   $��2��6   $��+��6   $��Right=�5Z      $��Bottom=�6)   $��50��5[      $��50��6   $��10q�6$    & "MaterialDesignFloatingActionButton#��[ 5\      ��5Y   *   �� *
AddCommand+5\      )  #MaterialDesignThemes.Wpf.DialogHost  5^   
      
Identifier$ 
RootDialog��6$   $��1��6<   5_      5`      