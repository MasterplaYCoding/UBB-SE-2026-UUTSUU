using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Core.Validators;
using System;
using System.Collections.ObjectModel;

namespace OurApp.Core.ViewModels;

/// <summary>
/// Form state for the company edit-profile page. Saves through <see cref="ICompanyService.UpdateCompany"/>.
/// </summary>
public partial class EditCompanyProfileViewModel : ObservableObject
{
    private readonly ICompanyService _companyService;
    private readonly GameService _gameService;
    private readonly CompanyValidator _validator = new();

    [ObservableProperty]
    private int _companyId;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _aboutUs = "";

    [ObservableProperty]
    public string profilePicturePath = "";

    [ObservableProperty]
    public string companyLogoPath = "";

    [ObservableProperty]
    private string _location = "";

    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private string _statusMessage = "";

    public EditGame editGame { get; }

    public EditCompanyProfileViewModel(ICompanyService companyService, GameService gameService)
    {
        _companyService = companyService;

        _gameService = gameService;

        editGame = new EditGame(_gameService);
    }

    public void Load(int companyId)
    {
        CompanyId = companyId;
        StatusMessage = "";
        var c = _companyService.GetCompanyById(companyId);
        if (c is null)
        {
            StatusMessage = "Company not found.";
            return;
        }

        Name = c.Name;
        AboutUs = c.AboutUs;
        ProfilePicturePath = c.ProfilePicturePath;
        CompanyLogoPath = c.CompanyLogoPath;
        Location = c.Location;
        Email = c.Email;
    }

    private Company ToCompany(int postedJobs, int collaborators)
    {
        return new Company(
            name: Name,
            aboutus: AboutUs,
            pfpUrl: ProfilePicturePath,
            logoUrl: CompanyLogoPath,
            location: Location,
            email: Email,
            companyId: CompanyId,
            postedJobsCount: postedJobs,
            collaboratorsCount: collaborators);
    }

   
    public string? TrySave()
    {
        StatusMessage = "";

        var existing = _companyService.GetCompanyById(CompanyId);
        var posted = existing?.PostedJobsCount ?? 0;
        var collab = existing?.CollaboratorsCount ?? 0;
        var copy = existing?.Collaborators ?? new System.Collections.Generic.List<string>();

        try
        {
            _validator.NameValidator(Name);
            
            var scenarioTuples = editGame.Scenarios
                .Select(s => (
                    scenarioText: s.ScenarioText ?? string.Empty,
                    choices: (IReadOnlyList<(string advice, string feedback)>)s.Choices
                        .Select(c => (
                            advice: c.Advice ?? string.Empty,
                            feedback: c.Feedback ?? string.Empty))
                        .ToList()
                ))
                .ToList();

          
            var gameValidator = new GameValidator();
            gameValidator.ValidateForActivation(scenarioTuples, editGame.Conclusion ?? string.Empty);

            
            var game = _gameService.CreateGameFromInput(
                buddyId: editGame.SelectedBuddyId,
                buddyName: editGame.BuddyName,
                buddyIntroduction: editGame.BuddyIntroduction,
                scenarios: scenarioTuples,
                conclusion: editGame.Conclusion ?? string.Empty,
                publish: true
            );

          
            var updated = ToCompany(posted, collab);
            updated.Collaborators = copy;
            updated.game = game; 

          
            _companyService.UpdateCompany(updated);

           
             _gameService.Save(game); 

            return null;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            return ex.Message;
        }
    }
}

    public partial class EditGame : ObservableObject
    {
        private readonly GameService _service;
        private readonly GameValidator _gameValidator = new GameValidator();

        public ObservableCollection<ScenarioInput> Scenarios { get; } = new ObservableCollection<ScenarioInput>();

        public ObservableCollection<int> AvailableBuddyIds { get; } = new ObservableCollection<int> { 0, 1 };

        [ObservableProperty]
        private int _selectedBuddyId = 1;

        public string BuddyImagePath => BuddyImageProvider.GetImagePathById(SelectedBuddyId);

        partial void OnSelectedBuddyIdChanged(int value)
        {
            OnPropertyChanged(nameof(BuddyImagePath));
        }

        [ObservableProperty]
        private string _buddyName = string.Empty;

        [ObservableProperty]
        private string _buddyIntroduction = string.Empty;

        [ObservableProperty]
        private string _conclusion = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public EditGame(GameService service)
        {
            _service = service;

            for (int i = 0; i < 2; i++)
            {
                var s = new ScenarioInput();

                s.Choices.Add(new AdviceChoiceInput());
                s.Choices.Add(new AdviceChoiceInput());
                s.Choices.Add(new AdviceChoiceInput());
                Scenarios.Add(s);
            }

            ApplyLoadedGame(_service.GetStoredGame());
            StatusMessage = string.Empty;
        }

        private void ApplyLoadedGame(Game game)
        {
            if (game == null)
                return;

            SelectedBuddyId = game.Buddy.Id;
            BuddyName = game.Buddy.Name ?? string.Empty;
            BuddyIntroduction = game.Buddy.Introduction ?? string.Empty;
            Conclusion = game.Conclusion ?? string.Empty;

            for (int i = 0; i < Scenarios.Count && i < game.Scenarios.Count; i++)
            {
                var scenarioVm = Scenarios[i];
                var scenarioModel = game.Scenarios[i];
                scenarioVm.ScenarioText = scenarioModel.Description ?? string.Empty;

                var adviceChoices = scenarioModel.AdviceChoices;
                for (int j = 0; j < scenarioVm.Choices.Count && j < adviceChoices.Count; j++)
                {
                    scenarioVm.Choices[j].Advice = adviceChoices[j].Advice ?? string.Empty;
                    scenarioVm.Choices[j].Feedback = adviceChoices[j].Feedback ?? string.Empty;
                }
            }
        }

        [RelayCommand]
        private void CreateGame()
        {
            try
            {
                var scenarioTuples = Scenarios
                    .Select(s => (
                        scenarioText: s.ScenarioText ?? string.Empty,
                        choices: (IReadOnlyList<(string advice, string feedback)>)s.Choices
                            .Select(c => (
                                advice: c.Advice ?? string.Empty,
                                feedback: c.Feedback ?? string.Empty))
                            .ToList()
                    ))
                    .ToList();

                _gameValidator.ValidateForActivation(scenarioTuples, Conclusion ?? string.Empty);

                var game = _service.CreateGameFromInput(
                    buddyId: SelectedBuddyId,
                    buddyName: BuddyName,
                    buddyIntroduction: BuddyIntroduction,
                    scenarios: scenarioTuples,
                    conclusion: Conclusion ?? string.Empty,
                    publish: false);

                _service.Save(game);
                StatusMessage = "Game created and saved successfully.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to create game: {ex.Message}";
            }
        }

    }

public partial class ScenarioInput : ObservableObject
{
    [ObservableProperty]
    private string _scenarioText = string.Empty;

    public ObservableCollection<AdviceChoiceInput> Choices { get; } = new ObservableCollection<AdviceChoiceInput>();
}

public partial class AdviceChoiceInput : ObservableObject
{
    [ObservableProperty]
    private string _advice = string.Empty;

    [ObservableProperty]
    private string _feedback = string.Empty;
}

