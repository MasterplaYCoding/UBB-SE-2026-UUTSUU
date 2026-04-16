using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OurApp.Core.Repositories;
using OurApp.Core.Services;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using OurApp.Core.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OurApp.WinUI
{
    public sealed partial class MainWindow : Window
    {
        public Frame RootFrame => rootFrame;
        public IEventsService eventsService { get; }
        public ICompanyService companyService { get; }
        public SessionService sessionService { get; }
        public ICollaboratorsService collabsService { get; }

        public IJobsRepository jobsRepository { get; }

        public IApplicantRepository applicantsRepository { get; }

        /// <summary>
        /// MainWindow constructor that initialize the repositories and services
        /// </summary>


        public GameService gameService;


        public MainWindow()
        {
            
            ICompanyRepo repo = new CompanyRepo();
            this.companyService = new CompanyService(repo);


            this.gameService = new GameService(repo);

            ICollaboratorsRepo collabRepo = new CollaboratorsRepo();
            this.collabsService = new CollaboratorsService(collabRepo);

            Company c1 = new Company("ndj", "dnis", "dnjs", "hdjd", "sybau", "dj@");
            //companyService.addCompany("ndj", "dnis", "dnjs", "hdjd", "sybau", "dj@");
            //companyService.addCompany("ndj2", "dnis", "dnjs", "hdjd", "sybau", "dj@");
            //companyService.printAll();
            InitializeComponent();

            IEventsRepo eventsRepo = new EventsRepo();

            // hardcode events
            //Event ev1 = new Event("", "Event1", "This is such a cool event. You should attend.", new DateTime(2026, 1, 21, 14, 0, 0), new DateTime(2026, 1, 24, 18, 0, 0), "Cluj-Napoca, Cluj", 1, new List<Company>());
            //Event ev2 = new Event("", "Event2", "This is another event. You should attend.", new DateTime(2026, 3, 21, 14, 0, 0), new DateTime(2026, 3, 24, 18, 0, 0), "Cluj-Napoca, Cluj", 1, new List<Company>());
            //Event ev3 = new Event("", "Event3", "Join us.", new DateTime(2026, 5, 21, 14, 0, 0), new DateTime(2026, 5, 21, 18, 0, 0), "Sibiu, Sibiu", 1, new List<Company>());
            //Event ev4 = new Event("", "Event4", "", new DateTime(2026, 3, 18, 14, 0, 0), new DateTime(2026, 3, 19, 18, 0, 0), "Bucuresti", 1, new List<Company>());

            //eventsRepo.AddEventToRepo(ev1);
            //eventsRepo.AddEventToRepo(ev2);
            //eventsRepo.AddEventToRepo(ev3);
            //eventsRepo.AddEventToRepo(ev4);

            //eventsRepo.printAll();
            eventsService = new EventsService(eventsRepo);
            sessionService = new SessionService(c1); // hardcode user = c1

            IJobsRepository jobsRepo = new JobsRepository();
            this.jobsRepository = jobsRepo;

            IApplicantRepository applicantRepo = new ApplicantRepository();
            this.applicantsRepository = applicantRepo;
        }

        /// <summary>
        /// Function that navigates to a different page: "Our Events" page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void NavigateToOurEvents_Click(object sender, RoutedEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Clicked to nav");
        //    RootFrame.Navigate(typeof(OurEventsPage));
        //}
        //private void NavigateToEditGame_Click(object sender, RoutedEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Clicked to editgame");
        //    RootFrame.Navigate(typeof(EditGame), gameService);
        //}

        /// <summary>
        /// Function that navigates to a different page: "Past Events" page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void NavigateToPastEvents_Click(object sender, RoutedEventArgs e)
        //{
        //    RootFrame.Navigate(typeof(PastEventsPage));
        //}

        //private void NavigateToGamePage_Click(object sender, RoutedEventArgs e)
        //{
        //    RootFrame.Navigate(typeof(GamePage));
        //}

        /// <summary>
        /// Function that navigates to a different page: "View Profile" page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigateToViewProfile_Click(object sender, RoutedEventArgs e)
        {
            RootFrame.Navigate(typeof(ViewProfilePage), 1);
        }

        /// <summary>
        /// Function that navigates to a different page: "Edit Profile" page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void NavigateToEditProfile_Click(object sender, RoutedEventArgs e)
        //{
        //    RootFrame.Navigate(typeof(EditProfilePage), 1);
        //    System.Diagnostics.Debug.WriteLine("Clicked to nav");

        //}

        // OLD CONECTIONS
        //<AppBarButton Label = "Our jobs" />
        //    < AppBarButton Label="Past jobs"/>
        //    <AppBarButton Label = "Our events" Click="NavigateToOurEvents_Click"/>
        //    <AppBarButton Label = "Past events" Click="NavigateToPastEvents_Click"/>
        //    <AppBarButton Label = "Game" Click="NavigateToGamePage_Click"/>
        //    <AppBarButton Label = "Edit Game" Click="NavigateToEditGame_Click"/>
        //<MenuFlyoutItem Text="Edit Profile" Click="NavigateToEditProfile_Click"/>
    }
}