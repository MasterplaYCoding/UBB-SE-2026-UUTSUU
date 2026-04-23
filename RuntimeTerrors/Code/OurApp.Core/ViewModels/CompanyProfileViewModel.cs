using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

    private readonly ICompanyService _companyService;
    private readonly IGameService _gameService;
    private readonly IEventsService _eventsService;
    private readonly IJobsRepository _jobsRepository;
    private readonly SessionService _sessionService;
    private readonly ICollaboratorsService _collaboratorsService;
    private readonly IProfileCompletionCalculator _calculator;

    private int _currentScenarioIndex;

    public Action<byte[]>? OnProfileImageDecoded { get; set; }
    public Action? OnProfileImageCleared { get; set; }
    public Action<byte[]>? OnLogoDecoded { get; set; }
    public Action? OnLogoCleared { get; set; }

    [ObservableProperty]
    private string _profilePictureHintText = string.Empty;

    [ObservableProperty]
    private string _companyLogoHintText = string.Empty;

    [ObservableProperty]
    private string _currentQuestion = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _currentChoices = new();

    [ObservableProperty]
    private string _feedback = string.Empty;

    public string BuddyImagePath => BuddyImageProvider.GetImagePathById(_gameService.GetBuddyId());

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private GameState _currentState = GameState.NotCompleted;

    [ObservableProperty]
    private Company? _company;

    [ObservableProperty]
    private string _loadMessage = string.Empty;

    [ObservableProperty]
    private int _completionPercentage;

    [ObservableProperty]
    private int _completedTasksCount;

    [ObservableProperty]
    private ObservableCollection<string> _remainingTasks = new();

    [ObservableProperty]
    private string _applicantSummary = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyTrendingSkillRow> _trendingSkills = new();

    public IEnumerable<CompanyProfileListRow> Top3JobPreviews =>
        _jobsRepository
            .GetAllJobs()
            .Take(MaximumTopJobsCount)
            .Select(job => new CompanyProfileListRow
            {
                Title = job.JobTitle,
                Subtitle = job.JobDescription
            });

    public IEnumerable<CompanyProfileListRow> Top3EventPreviews =>
        _eventsService
            .GetCurrentEvents(_sessionService.loggedInUser.CompanyId)
            .Take(MaximumTopEventsCount)
            .Select(eventItem => new CompanyProfileListRow
            {
                Title = eventItem.Title,
                Subtitle = eventItem.Description
            });

    public IEnumerable<CompanyCollabListRow> Top3CollabsPreviews =>
        _collaboratorsService
            .GetAllCollaborators(_sessionService.loggedInUser.CompanyId)
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
        _gameService = gameService;
        _companyService = companyService;
        _calculator = calculator;
        _eventsService = eventService;
        _sessionService = sessionService;
        _collaboratorsService = collaboratorsService;
        _jobsRepository = jobsRepository;
    }

    public void Load(int companyId)
    {
        CompanyId = companyId;
        Company = _companyService.GetCompanyById(companyId);
        if (Company is null)
        {
            LoadMessage = ProfileLoadErrorMessage;
            CompletionPercentage = EmptyCompletionPercentage;
            CompletedTasksCount = EmptyTaskCount;
            RemainingTasks.Clear();
            return;
        }

        ApplicantSummary = _calculator.applicantsMessage(companyId);

        LoadMessage = string.Empty;
        RefreshProfileStatistics();
        FillPreviewSections();
        ProcessImages();
        gamePreview();
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
            return;

        var (percentage, tasks) = _calculator.Calculate(Company);
        CompletionPercentage = percentage;
        CompletedTasksCount = TotalProfileTasksCount - tasks.Count;
        if (CompletedTasksCount < EmptyTaskCount)
            CompletedTasksCount = EmptyTaskCount;

        RemainingTasks.Clear();
        foreach (var task in tasks)
            RemainingTasks.Add(task);
    }

    private void FillPreviewSections()
    {
        if (Company is null)
            return;

        TrendingSkills.Clear();
        var (skillNames, percents) = _calculator.GetSkillsTop3(Company.CompanyId);

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
        if (_currentScenarioIndex < MaximumScenarioCount)
        {
            CurrentQuestion = _gameService.ShowScenarioText(_currentScenarioIndex);

            CurrentChoices.Clear();
            var choices = _gameService.ShowChoices(_currentScenarioIndex);
            foreach (var choice in choices)
                CurrentChoices.Add(choice);
        }
    }

    public void gamePreview()
    {
        if (_gameService.IsPublished())
        {
            WelcomeMessage = _gameService.ShowCoworker();
            CurrentState = GameState.Start;
            _currentScenarioIndex = InitialScenarioIndex;
            UpdateScenario();
        }
    }

    [RelayCommand]
    private void RetryGame()
    {
        gamePreview();
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
            return;

        int adviceIndex = CurrentChoices.IndexOf(choiceText);
        if (adviceIndex < 0)
            return;

        Feedback = _gameService.ChoiceMade(_currentScenarioIndex, adviceIndex);
        CurrentState = _currentScenarioIndex == InitialScenarioIndex ? GameState.Reaction1 : GameState.Reaction2;
    }

    [RelayCommand]
    private void GoToNextStep()
    {
        _currentScenarioIndex++;

        if (_currentScenarioIndex < MaximumScenarioCount)
        {
            UpdateScenario();
            CurrentState = GameState.Choices2;
        }
        else
        {
            Feedback = _gameService.ShowConclusion();
            CurrentState = GameState.Conclusion;
        }
    }
}