using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TextCaptureApp.Core.Models;

namespace TextCaptureApp.UI;

/// <summary>
/// Full-screen overlay window for region selection
/// </summary>
public partial class RegionSelectorWindow : Window
{
    private Point _startPoint;
    private bool _isSelecting;
    private RegionSelectionResult? _result;

    public RegionSelectorWindow()
    {
        InitializeComponent();
    }

    public RegionSelectionResult? Result => _result;

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _startPoint = e.GetPosition(this);
            _isSelecting = true;
            SelectionRectangle.Visibility = Visibility.Visible;
        }
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isSelecting) return;

        var currentPoint = e.GetPosition(this);

        var x = Math.Min(_startPoint.X, currentPoint.X);
        var y = Math.Min(_startPoint.Y, currentPoint.Y);
        var width = Math.Abs(currentPoint.X - _startPoint.X);
        var height = Math.Abs(currentPoint.Y - _startPoint.Y);

        Canvas.SetLeft(SelectionRectangle, x);
        Canvas.SetTop(SelectionRectangle, y);
        SelectionRectangle.Width = width;
        SelectionRectangle.Height = height;
    }

    private void Window_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting) return;

        _isSelecting = false;

        var endPoint = e.GetPosition(this);

        var x = (int)Math.Min(_startPoint.X, endPoint.X);
        var y = (int)Math.Min(_startPoint.Y, endPoint.Y);
        var width = (int)Math.Abs(endPoint.X - _startPoint.X);
        var height = (int)Math.Abs(endPoint.Y - _startPoint.Y);

        // Minimum size check (en az 10x10 pixel)
        if (width >= 10 && height >= 10)
        {
            _result = new RegionSelectionResult
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                IsCancelled = false
            };

            DialogResult = true;
            Close();
        }
        else
        {
            // Çok küçük seçim - tekrar dene
            SelectionRectangle.Visibility = Visibility.Collapsed;
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _result = new RegionSelectionResult { IsCancelled = true };
            DialogResult = false;
            Close();
        }
    }
}

