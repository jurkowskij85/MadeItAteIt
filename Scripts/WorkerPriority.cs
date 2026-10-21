using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.EnterableSystem;
using Timberborn.Goods;
using Timberborn.SingletonSystem;
using Timberborn.WorkSystem;
using Timberborn.Workshops;

namespace MadeItAteIt {
  // Attached to every Manufactory. When on and production finishes, one unit of each produced
  // food/water good is eaten on the spot by the neediest worker inside - before anyone can reserve it.
  public class WorkerPriority : BaseComponent, IAwakableComponent, IFinishedStateListener {

    private readonly IGoodService _goodService;
    private readonly EventBus _eventBus;
    private Manufactory _manufactory;
    private Workplace _workplace;
    private Enterable _enterable;
    private WorkersFirstSettings _settings;
    private readonly HashSet<Worker> _servedThisCycle = new HashSet<Worker>();

    private bool IsApplicable => _manufactory != null && _workplace != null && _enterable != null && _settings != null;

    public WorkerPriority(IGoodService goodService, EventBus eventBus) {
      _goodService = goodService;
      _eventBus = eventBus;
    }

    public void Awake() {
      _manufactory = GetComponent<Manufactory>();
      _workplace = GetComponent<Workplace>();
      _enterable = GetComponent<Enterable>();
      _settings = GetComponent<WorkersFirstSettings>();
    }

    public void OnEnterFinishedState() {
      if (IsApplicable) {
        _manufactory.ProductionFinished += OnProductionFinished;
      }
    }

    public void OnExitFinishedState() {
      if (IsApplicable) {
        _manufactory.ProductionFinished -= OnProductionFinished;
      }
    }

    // Raised synchronously by Manufactory.FinishProduction right after Inventory.GiveProduced,
    // inside the same tick, so no other agent has had a chance to reserve the new goods.
    private void OnProductionFinished(object sender, System.EventArgs e) {
      if (!_settings.IsOn || _manufactory.CurrentRecipe == null) {
        return;
      }
      _servedThisCycle.Clear();
      foreach (var product in _manufactory.CurrentRecipe.Products) {
        var good = _goodService.GetGood(product.Id);
        if (!WorkerMealPicker.ServesBasicNeed(good)) {
          continue;
        }
        var unit = new GoodAmount(product.Id, 1);
        if (!_manufactory.Inventory.HasUnreservedStock(unit)) {
          continue;
        }
        var worker = WorkerMealPicker.PickNeediestWorkerInside(_enterable, _workplace, good, _servedThisCycle);
        if (worker == null) {
          continue;
        }
        _manufactory.Inventory.TakeConsumed(unit);
        WorkerMealPicker.ApplyConsumption(worker, good, _eventBus);
        _servedThisCycle.Add(worker);
        UnityEngine.Debug.Log($"[MadeItAteIt] {worker.Name} ate {unit.GoodId} at {Name}");
      }
    }

  }
}
