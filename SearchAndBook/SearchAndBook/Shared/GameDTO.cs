using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media.Imaging;

namespace SearchAndBook.Shared
{
    /// IMPORTANT - IMAGE HANDLING:
    /// This class stores the image in TWO different formats for different purposes:
    ///
    /// 1. Image (byte[])
    ///    - Raw binary data (usually loaded from the database)
    ///    - Used for storage, transport, and persistence
    ///    - NOT directly usable in the UI
    ///
    /// 2. GameImage (BitmapImage)
    ///    - UI-friendly image format used by WinUI
    ///    - Can be directly bound to XAML controls (e.g. <Image Source="{Binding GameImage}" />)
    ///    - Must be created by converting the byte[] using ImageHelper
    ///
    /// WHY BOTH EXIST:
    /// - The database works with byte[]
    /// - The UI works with BitmapImage
    /// - Keeping both avoids repeated conversions and improves performance
    ///
    /// TYPICAL FLOW:
    /// - Load Image (byte[]) from database
    /// - Convert it using ImageHelper.ToBitmapImage(...)
    /// - Assign result to GameImage for UI display
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
