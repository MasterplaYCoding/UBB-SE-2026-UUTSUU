using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OurApp.Core.Models;
using OurApp.Core.Services;
using OurApp.Core.Validators;

namespace OurApp.Core.ViewModels;

public interface IImagePickerService
{
    Task<(string FileName, byte[] Bytes)?> PickImageAsync();
}

public partial class EditCompanyProfileViewModel : ObservableObject
{
    private const string MessageCompanyNotFound = "Company not found.";
    private const string DefaultPhotoFileName = "No image selected";
    private const string DataUriPrefix = "data:image/";
    private const string DataUriBase64Infix = ";base64,";
    private const string ExtensionJpg = "jpg";
    private const string MimeTypeJpeg = "jpeg";
    private const int DefaultCountValue = 0;

    private readonly ICompanyService companyService;
    private readonly IGameService gameService;
    private readonly ICompanyValidator companyValidator;
    private readonly IGameValidator gameValidator;
    private readonly IImagePickerService imagePickerService;

    public Action<byte[]>? OnProfilePreviewRequested { get; set; }
    public Action<byte[]>? OnLogoPreviewRequested { get; set; }

    [ObservableProperty]
    private int companyId;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string aboutUs = string.Empty;

    [ObservableProperty]
    private string profilePicturePath = string.Empty;

    [ObservableProperty]
    private string photoFileName = DefaultPhotoFileName;

    [ObservableProperty]
    private string companyLogoPath = string.Empty;

    [ObservableProperty]
    private string logoFileName = DefaultPhotoFileName;

    [ObservableProperty]
    private string location = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public EditGame EditGame { get; }

    public EditCompanyProfileViewModel(
        ICompanyService companyService,
        IGameService gameService,
        ICompanyValidator companyValidator,
        IGameValidator gameValidator,
        IImagePickerService imagePickerService)
    {
        this.companyService = companyService;
        this.gameService = gameService;
        this.companyValidator = companyValidator;
        this.gameValidator = gameValidator;
        this.imagePickerService = imagePickerService;

        EditGame = new EditGame(this.gameService, this.gameValidator);
    }

    [RelayCommand]
    private async Task PickProfileImageAsync()
    {
        var result = await imagePickerService.PickImageAsync();
        if (result == null)
        {
            return;
        }

        PhotoFileName = result.Value.FileName;
        var extension = System.IO.Path.GetExtension(result.Value.FileName).TrimStart('.').ToLowerInvariant();
        var mimeSubtype = extension == ExtensionJpg ? MimeTypeJpeg : extension;

        ProfilePicturePath = $"{DataUriPrefix}{mimeSubtype}{DataUriBase64Infix}{Convert.ToBase64String(result.Value.Bytes)}";
        OnProfilePreviewRequested?.Invoke(result.Value.Bytes);
    }

    [RelayCommand]
    private async Task PickLogoImageAsync()
    {
        var result = await imagePickerService.PickImageAsync();
        if (result == null)
        {
            return;
        }

        LogoFileName = result.Value.FileName;
        var extension = System.IO.Path.GetExtension(result.Value.FileName).TrimStart('.').ToLowerInvariant();
        var mimeSubtype = extension == ExtensionJpg ? MimeTypeJpeg : extension;

        CompanyLogoPath = $"{DataUriPrefix}{mimeSubtype}{DataUriBase64Infix}{Convert.ToBase64String(result.Value.Bytes)}";
        OnLogoPreviewRequested?.Invoke(result.Value.Bytes);
    }

