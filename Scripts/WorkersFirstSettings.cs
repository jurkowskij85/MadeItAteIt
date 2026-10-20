using System;
using Timberborn.BaseComponentSystem;
using Timberborn.DuplicationSystem;
using Timberborn.Persistence;
using Timberborn.WorldPersistence;

namespace MadeItAteIt {
  // Per-building player setting. Default (off) is not written to the save.
  // IDuplicable: copied by the game's "copy settings" tool and when a building is duplicated.
  public class WorkersFirstSettings : BaseComponent, IPersistentEntity, IDuplicable<WorkersFirstSettings> {

    private static readonly ComponentKey SettingsKey = new ComponentKey("MadeItAteIt.WorkersFirst");
    private static readonly PropertyKey<bool> IsOnKey = new PropertyKey<bool>("IsOn");

    // Raised whenever IsOn changes (panel toggle, batch control list, duplication).
    public event EventHandler IsOnChanged;

    // "Pracownicy mają pierwszeństwo"
    public bool IsOn { get; private set; }

    public void SetIsOn(bool isOn) {
      if (IsOn == isOn) {
        return;
      }
      IsOn = isOn;
      IsOnChanged?.Invoke(this, EventArgs.Empty);
    }

    // IDuplicable: the setting is always copyable (on and off both get transferred).
    public bool IsDuplicable => true;

    public void DuplicateFrom(WorkersFirstSettings source) {
      SetIsOn(source.IsOn);
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
