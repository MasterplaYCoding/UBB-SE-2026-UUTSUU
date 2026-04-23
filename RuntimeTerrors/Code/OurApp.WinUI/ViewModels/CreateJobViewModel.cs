using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OurApp.Core.Models;
using OurApp.Core.Repositories;
using OurApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace OurApp.WinUI.ViewModels;

public partial class SkillPickItem : ObservableObject
{
    public Skill Skill { get; }

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private string _requiredPercentText = "50";

    public SkillPickItem(Skill skill)
    {
        Skill = skill;
    }
}

public partial class CreateJobViewModel : ObservableObject
{
    private readonly IJobsRepository _jobsRepository;
    private readonly SessionService _sessionService;

    public ObservableCollection<SkillPickItem> SkillRows { get; } = new();

    public Action<bool, string>? OnSaveCompleted { get; set; }

    [ObservableProperty] private string _jobTitle = string.Empty;
    [ObservableProperty] private string _industryField = string.Empty;
    [ObservableProperty] private string _jobType = string.Empty;
    [ObservableProperty] private string _experienceLevel = string.Empty;
    [ObservableProperty] private DateTimeOffset? _startDate;
    [ObservableProperty] private DateTimeOffset? _endDate;
    [ObservableProperty] private string _jobDescription = string.Empty;
    [ObservableProperty] private string _jobLocation = string.Empty;
    [ObservableProperty] private double _availablePositions = 1;
    [ObservableProperty] private string _photo = string.Empty;
    [ObservableProperty] private string _salaryText = string.Empty;
    [ObservableProperty] private DateTimeOffset? _deadline;

    public CreateJobViewModel(IJobsRepository jobsRepository, SessionService sessionService)
    {
        _jobsRepository = jobsRepository;
        _sessionService = sessionService;

        foreach (var skillItem in _jobsRepository.GetAllSkills())
        {
            SkillRows.Add(new SkillPickItem(skillItem));
        }
    }

    [RelayCommand]
    public void SaveJob()
    {
        if (_sessionService?.loggedInUser == null)
        {
            return;
        }

        int companyId = _sessionService.loggedInUser.CompanyId;

        int? salary = null;
        if (!string.IsNullOrWhiteSpace(SalaryText)
            && int.TryParse(SalaryText.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsedSalary))
        {
            salary = parsedSalary;
        }

        if (string.IsNullOrWhiteSpace(JobTitle))
        {
            OnSaveCompleted?.Invoke(false, "Job title is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(IndustryField))
        {
            OnSaveCompleted?.Invoke(false, "Industry field is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(JobType))
        {
            OnSaveCompleted?.Invoke(false, "Job type is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(ExperienceLevel))
        {
            OnSaveCompleted?.Invoke(false, "Experience level is required.");
            return;
        }

        if (!StartDate.HasValue || !EndDate.HasValue)
        {
            OnSaveCompleted?.Invoke(false, "Start date and end date are required.");
            return;
        }

        if (EndDate.Value.Date < StartDate.Value.Date)
        {
            OnSaveCompleted?.Invoke(false, "End date must be on or after start date.");
            return;
        }

        if (string.IsNullOrWhiteSpace(JobDescription))
        {
            OnSaveCompleted?.Invoke(false, "Job description is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(JobLocation))
        {
            OnSaveCompleted?.Invoke(false, "Job location is required.");
            return;
        }

        if (AvailablePositions < 1)
        {
            OnSaveCompleted?.Invoke(false, "Available positions must be at least 1.");
            return;
        }

        if (salary.HasValue && salary.Value < 0)
        {
            OnSaveCompleted?.Invoke(false, "Salary cannot be negative.");
            return;
        }

        var links = new List<(int SkillId, int RequiredPercentage)>();
        foreach (var row in SkillRows.Where(r => r.IsSelected))
        {
            if (!int.TryParse(row.RequiredPercentText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var pct)
                && !int.TryParse(row.RequiredPercentText, NumberStyles.Integer, CultureInfo.InvariantCulture, out pct))
            {
                OnSaveCompleted?.Invoke(false, $"Invalid percentage for skill \"{row.Skill.SkillName}\".");
                return;
            }

            if (pct < 1 || pct > 100)
            {
                OnSaveCompleted?.Invoke(false, $"Required percentage for \"{row.Skill.SkillName}\" must be between 1 and 100.");
                return;
            }

            links.Add((row.Skill.SkillId, pct));
        }

        if (links.Count == 0)
        {
            OnSaveCompleted?.Invoke(false, "Select at least one required skill with a valid percentage (1–100).");
            return;
        }

        var job = new JobPosting
        {
            JobTitle = JobTitle.Trim(),
            IndustryField = IndustryField.Trim(),
            JobType = JobType.Trim(),
            ExperienceLevel = ExperienceLevel.Trim(),
            StartDate = StartDate?.DateTime.Date,
            EndDate = EndDate?.DateTime.Date,
            JobDescription = JobDescription.Trim(),
            JobLocation = JobLocation.Trim(),
            AvailablePositions = (int)AvailablePositions,
            Photo = string.IsNullOrWhiteSpace(Photo) ? null : Photo.Trim(),
            PostedAt = DateTime.Now,
            Salary = salary,
            AmountPayed = 0,
            Deadline = Deadline?.DateTime.Date
        };

        try
        {
            var newId = _jobsRepository.AddJob(job, companyId, links);
            OnSaveCompleted?.Invoke(true, $"Job created with id {newId}.");
        }
        catch (Exception ex)
        {
            OnSaveCompleted?.Invoke(false, ex.Message);
        }
    }
}
