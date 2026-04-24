using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Core.Services;

namespace OurApp.Core.ViewModels;

public sealed class CompanyCollabListRow
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>Rows for the "Posted jobs" / "Events" preview lists on the company view profile page.</summary>
public sealed class CompanyProfileListRow
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
}

/// <summary>One trending skill row for the statistics sidebar (frontend display; fill from analytics later).</summary>
public sealed class CompanyTrendingSkillRow
{
    public string Rank { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}

public partial class CompanyProfileViewModel : ObservableObject
{
    private const int MaximumTopJobsCount = 3;
    private const int MaximumTopEventsCount = 3;
    private const int MaximumTopCollaboratorsCount = 7;
    private const int MaximumTrendingSkillsCount = 3;
    private const int MaximumScenarioCount = 2;
    private const int InitialScenarioIndex = 0;
    private const int TotalProfileTasksCount = 5;
    private const int EmptyTaskCount = 0;
    private const int EmptyCompletionPercentage = 0;
    private const int BaseDisplayRankOffset = 1;
    private const int InvalidIndexFallback = 0;

    private const string ProfileLoadErrorMessage = "We could not load this company profile.";
    private const string EmptySkillNameFallback = "—";
    private const string EmptySkillPercentageFallback = "0%";
    private const string FormattedSkillPercentageSuffix = "%";

    private const string DataUriPrefix = "data:image/";
    private const string Base64Marker = ";base64,";
    private const string HintNoLogo = "(no logo)";
    private const string HintLogoSet = "(logo set)";
    private const string HintLogoRenderError = "(logo could not be rendered)";
    private const string HintNoImage = "(no image)";
    private const string HintImageSet = "(image set)";
    private const string HintImageRenderError = "(image could not be rendered)";

    private readonly ICompanyService companyService;
    private readonly IGameService gameService;
    private readonly IEventsService eventsService;
    private readonly IJobsRepository jobsRepository;
    private readonly SessionService sessionService;
    private readonly ICollaboratorsService collaboratorsService;
    private readonly IProfileCompletionCalculator calculator;

    private int currentScenarioIndex;

    public Action<byte[]>? OnProfileImageDecoded { get; set; }
    public Action? OnProfileImageCleared { get; set; }
    public Action<byte[]>? OnLogoDecoded { get; set; }
    public Action? OnLogoCleared { get; set; }

    [ObservableProperty]
    private string profilePictureHintText = string.Empty;

    [ObservableProperty]
    private string companyLogoHintText = string.Empty;

    [ObservableProperty]
    private string currentQuestion = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> currentChoices = new ();

    [ObservableProperty]
    private string feedback = string.Empty;

    public string BuddyImagePath => BuddyImageProvider.GetImagePathById(gameService.GetBuddyId());

    [ObservableProperty]
    private string welcomeMessage = string.Empty;

    [ObservableProperty]
    private GameState currentState = GameState.NotCompleted;

    [ObservableProperty]
    private Company? company;

    [ObservableProperty]
    private string loadMessage = string.Empty;

    [ObservableProperty]
    private int completionPercentage;

    [ObservableProperty]
    private int completedTasksCount;

    [ObservableProperty]
    private ObservableCollection<string> remainingTasks = new ();

    [ObservableProperty]
    private string applicantSummary = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyTrendingSkillRow> trendingSkills = new ();

    public IEnumerable<CompanyProfileListRow> Top3JobPreviews =>
        jobsRepository
            .GetAllJobs()
            .Take(MaximumTopJobsCount)
            .Select(job => new CompanyProfileListRow
            {
                Title = job.JobTitle,
                Subtitle = job.JobDescription
            });

    public IEnumerable<CompanyProfileListRow> Top3EventPreviews =>
        eventsService
            .GetCurrentEvents(sessionService.LoggedInUser.CompanyId)
            .Take(MaximumTopEventsCount)
            .Select(eventItem => new CompanyProfileListRow
            {
                Title = eventItem.Title,
                Subtitle = eventItem.Description
            });

    public IEnumerable<CompanyCollabListRow> Top3CollabsPreviews =>
        collaboratorsService
            .GetAllCollaborators(sessionService.LoggedInUser.CompanyId)
            .Take(MaximumTopCollaboratorsCount)
            .Select(collaborator => new CompanyCollabListRow
            {
                Name = collaborator.Name
            });

