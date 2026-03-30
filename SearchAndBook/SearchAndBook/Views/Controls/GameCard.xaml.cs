using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SearchAndBook.Views.Controls
{
    public sealed partial class GameCard : UserControl
    {
        public GameCard()
        {
            InitializeComponent();
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(GameCard),
                new PropertyMetadata(string.Empty));

        public string ImageUrl
        {
            get => (string)GetValue(ImageUrlProperty);
            set => SetValue(ImageUrlProperty, value);
        }

        public static readonly DependencyProperty ImageUrlProperty =
            DependencyProperty.Register(
                nameof(ImageUrl),
                typeof(string),
                typeof(GameCard),
                new PropertyMetadata(string.Empty));

        public string Location
        {
            get => (string)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register(
                nameof(Location),
                typeof(string),
                typeof(GameCard),
                new PropertyMetadata(string.Empty));

        public string PlayersText
        {
            get => (string)GetValue(PlayersTextProperty);
            set => SetValue(PlayersTextProperty, value);
        }

        public static readonly DependencyProperty PlayersTextProperty =
            DependencyProperty.Register(
                nameof(PlayersText),
                typeof(string),
                typeof(GameCard),
                new PropertyMetadata(string.Empty));

        public string PriceText
        {
            get => (string)GetValue(PriceTextProperty);
            set => SetValue(PriceTextProperty, value);
        }

        public static readonly DependencyProperty PriceTextProperty =
            DependencyProperty.Register(
                nameof(PriceText),
                typeof(string),
                typeof(GameCard),
                new PropertyMetadata(string.Empty));
    }
}

