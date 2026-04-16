using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OurApp.WinUI.ViewModels;
using System;

namespace OurApp.WinUI;

public sealed partial class CreateJobPage : Page
{
    public CreateJobViewModel ViewModel { get; } = new CreateJobViewModel();

    public CreateJobPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    private static string? GetComboString(ComboBox combo)
    {
        if (combo?.SelectedItem is ComboBoxItem item && item.Content != null)
        {
            return item.Content.ToString();
        }

        return null;
    }

    private static DateTime? GetCalendarDate(CalendarDatePicker? picker)
    {
        return picker?.Date?.DateTime.Date;
    }

    private static int? GetOptionalInt(int box)
    {
        if (box == null || double.IsNaN(box))
        {
            return null;
        }

        return (int)box;
    }

    private async void SaveJob_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var mw = App.mainWindow;
        if (mw?.sessionService?.loggedInUser == null)
        {
            return;
        }

        var companyId = mw.sessionService.loggedInUser.CompanyId;
        var industry = GetComboString(IndustryCombo);
        var jobType = GetComboString(JobTypeCombo);
        var experience = GetComboString(ExperienceCombo);

        int? salary = null;
        if (!string.IsNullOrWhiteSpace(SalaryBox.Text)
            && int.TryParse(SalaryBox.Text.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture, out var sal))
        {
            salary = sal;
        }

        if (salary.HasValue && salary.Value < 0)
        {
            var bad = new ContentDialog
            {
                Title = "Validation",
                Content = "Salary cannot be negative.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await bad.ShowAsync();
            return;
        }

        var (ok, message) = ViewModel.TrySave(
            companyId,
            JobTitleBox.Text,
            industry ?? "",
            jobType ?? "",
            experience ?? "",
            GetCalendarDate(StartDatePicker),
            GetCalendarDate(EndDatePicker),
            DescriptionBox.Text,
            LocationBox.Text,
            (int)PositionsBox.Value,
            PhotoBox.Text,
            salary,
            GetOptionalInt(0),
            GetCalendarDate(DeadlinePicker));

        var dialog = new ContentDialog
        {
            Title = ok ? "Success" : "Could not create job",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();

        if (ok && Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void Cancel_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void NavigateBack_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