    public event EventHandler? NavigateAllCollaboratorRequested;
    public event EventHandler? NavigateEditProfileRequested;
    public event EventHandler? NavigateAllEventsRequested;
    public event EventHandler? NavigateAllJobsRequested;

    public int CompanyId { get; private set; }

    public CompanyProfileViewModel(
        ICompanyService companyService,
        IProfileCompletionCalculator calculator,
        IGameService gameService,
        IEventsService eventService,
        SessionService sessionService,
        ICollaboratorsService collaboratorsService,
        IJobsRepository jobsRepository)
    {
        this.gameService = gameService;
        this.companyService = companyService;
        this.calculator = calculator;
        eventsService = eventService;
        this.sessionService = sessionService;
        this.collaboratorsService = collaboratorsService;
        this.jobsRepository = jobsRepository;
    }

    public void Load(int companyId)
    {
        CompanyId = companyId;
        Company = companyService.GetCompanyById(companyId);
        if (Company is null)
        {
            LoadMessage = ProfileLoadErrorMessage;
            CompletionPercentage = EmptyCompletionPercentage;
            CompletedTasksCount = EmptyTaskCount;
            RemainingTasks.Clear();
            return;
        }

        ApplicantSummary = calculator.ApplicantsMessage(companyId);

        LoadMessage = string.Empty;
        RefreshProfileStatistics();
        FillPreviewSections();
        ProcessImages();
        GamePreview();
    }

    private void ProcessImages()
    {
        var rawLogo = Company?.CompanyLogoPath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(rawLogo))
        {
            CompanyLogoHintText = HintNoLogo;
            OnLogoCleared?.Invoke();
        }
        else if (rawLogo.StartsWith(DataUriPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var base64Index = rawLogo.IndexOf(Base64Marker, StringComparison.OrdinalIgnoreCase);
            if (base64Index >= InvalidIndexFallback)
            {
                var base64 = rawLogo.Substring(base64Index + Base64Marker.Length);
                try
                {
                    var bytes = Convert.FromBase64String(base64);
                    CompanyLogoHintText = string.Empty;
                    OnLogoDecoded?.Invoke(bytes);
                }
                catch
                {
                    CompanyLogoHintText = HintLogoRenderError;
                    OnLogoCleared?.Invoke();
                }
            }
            else
            {
                CompanyLogoHintText = HintLogoRenderError;
                OnLogoCleared?.Invoke();
            }
        }
        else
        {
            CompanyLogoHintText = HintLogoSet;
            OnLogoCleared?.Invoke();
        }

        var rawPic = Company?.ProfilePicturePath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(rawPic))
        {
            ProfilePictureHintText = HintNoImage;
            OnProfileImageCleared?.Invoke();
        }
        else if (rawPic.StartsWith(DataUriPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var base64Index = rawPic.IndexOf(Base64Marker, StringComparison.OrdinalIgnoreCase);
            if (base64Index >= InvalidIndexFallback)
            {
                var base64 = rawPic.Substring(base64Index + Base64Marker.Length);
                try
                {
                    var bytes = Convert.FromBase64String(base64);
                    ProfilePictureHintText = string.Empty;
                    OnProfileImageDecoded?.Invoke(bytes);
                }
                catch
                {
                    ProfilePictureHintText = HintImageRenderError;
                    OnProfileImageCleared?.Invoke();
                }
            }
            else
            {
                ProfilePictureHintText = HintImageRenderError;
                OnProfileImageCleared?.Invoke();
            }
        }
        else
        {
            ProfilePictureHintText = HintImageSet;
            OnProfileImageCleared?.Invoke();
        }
    }

    public void RefreshProfileStatistics()
    {
        if (Company is null)
        {
            return;
        }

        var (percentage, tasks) = calculator.Calculate(Company);
        CompletionPercentage = percentage;
        CompletedTasksCount = TotalProfileTasksCount - tasks.Count;
        if (CompletedTasksCount < EmptyTaskCount)
        {
            CompletedTasksCount = EmptyTaskCount;
        }

        RemainingTasks.Clear();
        foreach (var task in tasks)
        {
            RemainingTasks.Add(task);
        }
    }

