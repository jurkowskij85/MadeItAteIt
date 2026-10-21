using System.Collections.Generic;
using System.IO;
using Timberborn.BatchControl;
using Timberborn.EntitySystem;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace MadeItAteIt {
  // Adds the "Workers get priority" toggle button to rows of the "Workplaces" batch control tab.
  // The game's row factory is closed, so the button is appended to each row's Root after the
  // tab builds its rows, and kept in sync while the tab is visible.
  public class WorkersFirstBatchControl : ILoadableSingleton, IUpdatableSingleton {

    private const string WorkplacesTabTypeName = "WorkplacesBatchControlTab";
    private const string ElementName = "MadeItAteIt.WorkersFirstRowItem";
    private const string TooltipLocKey = "MadeItAteIt.WorkersFirst";
    private const string IconResource = "MadeItAteIt.WorkersFirstIcon.png";
    private const int RescanInterval = 20;

    private readonly EventBus _eventBus;
    private readonly ToggleButtonBatchControlRowItemFactory _toggleFactory;
    private readonly WorkersFirstRegistry _registry;
    private readonly Dictionary<BatchControlRow, IBatchControlRowItem> _items =
        new Dictionary<BatchControlRow, IBatchControlRowItem>();
    private BatchControlTab _workplacesTab;
    private bool _visible;
    private int _framesToRescan;
    private Texture2D _icon;

    public WorkersFirstBatchControl(EventBus eventBus,
                                    ToggleButtonBatchControlRowItemFactory toggleFactory,
                                    WorkersFirstRegistry registry) {
      _eventBus = eventBus;
      _toggleFactory = toggleFactory;
      _registry = registry;
    }

    public void Load() {
      _eventBus.Register(this);
      _icon = LoadIcon();
    }

    [OnEvent]
    public void OnBatchControlTabShown(BatchControlTabShownEvent e) {
      var tab = e.BatchControlTab;
      _visible = tab != null && tab.GetType().Name == WorkplacesTabTypeName;
      if (_visible) {
        _workplacesTab = tab;
        _framesToRescan = 0;
      }
    }

    [OnEvent]
    public void OnBatchControlBoxHidden(BatchControlBoxHiddenEvent e) {
      _visible = false;
      _items.Clear();
    }

    public void UpdateSingleton() {
      if (!_visible || _workplacesTab == null) {
        return;
      }
      if (--_framesToRescan <= 0) {
        _framesToRescan = RescanInterval;
        Rescan();
      }
      foreach (var item in _items.Values) {
        (item as IUpdatableBatchControlRowItem)?.UpdateRowItem();
      }
    }

    private void Rescan() {
      var seen = new Dictionary<BatchControlRow, IBatchControlRowItem>();
      foreach (var settings in _registry.GetAlive()) {
        var entity = settings.GetComponent<EntityComponent>();
        if (!entity) {
          continue;
        }
        foreach (var row in _workplacesTab.GetEntityRows(entity)) {
          if (!_items.TryGetValue(row, out var item) || item.Root.parent != row.Root) {
            item = CreateItem(settings);
            row.Root.Add(item.Root);
          }
          // Rows add their own items when first shown - keep ours as the last one.
          if (row.Root.IndexOf(item.Root) != row.Root.childCount - 1) {
            item.Root.BringToFront();
          }
          seen[row] = item;
        }
      }
      _items.Clear();
      foreach (var pair in seen) {
        _items[pair.Key] = pair.Value;
      }
    }

    private IBatchControlRowItem CreateItem(WorkersFirstSettings settings) {
      var item = _toggleFactory.Create("madeitateit-workers-first",
                                       () => settings.SetIsOn(!settings.IsOn),
                                       () => settings.IsOn,
                                       TooltipLocKey);
      item.Root.name = ElementName;
      if (_icon != null) {
        var button = item.Root.Q<VisualElement>("ToggleButtonBatchControlRowItem") ?? item.Root;
        var icon = new VisualElement { pickingMode = PickingMode.Ignore };
        icon.style.position = Position.Absolute;
        icon.style.left = new Length(18, LengthUnit.Percent);
        icon.style.top = new Length(18, LengthUnit.Percent);
        icon.style.width = new Length(64, LengthUnit.Percent);
        icon.style.height = new Length(64, LengthUnit.Percent);
        icon.style.backgroundImage = new StyleBackground(_icon);
        button.Add(icon);
      }
      (item as IUpdatableBatchControlRowItem)?.UpdateRowItem();
      return item;
    }

    private static Texture2D LoadIcon() {
      using (var stream = typeof(WorkersFirstBatchControl).Assembly.GetManifestResourceStream(IconResource)) {
        if (stream == null) {
          Debug.LogWarning("[MadeItAteIt] Batch control icon resource missing");
          return null;
        }
        var memory = new MemoryStream();
        stream.CopyTo(memory);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(memory.ToArray());
        texture.filterMode = FilterMode.Bilinear;
        return texture;
      }
    }

  }
}
