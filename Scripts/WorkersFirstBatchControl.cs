using HarmonyLib;
using Timberborn.BatchControl;
using Timberborn.Localization;
using Timberborn.ModManagerScene;
using Timberborn.SingletonSystem;
using Timberborn.WorkplacesBatchControl;
using UnityEngine.UIElements;

namespace MadeItAteIt {
  // Adds the "Workers get priority" toggle to every row of the Workplaces tab in the building list
  // (batch control box), so the setting can be changed without opening each building.
  // The game builds those rows in WorkplacesBatchControlRowFactory.Create; a Harmony postfix appends
  // our toggle to the row. Rows are rebuilt each time the tab is opened.
  public class WorkersFirstBatchControl : ILoadableSingleton, IUnloadableSingleton {

    private static readonly string ShortLocKey = "MadeItAteIt.WorkersFirst.Short";
    private static readonly string TooltipLocKey = "MadeItAteIt.WorkersFirst";

    internal static WorkersFirstBatchControl Instance { get; private set; }

    private readonly ILoc _loc;

    public WorkersFirstBatchControl(ILoc loc) {
      _loc = loc;
    }

    public void Load() {
      Instance = this;
    }

    public void Unload() {
      Instance = null;
    }

    internal void AddToggle(BatchControlRow row) {
      var settings = row.Entity.GetComponent<WorkersFirstSettings>();
      if (settings == null) {
        return;
      }
      var toggle = new Toggle { text = _loc.T(ShortLocKey), tooltip = _loc.T(TooltipLocKey) };
      toggle.AddToClassList("game-toggle");
      toggle.AddToClassList("entity-panel__text");
      toggle.style.flexShrink = 0;
      toggle.style.marginLeft = 8;
      toggle.SetValueWithoutNotify(settings.IsOn);
      toggle.RegisterValueChangedCallback(changeEvent => settings.SetIsOn(changeEvent.newValue));
      // The row itself is clickable (selects the building); clicking the toggle should only toggle.
      toggle.RegisterCallback<ClickEvent>(clickEvent => clickEvent.StopPropagation());

      // Keep the toggle in sync when the setting changes elsewhere (building panel, copy settings).
      System.EventHandler refresh = (sender, e) => {
        toggle.SetValueWithoutNotify(settings.IsOn);
      };
      toggle.RegisterCallback<AttachToPanelEvent>(_ => {
        settings.IsOnChanged -= refresh;
        settings.IsOnChanged += refresh;
        toggle.SetValueWithoutNotify(settings.IsOn);
      });
      toggle.RegisterCallback<DetachFromPanelEvent>(_ => {
        settings.IsOnChanged -= refresh;
      });

      row.Root.Add(toggle);
    }

  }

  [HarmonyPatch(typeof(WorkplacesBatchControlRowFactory), nameof(WorkplacesBatchControlRowFactory.Create))]
  internal static class WorkplacesBatchControlRowFactoryPatch {

    [HarmonyPostfix]
    public static void Postfix(BatchControlRow __result) {
      if (__result != null) {
        WorkersFirstBatchControl.Instance?.AddToggle(__result);
      }
    }

  }

  // Applies the Harmony patches once, when the game loads the mod.
  public class MadeItAteItStarter : IModStarter {

    public void StartMod(IModEnvironment modEnvironment) {
      new Harmony("Jurkowski.MadeItAteIt").PatchAll(typeof(MadeItAteItStarter).Assembly);
    }

  }
}
