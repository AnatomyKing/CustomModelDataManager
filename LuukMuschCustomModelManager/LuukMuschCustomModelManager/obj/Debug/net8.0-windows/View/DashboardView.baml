   M S B A M L   `   `   `  ����V  RLuukMuschCustomModelManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullk:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views,LuukMuschCustomModelManager.ViewModels.Views  5      4    .LuukMuschCustomModelManager.View.DashboardView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      Q
viewmodels:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views     5      _ [MaterialDesignThemes.Wpf, Version=5.1.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5        � DataContext  5      E  ?LuukMuschCustomModelManager.ViewModels.Views.DashboardViewModel  5   
   .��5      � $��20q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   ]� $��0��6   $��10q�6!   .+�5      �� $
��Left=�6   )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ Search Custom Models��5       !  MaterialDesignOutlinedTextBox#��[  5      $	��300��5      $��0,0,10,0q�5      ��5      �� *
SearchText+ �� UpdateSourceTrigger$ PropertyChanged=�5      �� 5      $
��Left=�6   $
��Auto��6/   �� 5!      $��Right=�6    �� Orientation$ 
Horizontal=�5"      $��Center=�5#      $��0,0,20,0q�5$      .+�5'      �� $��Highest CMD:��6   $��Center=�5(      $��0,0,5,0q�5)      $��14(�5*       ! PrimaryHueDarkForegroundBrush#���  5+      �� 5,        PrimaryHueLightBrush#���  6    �� CornerRadius$ 4��5-       �� Padding$	 5,2q�5.      $��Center=�5/      ._�50      �� $
��Bold"�51       #���  52       $��16(�53       ��50       �� *HighestCustomModelNumber+53       54      �� 57      $��Next Avail CMD:��6   $��Center=�58      $��20,0,5,0q�59      $��14(�5:      #���  5;      �� 5<      #���  6   $ 4��5=      $	 5,2q�5>      $��Center=�5?      ._�5@      �� $
��Bold"�5A       #���  5B       $��16(�5C       ��5@       �� * NextAvailableCustomModelNumber+5C       5D      5E      5F      �� 5I   
   $��1��6   $��10q�5L      ��5J      �� *GroupedCustomModels+ �� SelectedItem 5K      �� *SelectedFlattenedItem+ �� Mode$ TwoWay=�	 �� 
GroupStyle	 5M      (  "System.Windows.Controls.GroupStyle  5N      
   HeaderTemplate
 5O      �� 5P      .R�5Q      �� $
��Bold"�5R   (   $��16(�5S   (   #���  5T   (   $	��0,5q�5U   (   ��5Q   (   �� *Name+5U   (   5V      5X      ��5Z      �� 5[      .R�5\      #  MaterialDesignThemes.Wpf.Card  $��10q�6*   $��5q�5]   *   #���  5^   *   .��5_      �� $ 
Horizontal=�6%   .+�5a      �� $
��Bold"�5b   (   #���  5c   (   ��5a   (   �� *
Value.Name+5c   (   �� 5d      $�� =��6(   $	��5,0q�5e   (   #���  5f   (   �� 5g      #���  5h   (   ��5g   (   �� *Value.CustomModelNumber+5h   (   �� 5k      $��10,0,0,0q�6Z   ��6+   �� *Value.CustomVariations+��5l   "   �� 5m   &   .R�5n   *   �� $ 
Horizontal=�65   .+�5o   .   �� $��#��68   #���  6A   �� 5p   .   $	��5,0q�5q   8   #���  5r   8   ��5p   8   �� *BlockType.Name+5r   8   �� 5s   .   $	��5,0q�5t   8   #���  5u   8   ��5s   8   �� *	Variation+5u   8   5v   +   5w   '   5y      5z      5{      5|      5~      �� 5�   
   $��2��6   $��+��5�      $��Right=�5�      $��Bottom=�5�      $��50��5�      $��50��5�      $��10q�5�       & "MaterialDesignFloatingActionButton#��[ 5�      ��5�      �� *
AddCommand+5�      )  #MaterialDesignThemes.Wpf.DialogHost  5�   
      
Identifier$ 
RootDialog��6$   $��1��5�   #   5�      5�      