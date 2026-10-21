using Timberborn.BaseComponentSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Localization;
using Timberborn.WorkSystem;
using UnityEngine.UIElements;

namespace MadeItAteIt {
  // Same classes as the game's own toggles (UI.zip: Views/Game/EntityPanel/FlippableDecalFragment.uxml).
  internal class WorkersFirstFragment : IEntityPanelFragment {

    private static readonly string IsOnLocKey = "MadeItAteIt.WorkersFirst";

    private readonly ILoc _loc;
    private VisualElement _root;
    private Toggle _isOnToggle;
    private WorkersFirstSettings _settings;

    public WorkersFirstFragment(ILoc loc) {
      _loc = loc;
    }

    public VisualElement InitializeFragment() {
      _root = new NineSliceVisualElement();
      _root.AddToClassList("entity-sub-panel");
      _root.AddToClassList("bg-sub-box--green");
      _isOnToggle = CreateToggle(IsOnLocKey);
      _isOnToggle.RegisterValueChangedCallback(changeEvent => {
        _settings?.SetIsOn(changeEvent.newValue);
      });
      _root.Add(_isOnToggle);
      _root.ToggleDisplayStyle(false);
      return _root;
    }

    public void ShowFragment(BaseComponent entity) {
      var settings = entity.GetComponent<WorkersFirstSettings>();
      if (settings != null && entity.GetComponent<Workplace>() != null) {
        _settings = settings;
        Refresh();
        _root.ToggleDisplayStyle(true);
      }
    }

    public void ClearFragment() {
      _settings = null;
      _root.ToggleDisplayStyle(false);
    }

    public void UpdateFragment() {
      if (_settings != null) {
        Refresh();
      }
    }

    private Toggle CreateToggle(string locKey) {
      var toggle = new Toggle { text = _loc.T(locKey) };
      toggle.AddToClassList("game-toggle");
      toggle.AddToClassList("entity-panel__text");
      toggle.AddToClassList("entity-panel__toggle");
      return toggle;
    }

    private void Refresh() {
      _isOnToggle.SetValueWithoutNotify(_settings.IsOn);
    }

  }
}
