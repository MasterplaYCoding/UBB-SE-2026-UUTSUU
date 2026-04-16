using System;
using System.Collections.Generic;
using System.Linq;

namespace OurApp.Core.Validators
{
    public class GameValidator
    {
        public const int MaxStruggleOrAdviceLength = 250;

        public bool MandatoryFieldsValidator(
            IReadOnlyList<(string scenarioText, IReadOnlyList<(string advice, string feedback)> choices)> scenarios)
        {
            if (scenarios == null || scenarios.Count != 2)
                throw new Exception("Both scenarios are required to activate the game.");

            for (int i = 0; i < scenarios.Count; i++)
            {
                var (scenarioText, choices) = scenarios[i];
                if (string.IsNullOrWhiteSpace(scenarioText))
                    throw new Exception($"Scenario {i + 1}: the struggle description is mandatory.");

                if (choices == null || choices.Count == 0)
                    throw new Exception($"Scenario {i + 1}: at least one advice option is required.");

                for (int j = 0; j < choices.Count; j++)
                {
                    var (advice, feedback) = choices[j];
                    if (string.IsNullOrWhiteSpace(advice))
                        throw new Exception($"Scenario {i + 1}, option {j + 1}: advice text is mandatory.");
                    if (string.IsNullOrWhiteSpace(feedback))
                        throw new Exception($"Scenario {i + 1}, option {j + 1}: reaction text is mandatory.");
                }
            }

            return true;
        }

        public bool CharacterLimitsValidator(
            IReadOnlyList<(string scenarioText, IReadOnlyList<(string advice, string feedback)> choices)> scenarios)
        {
            if (scenarios == null)
                return true;

            for (int i = 0; i < scenarios.Count; i++)
            {
                var (scenarioText, choices) = scenarios[i];
                if (scenarioText != null && scenarioText.Length > MaxStruggleOrAdviceLength)
                    throw new Exception(
                        $"Scenario {i + 1}: struggle text must be at most {MaxStruggleOrAdviceLength} characters for mobile readability.");

                if (choices == null)
                    continue;

                for (int j = 0; j < choices.Count; j++)
                {
                    var (advice, _) = choices[j];
                    if (advice != null && advice.Length > MaxStruggleOrAdviceLength)
                        throw new Exception(
                            $"Scenario {i + 1}, option {j + 1}: advice must be at most {MaxStruggleOrAdviceLength} characters for mobile readability.");
                }
            }

            return true;
        }

        public bool ConclusionPositiveValidator(string conclusion)
        {
            if (string.IsNullOrWhiteSpace(conclusion))
                throw new Exception("A conclusion is required so the game can end on a positive note.");
             
            return true;
        }

        public bool ValidateForActivation(
            IReadOnlyList<(string scenarioText, IReadOnlyList<(string advice, string feedback)> choices)> scenarios,
            string conclusion)
        {
            MandatoryFieldsValidator(scenarios);
            CharacterLimitsValidator(scenarios);
            ConclusionPositiveValidator(conclusion);
            return true;
        }
    }
}
