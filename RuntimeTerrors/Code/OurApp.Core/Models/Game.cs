using System;
using System.Collections.Generic;
using System.Linq;
using OurApp.Core.Models;

public class Game
{
    private const int DefaultBuddyId = 0;

    public Buddy Buddy { get; private set; }
    public IReadOnlyList<Scenario> Scenarios => _scenarios;
    private readonly List<Scenario> _scenarios;

    public string Conclusion { get; private set; }
    public bool IsPublished { get; private set; }

    public Game()
    {
        Buddy = new Buddy(DefaultBuddyId, string.Empty, string.Empty);
        _scenarios = new List<Scenario>();
        Conclusion = string.Empty;
        IsPublished = false;
    }

    public Game(Buddy buddy, IEnumerable<Scenario> scenarioList, string conclusion, bool isPublished = false)
    {
        Buddy = buddy ?? throw new ArgumentNullException(nameof(buddy));
        _scenarios = scenarioList?.ToList() ?? throw new ArgumentNullException(nameof(scenarioList));
        Conclusion = conclusion ?? string.Empty;
        IsPublished = isPublished;
    }
    public Scenario GetScenario(int index)
    {
        return _scenarios[index];
    }

    public void AddScenario(Scenario scenario)
    {
        _scenarios.Add(scenario);
    }

    public void Publish()
    {
        IsPublished = true;
    }

    public void Unpublish()
    {
        IsPublished = false;
    }
}
