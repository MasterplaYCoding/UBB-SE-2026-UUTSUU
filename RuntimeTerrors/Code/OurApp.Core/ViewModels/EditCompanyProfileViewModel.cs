using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Core.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OurApp.Core.ViewModels;

/// <summary>
/// Form state for the company edit-profile page. Saves through <see cref="ICompanyService.UpdateCompany"/>.
/// </summary>
public partial class EditCompanyProfileViewModel : ObservableObject
{
    private const string MessageCompanyNotFound = "Company not found.";
    private const int DefaultCountValue = 0;

    private readonly ICompanyService _companyService;
    private readonly IGameService _gameService;
    private readonly ICompanyValidator _companyValidator;
    private readonly IGameValidator _gameValidator;

    [ObservableProperty]
    private int _companyId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _aboutUs = string.Empty;

    [ObservableProperty]
    private string _profilePicturePath = string.Empty;

    [ObservableProperty]
    private string _companyLogoPath = string.Empty;

    [ObservableProperty]
    private string _location = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public EditGame editGame { get; }

    public EditCompanyProfileViewModel(
        ICompanyService companyService,
        IGameService gameService,
        ICompanyValidator companyValidator,
        IGameValidator gameValidator)
    {
        _companyService = companyService;
        _gameService = gameService;
        _companyValidator = companyValidator;
        _gameValidator = gameValidator;

        editGame = new EditGame(_gameService, _gameValidator);
    }

    public void Load(int companyId)
    {
        CompanyId = companyId;
        StatusMessage = string.Empty;

        Company? existingCompany = _companyService.GetCompanyById(companyId);
        if (existingCompany is null)
        {
            StatusMessage = MessageCompanyNotFound;
            return;
        }

        Name = existingCompany.Name;
        AboutUs = existingCompany.AboutUs;
        ProfilePicturePath = existingCompany.ProfilePicturePath;
        CompanyLogoPath = existingCompany.CompanyLogoPath;
        Location = existingCompany.Location;
        Email = existingCompany.Email;
    }

    private Company ToCompany(int postedJobsCount, int collaboratorsCount)
    {
        return new Company(
            name: Name,
            aboutus: AboutUs,
            pfpUrl: ProfilePicturePath,
            logoUrl: CompanyLogoPath,
            location: Location,
            email: Email,
            companyId: CompanyId,
            postedJobsCount: postedJobsCount,
            collaboratorsCount: collaboratorsCount);
    }

    public string? TrySave()
    {
        StatusMessage = string.Empty;

        Company? existingCompany = _companyService.GetCompanyById(CompanyId);
        int existingPostedJobsCount = existingCompany?.PostedJobsCount ?? DefaultCountValue;
        int existingCollaboratorsCount = existingCompany?.CollaboratorsCount ?? DefaultCountValue;
        List<string> collaboratorsCopy = existingCompany?.Collaborators ?? new List<string>();

        try
        {
            _companyValidator.ValidateName(Name);

            var scenarioTuplesList = editGame.Scenarios
                .Select(scenario => (
                    scenarioText: scenario.ScenarioText ?? string.Empty,
                    choices: (IReadOnlyList<(string advice, string feedback)>)scenario.Choices
                        .Select(choice => (
                            advice: choice.Advice ?? string.Empty,
                            feedback: choice.Feedback ?? string.Empty))
                        .ToList()
                ))
                .ToList();

            _gameValidator.ValidateForActivation(scenarioTuplesList, editGame.Conclusion ?? string.Empty);

            Game newGame = _gameService.CreateGameFromInput(
                buddyId: editGame.SelectedBuddyId,
                buddyName: editGame.BuddyName,
                buddyIntroduction: editGame.BuddyIntroduction,
                scenarios: scenarioTuplesList,
                conclusion: editGame.Conclusion ?? string.Empty,
                publish: true
            );

            Company updatedCompany = ToCompany(existingPostedJobsCount, existingCollaboratorsCount);
            updatedCompany.Collaborators = collaboratorsCopy;
            updatedCompany.Game = newGame;

            _companyService.UpdateCompany(updatedCompany);
            _gameService.Save(newGame);

            return null;
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
            return exception.Message;
        }
    }
}

public partial class EditGame : ObservableObject
{
    private const int RequiredScenariosCount = 2;
    private const int ChoicesPerScenarioCount = 3;
    private const int DefaultBuddyId = 1;
    private const string MessageGameCreatedSuccessfully = "Game created and saved successfully.";
    private const string MessageGameCreateFailedPrefix = "Failed to create game: ";

    private readonly IGameService _gameService;
    private readonly IGameValidator _gameValidator;

    public ObservableCollection<ScenarioInput> Scenarios { get; } = new ObservableCollection<ScenarioInput>();

    public ObservableCollection<int> AvailableBuddyIds { get; } = new ObservableCollection<int> { 0, 1 };

    [ObservableProperty]
    private int _selectedBuddyId = DefaultBuddyId;

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

    public EditGame(IGameService gameService, IGameValidator gameValidator)
    {
        _gameService = gameService;
        _gameValidator = gameValidator;

        for (int scenarioIndex = 0; scenarioIndex < RequiredScenariosCount; scenarioIndex++)
        {
            var scenarioInput = new ScenarioInput();

            for (int choiceIndex = 0; choiceIndex < ChoicesPerScenarioCount; choiceIndex++)
            {
                scenarioInput.Choices.Add(new AdviceChoiceInput());
            }

            Scenarios.Add(scenarioInput);
        }

        ApplyLoadedGame(_gameService.GetStoredGame());
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

        for (int scenarioIndex = 0; scenarioIndex < Scenarios.Count && scenarioIndex < game.Scenarios.Count; scenarioIndex++)
        {
            var scenarioViewModel = Scenarios[scenarioIndex];
            var scenarioModel = game.Scenarios[scenarioIndex];
            scenarioViewModel.ScenarioText = scenarioModel.Description ?? string.Empty;

            var adviceChoicesList = scenarioModel.AdviceChoices;
            for (int choiceIndex = 0; choiceIndex < scenarioViewModel.Choices.Count && choiceIndex < adviceChoicesList.Count; choiceIndex++)
            {
                scenarioViewModel.Choices[choiceIndex].Advice = adviceChoicesList[choiceIndex].Advice ?? string.Empty;
                scenarioViewModel.Choices[choiceIndex].Feedback = adviceChoicesList[choiceIndex].Feedback ?? string.Empty;
            }
        }
    }

    [RelayCommand]
    private void CreateGame()
    {
        try
        {
            var scenarioTuplesList = Scenarios
                .Select(scenario => (
                    scenarioText: scenario.ScenarioText ?? string.Empty,
                    choices: (IReadOnlyList<(string advice, string feedback)>)scenario.Choices
                        .Select(choice => (
                            advice: choice.Advice ?? string.Empty,
                            feedback: choice.Feedback ?? string.Empty))
                        .ToList()
                ))
                .ToList();

            _gameValidator.ValidateForActivation(scenarioTuplesList, Conclusion ?? string.Empty);

            var newGame = _gameService.CreateGameFromInput(
                buddyId: SelectedBuddyId,
                buddyName: BuddyName,
                buddyIntroduction: BuddyIntroduction,
                scenarios: scenarioTuplesList,
                conclusion: Conclusion ?? string.Empty,
                publish: false);

            _gameService.Save(newGame);
            StatusMessage = MessageGameCreatedSuccessfully;
        }
        catch (Exception exception)
        {
            StatusMessage = $"{MessageGameCreateFailedPrefix}{exception.Message}";
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