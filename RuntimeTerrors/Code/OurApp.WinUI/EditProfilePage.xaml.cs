using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using OurApp.Core.Repositories;
using OurApp.Core.Services;
using OurApp.Core.ViewModels;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace OurApp.WinUI;

public sealed partial class EditProfilePage : Page
{
    public EditCompanyProfileViewModel ViewModel { get; }

    public EditProfilePage()
    {
        var mainWindow = App.mainWindow;
        ViewModel = new EditCompanyProfileViewModel(mainWindow.companyService, mainWindow.gameService);
        InitializeComponent();
        DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var id = e.Parameter is int companyId ? companyId : 1;
        ViewModel.Load(id);
    }

    private void NavigateBack_Click(object sender, RoutedEventArgs e)
    {
        var mainW = App.mainWindow;
        if (mainW.RootFrame.CanGoBack)
            mainW.RootFrame.GoBack();
        else
            mainW.RootFrame.Navigate(typeof(ViewProfilePage), ViewModel.CompanyId);
    }


    private async void AttachProfileImage_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };

        // Common image formats
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".gif");

        // Required for WinUI 3 pickers
        IntPtr hwnd = WindowNative.GetWindowHandle(App.mainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file == null)
            return;

        PhotoFileNameTextBlock.Text = file.Name;

        // Read file bytes and convert to base64 (stored in ViewModel.Photo)
        byte[] bytes;
        using (var input = await file.OpenReadAsync())
        using (var reader = new DataReader(input.GetInputStreamAt(0)))
        {
            await reader.LoadAsync((uint)input.Size);
            bytes = new byte[input.Size];
            reader.ReadBytes(bytes);
        }

        // Store as a data-URI so the backend validator can recognize the image type.
        var ext = Path.GetExtension(file.Name).TrimStart('.').ToLowerInvariant();
        var mimeSubtype = ext == "jpg" ? "jpeg" : ext;
        ViewModel.ProfilePicturePath = $"data:image/{mimeSubtype};base64,{Convert.ToBase64String(bytes)}";

        // Create preview image from the selected bytes
        var bitmapImage = new BitmapImage();
        using (var memStream = new InMemoryRandomAccessStream())
        {
            await memStream.WriteAsync(bytes.AsBuffer());
            memStream.Seek(0);
            bitmapImage.SetSource(memStream);
        }
        PhotoPreviewImage.Source = bitmapImage;
    }
    private async void AttachLogoImage_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };

        // Common image formats
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".gif");

        // Required for WinUI 3 pickers
        IntPtr hwnd = WindowNative.GetWindowHandle(App.mainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file == null)
            return;

        LogoFileNameTextBlock.Text = file.Name;

        // Read file bytes and convert to base64 (stored in ViewModel.Photo)
        byte[] bytes;
        using (var input = await file.OpenReadAsync())
        using (var reader = new DataReader(input.GetInputStreamAt(0)))
        {
            await reader.LoadAsync((uint)input.Size);
            bytes = new byte[input.Size];
            reader.ReadBytes(bytes);
        }

        // Store as a data-URI so the backend validator can recognize the image type.
        var ext = Path.GetExtension(file.Name).TrimStart('.').ToLowerInvariant();
        var mimeSubtype = ext == "jpg" ? "jpeg" : ext;
        ViewModel.CompanyLogoPath = $"data:image/{mimeSubtype};base64,{Convert.ToBase64String(bytes)}";

        // Create preview image from the selected bytes
        var bitmapImage = new BitmapImage();
        using (var memStream = new InMemoryRandomAccessStream())
        {
            await memStream.WriteAsync(bytes.AsBuffer());
            memStream.Seek(0);
            bitmapImage.SetSource(memStream);
        }
        LogoPreviewImage.Source = bitmapImage;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        var err = ViewModel.TrySave();
        if (err is null)
        {
            var ok = new ContentDialog
            {
                Title = "Profile saved",
                Content = "Your changes were saved.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await ok.ShowAsync();
            App.mainWindow.RootFrame.Navigate(typeof(ViewProfilePage), ViewModel.CompanyId);
            return;
        }

        var bad = new ContentDialog
        {
            Title = "Could not save",
            Content = err,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await bad.ShowAsync();
    }

    private async void Cancel_Click(object sender, RoutedEventArgs e)
    {
        var confirm = new ContentDialog
        {
            Title = "Discard changes?",
            Content = "Are you sure you want to cancel the modifications?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            XamlRoot = XamlRoot
        };
        var result = await confirm.ShowAsync();
        if (result != ContentDialogResult.Primary)
            return;

        NavigateBack_Click(sender, e);
    }
}
