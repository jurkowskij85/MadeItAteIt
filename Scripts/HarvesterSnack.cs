using Timberborn.BaseComponentSystem;
using Timberborn.Bots;
using Timberborn.Carrying;
using Timberborn.Fields;
using Timberborn.Gathering;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.NeedSystem;
using Timberborn.SingletonSystem;
using Timberborn.TickSystem;
using Timberborn.WorkSystem;
using Timberborn.Yielding;

namespace MadeItAteIt {
  // Attached to every character with a YielderRemover. A farmer (FarmHouse) or gatherer (GathererFlag)
  // carrying an edible harvest eats one unit straight from the hands as soon as their Hunger/Thirst
  // drops below WorkerMealPicker.NeedPointsThreshold (checked every tick), and also right when the
  // harvest is picked up if they are already below it. The rest is delivered as usual.
  public class HarvesterSnack : TickableComponent, IAwakableComponent {

    private readonly IGoodService _goodService;
    private readonly EventBus _eventBus;
    private YielderRemover _yielderRemover;
    private GoodCarrier _goodCarrier;
    private GoodReserver _goodReserver;
    private Worker _worker;
    private NeedManager _needManager;

    public HarvesterSnack(IGoodService goodService, EventBus eventBus) {
      _goodService = goodService;
      _eventBus = eventBus;
    }

    public void Awake() {
      _yielderRemover = GetComponent<YielderRemover>();
      _goodCarrier = GetComponent<GoodCarrier>();
      _goodReserver = GetComponent<GoodReserver>();
      _worker = GetComponent<Worker>();
      _needManager = GetComponent<NeedManager>();
      if (_yielderRemover != null && _goodCarrier != null && _goodReserver != null && _worker != null
          && _needManager != null && GetComponent<Bot>() == null) {
        _yielderRemover.YieldReservationCompleted += OnYieldReservationCompleted;
      } else {
        DisableComponent();
      }
    }

    // The moment the need drops below the threshold while carrying the harvest.
    public override void Tick() {
      TryEatFromHands(null, "need dropped");
    }

    // Raised by YielderRemover.CompleteReservation right after GoodCarrier.PutGoodsInHands.
    private void OnYieldReservationCompleted(object sender, YieldReservationCompletedEventArgs e) {
      TryEatFromHands(e.Yield.GoodId, "harvested");
    }

    // requiredGoodId: when set, only that good may be eaten (the yield that was just picked up).
    private void TryEatFromHands(string requiredGoodId, string reason) {
      var workplace = _worker.Workplace;
      if (workplace == null || (workplace.GetComponent<FarmHouse>() == null && workplace.GetComponent<GathererFlag>() == null)) {
        return;
      }
      var settings = workplace.GetComponent<WorkersFirstSettings>();
      if (settings == null || !settings.IsOn) {
        return;
      }
      var carried = _goodCarrier.CarriedGood;
      if (carried.IsEmpty) {
        return;
      }
      var goodId = carried.GoodAmount.GoodId;
      if (requiredGoodId != null && goodId != requiredGoodId) {
        return;
      }
      var good = _goodService.GetGood(goodId);
      if (!WorkerMealPicker.ServesBasicNeed(good) || WorkerMealPicker.MissingPoints(_needManager, good) <= 0f) {
        return;
      }
      EatOneFromHands(carried, goodId);
      WorkerMealPicker.ApplyConsumption(_worker, good, _eventBus);
      UnityEngine.Debug.Log($"[MadeItAteIt] {Name} ate {goodId} from hands ({reason})");
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
