using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.Bots;
using Timberborn.Carrying;
using Timberborn.Fields;
using Timberborn.Gathering;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.NeedSystem;
using Timberborn.SingletonSystem;
using Timberborn.WorkSystem;
using Timberborn.Yielding;

namespace MadeItAteIt {
  // Attached to every character with a YielderRemover.
  //
  // Farmers (FarmHouse) and gatherers (GathererFlag) carry the harvest back as usual. The moment it
  // lands in their workplace's inventory, a worker whose Hunger/Thirst is below the threshold eats
  // one unit of it. This happens inside the game's CarryRootBehavior.CompleteDelivery
  // (Inventory.Give* -> GoodCarrier.EmptyHands), in the same call, so haulers have no chance to
  // reserve the goods first.
  public class HarvesterSnack : BaseComponent, IAwakableComponent {

    private readonly IGoodService _goodService;
    private readonly EventBus _eventBus;
    private YielderRemover _yielderRemover;
    private GoodCarrier _goodCarrier;
    private Worker _worker;
    private NeedManager _needManager;

    // Delivery tracking (set after an edible harvest, cleared when hands empty).
    private readonly List<Inventory> _watchedInventories = new List<Inventory>();
    private string _harvestGoodId;
    private Inventory _deliveredTo;
    private int _deliveredFrame = -1;

    public HarvesterSnack(IGoodService goodService, EventBus eventBus) {
      _goodService = goodService;
      _eventBus = eventBus;
    }

    public void Awake() {
      _yielderRemover = GetComponent<YielderRemover>();
      _goodCarrier = GetComponent<GoodCarrier>();
      _worker = GetComponent<Worker>();
      _needManager = GetComponent<NeedManager>();
      if (_yielderRemover != null && _goodCarrier != null && _worker != null
          && _needManager != null && GetComponent<Bot>() == null) {
        _yielderRemover.YieldReservationCompleted += OnYieldReservationCompleted;
        _goodCarrier.CarriedGoodsChanged += OnCarriedGoodsChanged;
      }
    }

    // Raised by YielderRemover.CompleteReservation right after GoodCarrier.PutGoodsInHands.
    private void OnYieldReservationCompleted(object sender, YieldReservationCompletedEventArgs e) {
      var workplace = _worker.Workplace;
      if (workplace == null || !IsOn(workplace)) {
        return;
      }
      var goodId = e.Yield.GoodId;
      var good = _goodService.GetGood(goodId);
      if (!WorkerMealPicker.ServesBasicNeed(good)) {
        return;
      }
      if (workplace.GetComponent<FarmHouse>() != null || workplace.GetComponent<GathererFlag>() != null) {
        // Eat later, when the harvest is delivered to the workplace.
        StartWatching(workplace, goodId);
      }
    }

    // ---- Eat on delivery to the workplace ----

    private void StartWatching(Workplace workplace, string goodId) {
      StopWatching();
      _harvestGoodId = goodId;
      foreach (var inventory in workplace.GetComponentsAllocating<Inventory>()) {
        inventory.InventoryStockChanged += OnWorkplaceStockChanged;
        _watchedInventories.Add(inventory);
      }
    }

    private void StopWatching() {
      foreach (var inventory in _watchedInventories) {
        if (inventory != null) {
          inventory.InventoryStockChanged -= OnWorkplaceStockChanged;
        }
      }
      _watchedInventories.Clear();
      _harvestGoodId = null;
      _deliveredTo = null;
      _deliveredFrame = -1;
    }

    // Raised by Inventory.GiveProduced/GiveExisting inside CompleteDelivery, just before EmptyHands.
    private void OnWorkplaceStockChanged(object sender, InventoryStockChangedEventArgs e) {
      if (_harvestGoodId == null || !_goodCarrier.IsCarrying || _goodCarrier.CarriedGoodId != _harvestGoodId) {
        return;
      }
      var amount = e.GoodAmount;
      if (amount.GoodId == _harvestGoodId && amount.Amount > 0) {
        _deliveredTo = sender as Inventory;
        _deliveredFrame = UnityEngine.Time.frameCount;
      }
    }

    // Raised by GoodCarrier.SetCarriedGood (PutGoodsInHands / EmptyHands).
    private void OnCarriedGoodsChanged(object sender, CarriedGoodsChangedEventArgs e) {
      if (_harvestGoodId == null) {
        return;
      }
      if (!e.CarriedGood.IsEmpty) {
        if (e.CarriedGood.GoodAmount.GoodId != _harvestGoodId) {
          StopWatching();
        }
        return;
      }
      // Hands just emptied. If it was our delivery into the workplace (same frame), eat from it now.
      var inventory = _deliveredTo;
      var goodId = _harvestGoodId;
      var delivered = inventory != null && _deliveredFrame == UnityEngine.Time.frameCount;
      StopWatching();
      if (delivered) {
        EatFromWorkplaceIfHungry(inventory, goodId);
      }
    }

    private void EatFromWorkplaceIfHungry(Inventory inventory, string goodId) {
      var workplace = _worker.Workplace;
      if (workplace == null || !IsOn(workplace) || !inventory.Enabled) {
        return;
      }
      var good = _goodService.GetGood(goodId);
      if (WorkerMealPicker.MissingPoints(_needManager, good) <= 0f) {
        return;
      }
      var unit = new GoodAmount(goodId, 1);
      if (!inventory.HasUnreservedStock(unit)) {
        return;
      }
      inventory.TakeConsumed(unit);
      WorkerMealPicker.ApplyConsumption(_worker, good, _eventBus);
      UnityEngine.Debug.Log($"[MadeItAteIt] {Name} ate {goodId} on delivery to {workplace.Name}");
    }

    private static bool IsOn(Workplace workplace) {
      var settings = workplace.GetComponent<WorkersFirstSettings>();
      return settings != null && settings.IsOn;
    }

  }
}