    private void FillPreviewSections()
    {
        if (Company is null)
        {
            return;
        }

        TrendingSkills.Clear();
        var (skillNames, percents) = calculator.GetSkillsTop3(Company.CompanyId);

        for (int index = 0; index < MaximumTrendingSkillsCount; index++)
        {
            string skillName = index < skillNames.Count ? skillNames[index] : EmptySkillNameFallback;
            string percent = index < percents.Count ? $"{percents[index]}{FormattedSkillPercentageSuffix}" : EmptySkillPercentageFallback;

            TrendingSkills.Add(new CompanyTrendingSkillRow
            {
                Rank = (index + BaseDisplayRankOffset).ToString(),
                SkillName = skillName,
                Detail = percent
            });
        }

        OnPropertyChanged(nameof(Top3JobPreviews));
        OnPropertyChanged(nameof(Top3EventPreviews));
        OnPropertyChanged(nameof(Top3CollabsPreviews));
    }

    [RelayCommand]
    private void SeeAllCollaborators()
    {
        NavigateAllCollaboratorRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void EditProfile()
    {
        NavigateEditProfileRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void SeeAllEvents()
    {
        NavigateAllEventsRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void SeeAllJobs()
    {
        NavigateAllJobsRequested?.Invoke(this, EventArgs.Empty);
    }

    partial void OnCurrentStateChanged(GameState value)
    {
        OnPropertyChanged(nameof(IncompleteGame));
        OnPropertyChanged(nameof(IsStartVisible));
        OnPropertyChanged(nameof(IsChoice1Visible));
        OnPropertyChanged(nameof(IsReaction1Visible));
        OnPropertyChanged(nameof(IsChoice2Visible));
        OnPropertyChanged(nameof(IsReaction2Visible));
        OnPropertyChanged(nameof(IsConclusionVisible));
        OnPropertyChanged(nameof(IsChoiceActive));
        OnPropertyChanged(nameof(IsReactionActive));
    }

    public bool IncompleteGame => CurrentState == GameState.NotCompleted;
    public bool IsStartVisible => CurrentState == GameState.Start;
    public bool IsChoice1Visible => CurrentState == GameState.Choices1;
    public bool IsReaction1Visible => CurrentState == GameState.Reaction1;
    public bool IsChoice2Visible => CurrentState == GameState.Choices2;
    public bool IsReaction2Visible => CurrentState == GameState.Reaction2;
    public bool IsConclusionVisible => CurrentState == GameState.Conclusion;

    public bool IsChoiceActive => IsChoice1Visible || IsChoice2Visible;
    public bool IsReactionActive => IsReaction1Visible || IsReaction2Visible;

    private void UpdateScenario()
    {
        if (currentScenarioIndex < MaximumScenarioCount)
        {
            CurrentQuestion = gameService.ShowScenarioText(currentScenarioIndex);

            CurrentChoices.Clear();
            var choices = gameService.ShowChoices(currentScenarioIndex);
            foreach (var choice in choices)
            {
                CurrentChoices.Add(choice);
            }
        }
    }

    public void GamePreview()
    {
        if (gameService.IsPublished())
        {
            WelcomeMessage = gameService.ShowCoworker();
            CurrentState = GameState.Start;
            currentScenarioIndex = InitialScenarioIndex;
            UpdateScenario();
        }
    }

    [RelayCommand]
    private void RetryGame()
    {
        GamePreview();
    }

    [RelayCommand]
    private void StartGame()
    {
        CurrentState = GameState.Choices1;
    }

    [RelayCommand]
    private void SelectChoice(string? choiceText)
    {
        if (string.IsNullOrEmpty(choiceText) || CurrentChoices == null)
        {
            return;
        }
        int adviceIndex = CurrentChoices.IndexOf(choiceText);
        if (adviceIndex < 0)
        {
            return;
        }
        Feedback = gameService.ChoiceMade(currentScenarioIndex, adviceIndex);
        CurrentState = currentScenarioIndex == InitialScenarioIndex ? GameState.Reaction1 : GameState.Reaction2;
    }

    [RelayCommand]
    private void GoToNextStep()
    {
        currentScenarioIndex++;

        if (currentScenarioIndex < MaximumScenarioCount)
        {
            UpdateScenario();
            CurrentState = GameState.Choices2;
        }
        else
        {
            Feedback = gameService.ShowConclusion();
            CurrentState = GameState.Conclusion;
        }
    }
}