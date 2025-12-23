using Avalonia.Controls;
using AvApp1.Model;

namespace AvApp1.Views;

public partial class ConfirmCloseDialog : Window
{
    public ConfirmCloseDialog()
    {
        InitializeComponent();
    }

    private void Save_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(ConfirmCloseResult.Save);
    }

    private void DontSave_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(ConfirmCloseResult.DontSave);
    }

    private void Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(ConfirmCloseResult.Cancel);
    }
}
