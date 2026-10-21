using Timberborn.BaseComponentSystem;
using Timberborn.DuplicationSystem;
using Timberborn.Persistence;
using Timberborn.WorldPersistence;

namespace MadeItAteIt {
  // Per-building player setting. Default (off) is not written to the save.
  // IDuplicable: "Powiel" (duplicate building) and "Kopiuj ustawienia" carry the toggle over
  // to any target building that also has this component.
  public class WorkersFirstSettings : BaseComponent, IAwakableComponent, IPersistentEntity,
                                      IDuplicable<WorkersFirstSettings> {

    private static readonly ComponentKey SettingsKey = new ComponentKey("MadeItAteIt.WorkersFirst");
    private static readonly PropertyKey<bool> IsOnKey = new PropertyKey<bool>("IsOn");

    private readonly WorkersFirstRegistry _registry;

    public WorkersFirstSettings(WorkersFirstRegistry registry) {
      _registry = registry;
    }

    // "Pracownicy mają pierwszeństwo"
    public bool IsOn { get; private set; }

    public bool IsDuplicable => true;

    public void Awake() {
      _registry.Add(this);
    }

    public void SetIsOn(bool isOn) {
      IsOn = isOn;
    }

    public void DuplicateFrom(WorkersFirstSettings source) {
      IsOn = source.IsOn;
    }

    public void Save(IEntitySaver entitySaver) {
      if (IsOn) {
        entitySaver.GetComponent(SettingsKey).Set(IsOnKey, true);
      }
    }

    public void Load(IEntityLoader entityLoader) {
      if (entityLoader.TryGetComponent(SettingsKey, out var loader) && loader.Has(IsOnKey)) {
        IsOn = loader.Get(IsOnKey);
      }
    }

  }
}
