using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OurApp.Core.Repositories;
using OurApp.Core.Services;
using OurApp.Core.Models;
using OurApp.Core.Validators;

namespace OurApp.WinUI
{
    public sealed partial class MainWindow : Window
    {
        private const int DefaultCompanyId = 1;

        public Frame RootFrame => rootFrame;
        public IEventsService eventsService { get; }
        public ICompanyService companyService { get; }
        public SessionService sessionService { get; }
        public ICollaboratorsService collabsService { get; }
        public IJobsRepository jobsRepository { get; }
        public IApplicantRepository applicantsRepository { get; }
        public IGameService gameService { get; }
        public IPaymentService paymentService { get; }
        public IGameValidator gameValidator {  get; }
        public IEventValidator eventValidator { get; }
        public ICompanyValidator companyValidator { get; }
        public IPaymentValidator paymentValidator { get; }
        /// <summary>
        /// MainWindow constructor that initialize the repositories and services
        /// </summary>
        public MainWindow()
        {
            ICompanyRepo companyRepository = new CompanyRepo();
            this.companyService = new CompanyService(companyRepository);

            // Interface fix: Mapping concrete GameService to IGameService
            this.gameService = new GameService(companyRepository);

            ICollaboratorsRepo collaboratorsRepository = new CollaboratorsRepo();
            this.collabsService = new CollaboratorsService(collaboratorsRepository);

            Company defaultCompany = new Company("ndj", "dnis", "dnjs", "hdjd", "sybau", "dj@");
            //companyService.addCompany("ndj", "dnis", "dnjs", "hdjd", "sybau", "dj@");
            //companyService.addCompany("ndj2", "dnis", "dnjs", "hdjd", "sybau", "dj@");
            //companyService.printAll();

            InitializeComponent();

            IEventsRepo eventsRepository = new EventsRepo();

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

            this.eventsService = new EventsService(eventsRepository);

            // Interface fix: Mapping concrete SessionService to ISessionService
            this.sessionService = new SessionService(defaultCompany); // hardcode user = defaultCompany

            this.jobsRepository = new JobsRepository();
            this.applicantsRepository = new ApplicantRepository();

            this.companyValidator = new CompanyValidator();
            this.eventValidator = new EventValidator();
            this.paymentValidator = new PaymentValidator();
            this.gameValidator = new GameValidator();

            IPaymentRepository paymentRepository = new PaymentRepository();
            this.paymentService = new PaymentService(paymentRepository, this.paymentValidator);
        }

        /// <summary>
        /// Function that navigates to a different page: "View Profile" page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigateToViewProfile_Click(object sender, RoutedEventArgs e)
        {
            RootFrame.Navigate(typeof(ViewProfilePage), DefaultCompanyId);
        }

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
