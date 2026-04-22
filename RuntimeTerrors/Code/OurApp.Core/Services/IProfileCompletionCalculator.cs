using System.Collections.Generic;
using OurApp.Core.Models;

namespace OurApp.Core.Services
{
    public interface IProfileCompletionCalculator
    {
        (int percentage, List<string> remainingTasks) Calculate(Company company);

        (List<string> skillNames, List<int> percents) GetSkillsTop3(int companyId);

        string applicantsMessage(int companyId);
    }
}