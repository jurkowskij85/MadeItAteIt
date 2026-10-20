using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.EnterableSystem;
using Timberborn.Goods;
using Timberborn.SingletonSystem;
using Timberborn.TickSystem;
using Timberborn.WorkSystem;
using Timberborn.Workshops;

namespace MadeItAteIt {
  // Attached to every Manufactory. When on, a worker inside the building takes one unit of the
  // food/water the building produces as soon as their Hunger/Thirst drops below
  // WorkerMealPicker.NeedPointsThreshold (checked every tick). If the building has nothing in stock
  // at that moment, the worker eats the moment production finishes - before anyone can reserve it.
  public class WorkerPriority : TickableComponent, IAwakableComponent, IFinishedStateListener {

    private readonly IGoodService _goodService;
    private readonly EventBus _eventBus;
    private Manufactory _manufactory;
    private Workplace _workplace;
    private Enterable _enterable;
    private WorkersFirstSettings _settings;
    private bool _isFinished;
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
      if (!IsApplicable) {
        DisableComponent();
      }
    }

    public void OnEnterFinishedState() {
      if (IsApplicable) {
        _isFinished = true;
        _manufactory.ProductionFinished += OnProductionFinished;
      }
    }

    public void OnExitFinishedState() {
      if (IsApplicable) {
        _isFinished = false;
        _manufactory.ProductionFinished -= OnProductionFinished;
      }
    }

    // The moment a worker's need drops below the threshold: eat from what is already in stock.
    public override void Tick() {
      if (_isFinished) {
        ServeHungryWorkers("need dropped");
      }
    }

    // Raised synchronously by Manufactory.FinishProduction right after Inventory.GiveProduced,
    // inside the same tick, so no other agent has had a chance to reserve the new goods.
    private void OnProductionFinished(object sender, System.EventArgs e) {
      ServeHungryWorkers("production finished");
    }

    // For each produced food/water good, the neediest worker inside (below the threshold) eats one unit.
    // A worker eats at most once per call.
    private void ServeHungryWorkers(string reason) {
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
        UnityEngine.Debug.Log($"[MadeItAteIt] {worker.Name} ate {unit.GoodId} at {Name} ({reason})");
      }
    }

  }
}
