using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;

namespace SearchAndBook.Shared
{
    public class GameDTO : INotifyPropertyChanged
    {
        public int GameId { get; set; }
        public string Name { get; set; }
        public byte[]? Image { get; set; }
        public decimal Price { get; set; }
        public string City { get; set; }
        public int MaximumPlayerNumber { get; set; }
        public int MinimumPlayerNumber { get; set; }

        private BitmapImage? _gameImage;
        public BitmapImage? GameImage
        {
            get => _gameImage;
            set
            {
                _gameImage = value;
                OnPropertyChanged(nameof(GameImage));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
