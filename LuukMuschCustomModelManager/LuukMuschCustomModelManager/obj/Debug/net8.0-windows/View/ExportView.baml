   M S B A M L   `   `   `  ����V  RLuukMuschCustomModelManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullk:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views,LuukMuschCustomModelManager.ViewModels.Views  5      1    +LuukMuschCustomModelManager.View.ExportView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Q
viewmodels:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views     5      _ [MaterialDesignThemes.Wpf, Version=5.1.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5        � DataContext  5
      B  <LuukMuschCustomModelManager.ViewModels.Views.ExportViewModel  5   
   .��5      � $��20q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��Export Settings��6   $��24(�5      $��0,0,0,10q�5      �� 5   
   $��1��6    �� Orientation$ Vertical=�6"   $��	0,10,0,10q�69   .+�5      �� $ 
Horizontal=�6   $��0,0,0,8q�62   .+�5      �� $��YAML Export Path:��6   $��Center=�5      $��0,0,10,0q�5       �� 5!      $	��500��5"       !  MaterialDesignOutlinedTextBox#��[  5#      $��Center=�5$      ��5!      �� *
ExportPath+ �� UpdateSourceTrigger$ PropertyChanged=�5$      �� 5%      $��Export .yml��6     MaterialDesignRaisedButton#��[ 5'      $��10,0,0,0q�5(      $��Center=�5)      ��5&      �� *ExportCommand+5)      5*      �� 5-      $ 
Horizontal=�6   $��0,0,0,8q�62   .+�5.      �� $��JSON Export Folder:��6   $��Center=�5/      $��0,0,10,0q�50      �� 51      $	��500��52      #��[  53      $��Center=�54      ��51      �� *JsonExportFolder+$ PropertyChanged=�54      55      �� 58      $*��$Parent Whitelist (JSON Export Only):��6   $
��Bold"�59      $��0,8,0,4q�5:      �� 5;        PrimaryHueLightBrush#���  6   $��1q�6J    �� Padding$ 8q�6^   ._�5<      �� $
��Auto=�6   $	��120��6B   .��5=      �� ��6#   �� *ParentWhitelist+��5>      �� 5?      .R�5@   "   �� $ 
Horizontal=�6-   $��2q�6F   .+�5F   &   �� $��Center=�5G   /    d� 	IsChecked 5F   /   �� *
IsSelected+5G   /   �� 5H   &   $��5,0,0,0q�5I   0   $��Center=�5J   0   ��5H   0   �� *
ParentName+5J   0   5K   #   5L      5N      5O      5P      �� 5S      $��Export JSON��6   #��[ 5U      $��0,10,0,0q�5V      $
��Left=�5W      ��5T      �� *ExportToJsonCommand+5W      5X      5[      5\      