    public void Load(int companyId)
    {
        CompanyId = companyId;
        StatusMessage = string.Empty;

        Company? existingCompany = companyService.GetCompanyById(companyId);
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

        Company? existingCompany = companyService.GetCompanyById(CompanyId);
        int existingPostedJobsCount = existingCompany?.PostedJobsCount ?? DefaultCountValue;
        int existingCollaboratorsCount = existingCompany?.CollaboratorsCount ?? DefaultCountValue;
        List<string> collaboratorsCopy = existingCompany?.Collaborators ?? new List<string>();

        try
        {
            companyValidator.ValidateName(Name);

            var scenarioTuplesList = EditGame.Scenarios
                .Select(scenario => (
                    scenarioText: scenario.ScenarioText ?? string.Empty,
                    choices: (IReadOnlyList<(string advice, string feedback)>)scenario.Choices
                        .Select(choice => (
                            advice: choice.Advice ?? string.Empty,
                            feedback: choice.Feedback ?? string.Empty))
                        .ToList()))
                .ToList();

            gameValidator.ValidateForActivation(scenarioTuplesList, EditGame.Conclusion ?? string.Empty);

            Game newGame = gameService.CreateGameFromInput(
                buddyId: EditGame.SelectedBuddyId,
                buddyName: EditGame.BuddyName,
                buddyIntroduction: EditGame.BuddyIntroduction,
                scenarios: scenarioTuplesList,
                conclusion: EditGame.Conclusion ?? string.Empty,
                publish: true);

            Company updatedCompany = ToCompany(existingPostedJobsCount, existingCollaboratorsCount);
            updatedCompany.Collaborators = collaboratorsCopy;
            updatedCompany.Game = newGame;

            companyService.UpdateCompany(updatedCompany);
            gameService.Save(newGame);

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

    private readonly IGameService gameService;
    private readonly IGameValidator gameValidator;

    public ObservableCollection<ScenarioInput> Scenarios { get; } = new ObservableCollection<ScenarioInput>();

    public ObservableCollection<int> AvailableBuddyIds { get; } = new ObservableCollection<int> { 0, 1 };

    [ObservableProperty]
    private int selectedBuddyId = DefaultBuddyId;

    public string BuddyImagePath => BuddyImageProvider.GetImagePathById(SelectedBuddyId);

    partial void OnSelectedBuddyIdChanged(int value)
    {
        OnPropertyChanged(nameof(BuddyImagePath));
    }

    [ObservableProperty]
    private string buddyName = string.Empty;

    [ObservableProperty]
    private string buddyIntroduction = string.Empty;

    [ObservableProperty]
    private string conclusion = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public EditGame(IGameService gameService, IGameValidator gameValidator)
    {
        this.gameService = gameService;
        this.gameValidator = gameValidator;

        for (int scenarioIndex = 0; scenarioIndex < RequiredScenariosCount; scenarioIndex++)
        {
            var scenarioInput = new ScenarioInput();

            for (int choiceIndex = 0; choiceIndex < ChoicesPerScenarioCount; choiceIndex++)
            {
                scenarioInput.Choices.Add(new AdviceChoiceInput());
            }

            Scenarios.Add(scenarioInput);
        }

        ApplyLoadedGame(this.gameService.GetStoredGame());
        StatusMessage = string.Empty;
    }

    private void ApplyLoadedGame(Game game)
    {
        if (game == null)
        {
            return;
        }

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
                        .ToList()))
                .ToList();

            gameValidator.ValidateForActivation(scenarioTuplesList, Conclusion ?? string.Empty);

            var newGame = gameService.CreateGameFromInput(
                buddyId: SelectedBuddyId,
                buddyName: BuddyName,
                buddyIntroduction: BuddyIntroduction,
                scenarios: scenarioTuplesList,
                conclusion: Conclusion ?? string.Empty,
                publish: false);

            gameService.Save(newGame);
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
    private string scenarioText = string.Empty;

    public ObservableCollection<AdviceChoiceInput> Choices { get; } = new ObservableCollection<AdviceChoiceInput>();
}

public partial class AdviceChoiceInput : ObservableObject
{
    [ObservableProperty]
    private string advice = string.Empty;

    [ObservableProperty]
    private string feedback = string.Empty;
}