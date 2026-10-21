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
  // Attached to every character with a YielderRemover. When a farmer (FarmHouse) or gatherer
  // (GathererFlag) finishes harvesting an edible yield and is hungry enough, one unit is eaten
  // straight from the hands, on the field/bush. The rest is delivered as usual.
  public class HarvesterSnack : BaseComponent, IAwakableComponent {

    private readonly IGoodService _goodService;
    private readonly EventBus _eventBus;
    private YielderRemover _yielderRemover;
    private GoodCarrier _goodCarrier;
    private GoodReserver _goodReserver;
    private Worker _worker;

    public HarvesterSnack(IGoodService goodService, EventBus eventBus) {
      _goodService = goodService;
      _eventBus = eventBus;
    }

    public void Awake() {
      _yielderRemover = GetComponent<YielderRemover>();
      _goodCarrier = GetComponent<GoodCarrier>();
      _goodReserver = GetComponent<GoodReserver>();
      _worker = GetComponent<Worker>();
      if (_yielderRemover != null && _goodCarrier != null && _goodReserver != null && _worker != null
          && GetComponent<Bot>() == null) {
        _yielderRemover.YieldReservationCompleted += OnYieldReservationCompleted;
      }
    }

    // Raised by YielderRemover.CompleteReservation right after GoodCarrier.PutGoodsInHands.
    private void OnYieldReservationCompleted(object sender, YieldReservationCompletedEventArgs e) {
      var workplace = _worker.Workplace;
      if (workplace == null || (workplace.GetComponent<FarmHouse>() == null && workplace.GetComponent<GathererFlag>() == null)) {
        return;
      }
      var settings = workplace.GetComponent<WorkersFirstSettings>();
      if (settings == null || !settings.IsOn) {
        return;
      }
      var carried = _goodCarrier.CarriedGood;
      var goodId = e.Yield.GoodId;
      if (carried.IsEmpty || carried.GoodAmount.GoodId != goodId) {
        return;
      }
      var good = _goodService.GetGood(goodId);
      if (!WorkerMealPicker.ServesBasicNeed(good)) {
        return;
      }
      var needManager = GetComponent<NeedManager>();
      if (needManager == null || WorkerMealPicker.MissingPoints(needManager, good) <= 0f) {
        return;
      }
      EatOneFromHands(carried, goodId);
      WorkerMealPicker.ApplyConsumption(_worker, good, _eventBus);
      UnityEngine.Debug.Log($"[MadeItAteIt] {Name} ate {goodId} while harvesting");
    }

    // CarryRootBehavior.CompleteDelivery gives the receiving inventory CapacityReservation.GoodAmount,
    // not what is in hands - so both must shrink together or the good would be duplicated.
    private void EatOneFromHands(CarriedGood carried, string goodId) {
      var remaining = carried.GoodAmount.Amount - 1;
      var reservation = _goodReserver.CapacityReservation;
      _goodCarrier.EmptyHands();
      if (remaining <= 0) {
        _goodReserver.UnreserveCapacity();
        return;
      }
      var rest = new GoodAmount(goodId, remaining);
      _goodCarrier.PutGoodsInHands(new CarriedGood(rest, carried.Type));
      if (reservation.Inventory != null && reservation.Inventory.Enabled && reservation.GoodAmount.GoodId == goodId) {
        _goodReserver.ReserveCapacity(reservation.Inventory, rest);
      }
    }

  }
}
