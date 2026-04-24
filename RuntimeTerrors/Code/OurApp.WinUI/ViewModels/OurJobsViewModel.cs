using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using OurApp.Core.Models;
using OurApp.Core.Repositories;

namespace OurApp.WinUI.ViewModels
{
    public partial class OurJobsViewModel : ObservableObject
    {
        private const string ErrorMessagePrefix = "Database error loading jobs: ";

        private readonly IJobsRepository jobsRepository;

        public Visibility JobsVisibility => Visibility.Visible;
        public Visibility BackButtonVisibility => Visibility.Collapsed;

        public ObservableCollection<JobPosting> Jobs { get; } = new ObservableCollection<JobPosting>();

        public OurJobsViewModel(IJobsRepository jobsRepository)
        {
            this.jobsRepository = jobsRepository;
            ReloadJobs();
        }

        public void ReloadJobs()
        {
            Jobs.Clear();
            try
            {
                var jobsFromDatabase = jobsRepository.GetAllJobs();
                foreach (var job in jobsFromDatabase)
                {
                    Jobs.Add(job);
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"{ErrorMessagePrefix}{exception.Message}");
            }
        }
    }
}
