using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using OurApp.Core.Repositories;
using OurApp.Core.Services;
using OurApp.Core.ViewModels;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace OurApp.WinUI;

public sealed partial class ViewProfilePage : Page
{
    public CompanyProfileViewModel ViewModel { get; }

    public ViewProfilePage()
    {
        var mainWindow = App.mainWindow;
        ViewModel = new CompanyProfileViewModel(mainWindow.companyService, new ProfileCompletionCalculator(mainWindow.jobsRepository, mainWindow.applicantsRepository), mainWindow.gameService, mainWindow.eventsService, mainWindow.sessionService, mainWindow.collabsService, mainWindow.jobsRepository);
        InitializeComponent();
        DataContext = ViewModel;
        ViewModel.NavigateEditProfileRequested += (_, _) =>
        {
            App.mainWindow.RootFrame.Navigate(typeof(EditProfilePage), ViewModel.CompanyId);
        };
        ViewModel.NavigateAllEventsRequested += (_, _) =>
        {
            App.mainWindow.RootFrame.Navigate(typeof(OurEventsPage), ViewModel.CompanyId);
        };
        ViewModel.NavigateAllJobsRequested += (_, _) =>
        {
            App.mainWindow.RootFrame.Navigate(typeof(OurEventsPage), ViewModel.CompanyId);
        };
        ViewModel.NavigateAllCollaboratorRequested += (_, _) =>
        {
            App.mainWindow.RootFrame.Navigate(typeof(CollaboratorsPage), ViewModel.CompanyId);
        };
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var id = e.Parameter is int companyId ? companyId : 1;
        ViewModel.Load(id);

        TryRenderCompanyImage();
        TryRenderCompanyLogo();
    }

    private async void TryRenderCompanyLogo()
    {
        try
        {
            var raw = ViewModel.Company?.CompanyLogoPath ?? "";
            if (string.IsNullOrWhiteSpace(raw))
            {
                CompanyLogoHintText.Text = "(no logo)";
                CompanyLogoBrush.ImageSource = null;
                return;
            }

            // If the DB stored a data-URI, decode and show it.
            const string prefix = "data:image/";
            const string base64Marker = ";base64,";
            if (raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var base64Index = raw.IndexOf(base64Marker, StringComparison.OrdinalIgnoreCase);
                if (base64Index < 0)
                    throw new FormatException("Invalid image data URI.");

                var base64 = raw.Substring(base64Index + base64Marker.Length);
                var bytes = Convert.FromBase64String(base64);

                var bitmap = new BitmapImage();
                using (var mem = new InMemoryRandomAccessStream())
                {
                    await mem.WriteAsync(bytes.AsBuffer());
                    mem.Seek(0);
                    bitmap.SetSource(mem);
                }

                CompanyLogoHintText.Text = "";
                CompanyLogoBrush.ImageSource = bitmap;
                return;
            }

            // Otherwise, don't spam the UI with the raw string.
            CompanyLogoHintText.Text = "(logo set)";
            CompanyLogoBrush.ImageSource = null;
        }
        catch
        {
            CompanyLogoHintText.Text = "(logo could not be rendered)";
            CompanyLogoBrush.ImageSource = null;
        }
    }
    private async void TryRenderCompanyImage()
    {
        try
        {
            var raw = ViewModel.Company?.ProfilePicturePath ?? "";
            if (string.IsNullOrWhiteSpace(raw))
            {
                ProfilePictureHintText.Text = "(no image)";
                ProfilePictureBrush.ImageSource = null;
                return;
            }

            // If the DB stored a data-URI, decode and show it.
            const string prefix = "data:image/";
            const string base64Marker = ";base64,";
            if (raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var base64Index = raw.IndexOf(base64Marker, StringComparison.OrdinalIgnoreCase);
                if (base64Index < 0)
                    throw new FormatException("Invalid image data URI.");

                var base64 = raw.Substring(base64Index + base64Marker.Length);
                var bytes = Convert.FromBase64String(base64);

                var bitmap = new BitmapImage();
                using (var mem = new InMemoryRandomAccessStream())
                {
                    await mem.WriteAsync(bytes.AsBuffer());
                    mem.Seek(0);
                    bitmap.SetSource(mem);
                }

                ProfilePictureHintText.Text = "";
                ProfilePictureBrush.ImageSource = bitmap;
                return;
            }

            // Otherwise, don't spam the UI with the raw string.
            ProfilePictureHintText.Text = "(image set)";
            ProfilePictureBrush.ImageSource = null;
        }
        catch
        {
            ProfilePictureHintText.Text = "(image could not be rendered)";
            ProfilePictureBrush.ImageSource = null;
        }
    }


    private void SeeAllEventsButton_Click(object sender, RoutedEventArgs e)
    {
        this.Frame.Navigate(typeof(OurEventsPage), ViewModel.CompanyId);
    }
    private void SeeAllJobsButton_Click(object sender, RoutedEventArgs e)
    {
        this.Frame.Navigate(typeof(OurJobsPage), ViewModel.CompanyId);
    }
    private void SeeAllCollaboratorsButton_Click(object sender, RoutedEventArgs e)
    {
        this.Frame.Navigate(typeof(CollaboratorsPage), ViewModel.CompanyId);
    }
}
