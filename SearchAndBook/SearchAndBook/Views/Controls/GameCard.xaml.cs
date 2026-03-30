using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace SearchAndBook.Views.Controls
{
    public sealed partial class GameCard : UserControl
    {
        public GameCard()
        {
            InitializeComponent();
        }

        // GAME ID (needed for navigation)
        public int GameId
        {
            get => (int)GetValue(GameIdProperty);
            set => SetValue(GameIdProperty, value);
        }

        public static readonly DependencyProperty GameIdProperty =
            DependencyProperty.Register(
                nameof(GameId),
                typeof(int),
                typeof(GameCard),
                new PropertyMetadata(0));

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

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                nameof(ImageSource),
                typeof(ImageSource),
                typeof(GameCard),
                new PropertyMetadata(null));

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

        private void OnCardClicked(object sender, RoutedEventArgs e)
        {
            if (this.Parent is FrameworkElement parent)
            {
                var frame = parent.XamlRoot.Content as Frame;
                frame?.Navigate(typeof(SearchAndBook.Views.GameDetailsView), GameId);
            }
        }
    }
}