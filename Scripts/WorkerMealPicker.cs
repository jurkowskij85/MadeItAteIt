using System.Collections.Generic;
using Timberborn.Bots;
using Timberborn.Effects;
using Timberborn.EnterableSystem;
using Timberborn.Goods;
using Timberborn.InventoryNeedSystem;
using Timberborn.NeedSpecs;
using Timberborn.NeedSystem;
using Timberborn.SingletonSystem;
using Timberborn.WorkSystem;

namespace MadeItAteIt {
  internal static class WorkerMealPicker {

    // Need ids from Needs/Need.Beaver.Hunger.blueprint.json and Need.Beaver.Thirst.blueprint.json.
    private static readonly HashSet<string> BasicNeedIds = new HashSet<string> { "Hunger", "Thirst" };

    // The mod kicks in when the need's points drop below this value (needs range -3..1 in the blueprints).
    public const float NeedPointsThreshold = 0.1f;

    public static bool ServesBasicNeed(GoodSpec good) {
      foreach (InstantEffectSpec effect in good.ConsumptionEffects) {
        if (BasicNeedIds.Contains(effect.NeedId)) {
          return true;
        }
      }
      return false;
    }

    // Worker of this workplace, physically inside, not a bot, with Hunger/Thirst below the threshold.
    // Returns the one with the most points missing to max, or null.
    public static Worker PickNeediestWorkerInside(Enterable enterable, Workplace workplace, GoodSpec good,
                                                  ISet<Worker> excluded) {
      Worker best = null;
      var bestMissing = 0f;
      foreach (Enterer enterer in enterable.EnterersInside) {
        var worker = enterer.GetComponent<Worker>();
        if (worker == null || worker.Workplace != workplace || excluded.Contains(worker)) {
          continue;
        }
        if (enterer.GetComponent<Bot>() != null) {
          continue;
        }
        var needManager = enterer.GetComponent<NeedManager>();
        if (needManager == null) {
          continue;
        }
        var missing = MissingPoints(needManager, good);
        if (missing > bestMissing) {
          bestMissing = missing;
          best = worker;
        }
      }
      return best;
    }

    // Points missing to max for the good's Hunger/Thirst effects, counted only when the need's
    // points are below NeedPointsThreshold. 0 = does not need it.
    public static float MissingPoints(NeedManager needManager, GoodSpec good) {
      var missing = 0f;
      foreach (InstantEffectSpec effect in good.ConsumptionEffects) {
        var needId = effect.NeedId;
        if (!BasicNeedIds.Contains(needId) || !needManager.HasNeed(needId) || !needManager.NeedIsEnabled(needId)) {
          continue;
        }
        if (needManager.GetNeedPoints(needId) < NeedPointsThreshold) {
          missing = System.Math.Max(missing, needManager.NeedPointsToMax(needId));
        }
      }
      return missing;
    }

    // Same as InventoryNeedBehavior.ConsumeGood (minus taking the good): apply effects, post the event.
    public static void ApplyConsumption(Worker worker, GoodSpec good, EventBus eventBus) {
      var needManager = worker.GetComponent<NeedManager>();
      foreach (InstantEffectSpec effectSpec in good.ConsumptionEffects) {
        var effect = InstantEffect.FromSpec(effectSpec, 1);
        needManager.ApplyEffect(in effect);
      }
      eventBus.Post(new GoodConsumedEvent(good.Id));
    }

  }
}
