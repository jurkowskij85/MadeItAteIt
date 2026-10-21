using System.Collections.Generic;

namespace MadeItAteIt {
  // All WorkersFirstSettings components alive in the game. Destroyed ones are pruned lazily.
  public class WorkersFirstRegistry {

    private readonly List<WorkersFirstSettings> _settings = new List<WorkersFirstSettings>();

    public void Add(WorkersFirstSettings settings) {
      _settings.Add(settings);
    }

    public IReadOnlyList<WorkersFirstSettings> GetAlive() {
      _settings.RemoveAll(s => !s);
      return _settings;
    }

  }
}
