using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using OurApp.Core.Models;
using OurApp.Core.Repositories;

namespace OurApp.WinUI.ViewModels;

public sealed class SkillPickItem : INotifyPropertyChanged
{
    public Skill Skill { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    private string _requiredPercentText = "50";
    public string RequiredPercentText
    {
        get => _requiredPercentText;
        set
        {
            if (_requiredPercentText != value)
            {
                _requiredPercentText = value ?? "50";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RequiredPercentText)));
            }
        }
    }

    public SkillPickItem(Skill skill)
    {
        Skill = skill;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class CreateJobViewModel : INotifyPropertyChanged
{
    private readonly IJobsRepository _jobsRepository;

    public ObservableCollection<SkillPickItem> SkillRows { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public CreateJobViewModel(IJobsRepository? jobsRepository = null)
    {
        _jobsRepository = jobsRepository ?? new JobsRepository();
        foreach (var s in _jobsRepository.GetAllSkills())
        {
            SkillRows.Add(new SkillPickItem(s));
        }
    }

    /// <summary>Try to insert job + job_skills. Returns (success, error or success message).</summary>
    public (bool Ok, string Message) TrySave(
        int companyId,
        string jobTitle,
        string industryField,
        string jobType,
        string experienceLevel,
        DateTime? startDate,
        DateTime? endDate,
        string jobDescription,
        string jobLocation,
        int availablePositions,
        string? photo,
        int? salary,
        int? amountPayed,
        DateTime? deadline)
    {
        if (string.IsNullOrWhiteSpace(jobTitle))
        {
            return (false, "Job title is required.");
        }

        if (string.IsNullOrWhiteSpace(industryField))
        {
            return (false, "Industry field is required.");
        }

        if (string.IsNullOrWhiteSpace(jobType))
        {
            return (false, "Job type is required.");
        }

        if (string.IsNullOrWhiteSpace(experienceLevel))
        {
            return (false, "Experience level is required.");
        }

        if (!startDate.HasValue || !endDate.HasValue)
        {
            return (false, "Start date and end date are required.");
        }

        if (endDate.Value.Date < startDate.Value.Date)
        {
            return (false, "End date must be on or after start date.");
        }

        if (string.IsNullOrWhiteSpace(jobDescription))
        {
            return (false, "Job description is required.");
        }

        if (string.IsNullOrWhiteSpace(jobLocation))
        {
            return (false, "Job location is required.");
        }

        if (availablePositions < 1)
        {
            return (false, "Available positions must be at least 1.");
        }

        if (salary.HasValue && salary.Value < 0)
        {
            return (false, "Salary cannot be negative.");
        }

        var ap = amountPayed ?? 0;
        if (ap < 0)
        {
            return (false, "Amount paid cannot be negative.");
        }

        var links = new List<(int SkillId, int RequiredPercentage)>();
        foreach (var row in SkillRows.Where(r => r.IsSelected))
        {
            if (!int.TryParse(row.RequiredPercentText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var pct)
                && !int.TryParse(row.RequiredPercentText, NumberStyles.Integer, CultureInfo.InvariantCulture, out pct))
            {
                return (false, $"Invalid percentage for skill \"{row.Skill.SkillName}\".");
            }

            if (pct < 1 || pct > 100)
            {
                return (false, $"Required percentage for \"{row.Skill.SkillName}\" must be between 1 and 100.");
            }

            links.Add((row.Skill.SkillId, pct));
        }

        if (links.Count == 0)
        {
            return (false, "Select at least one required skill with a valid percentage (1–100).");
        }

        var job = new JobPosting
        {
            JobTitle = jobTitle.Trim(),
            IndustryField = industryField.Trim(),
            JobType = jobType.Trim(),
            ExperienceLevel = experienceLevel.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            JobDescription = jobDescription.Trim(),
            JobLocation = jobLocation.Trim(),
            AvailablePositions = availablePositions,
            Photo = string.IsNullOrWhiteSpace(photo) ? null : photo.Trim(),
            PostedAt = DateTime.Now,
            Salary = salary,
            AmountPayed = ap,
            Deadline = deadline
        };

        try
        {
            var newId = _jobsRepository.AddJob(job, companyId, links);
            return (true, $"Job created with id {newId}.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
