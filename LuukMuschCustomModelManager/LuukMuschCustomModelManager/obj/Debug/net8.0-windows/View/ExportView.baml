   M S B A M L   `   `   `  ����V  RLuukMuschCustomModelManager, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullk:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views,LuukMuschCustomModelManager.ViewModels.Views  5      1    +LuukMuschCustomModelManager.View.ExportView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Q
viewmodels:clr-namespace:LuukMuschCustomModelManager.ViewModels.Views     5      _ [MaterialDesignThemes.Wpf, Version=5.1.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5        � DataContext  5
      B  <LuukMuschCustomModelManager.ViewModels.Views.ExportViewModel  5   
   .��5      � $��20q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��Export Settings��6   $��24(�6+   $��0,0,0,10q�69   �� 5   
    �� Orientation$ 
Horizontal=�6   $��	0,10,0,10q�6.   $��1��6A   .+�5      �� $��Export Path:��6   $��Center=�6,   $��0,0,10,0q�6G   �� 5      $	��500��5       !  MaterialDesignOutlinedTextBox#��[  5      $��Center=�5      ��5      �� *
ExportPath+ �� UpdateSourceTrigger$ PropertyChanged=�5      5      �� 5!   
   $ 
Horizontal=�6   $��2��6.   $	��Top=�6;   $��0,0,0,10q�6S   .+�5"      �� $��Export��6     MaterialDesignRaisedButton#��[ 5$      $��5q�5%      ��5#      �� *ExportCommand+5%      5&      5'      5(      