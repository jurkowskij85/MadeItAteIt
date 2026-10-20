using Timberborn.BaseComponentSystem;
using Timberborn.Persistence;
using Timberborn.WorldPersistence;

namespace MadeItAteIt {
  // Per-building player setting. Default (off) is not written to the save.
  public class WorkersFirstSettings : BaseComponent, IPersistentEntity {

    private static readonly ComponentKey SettingsKey = new ComponentKey("MadeItAteIt.WorkersFirst");
    private static readonly PropertyKey<bool> IsOnKey = new PropertyKey<bool>("IsOn");

    // "Pracownicy mają pierwszeństwo"
    public bool IsOn { get; private set; }

    public void SetIsOn(bool isOn) {
      IsOn = isOn;
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
