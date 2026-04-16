using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using OurApp.Core.Models;
using OurApp.Core.Repositories;

namespace OurApp.WinUI.ViewModels
{
    public class OurJobsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility JobsVisibility => Visibility.Visible;
        public Visibility BackButtonVisibility => Visibility.Collapsed;

        public ObservableCollection<JobPosting> Jobs { get; } = new ObservableCollection<JobPosting>();

        public OurJobsViewModel()
        {
            ReloadJobs();
        }

        public void ReloadJobs()
        {
            Jobs.Clear();
            IJobsRepository jobsRepo = new JobsRepository();
            try
            {
                var jobsFromDb = jobsRepo.GetAllJobs();
                foreach (var job in jobsFromDb)
                {
                    Jobs.Add(job);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error loading jobs: {ex.Message}");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
