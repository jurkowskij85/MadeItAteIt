using Bindito.Core;
using Timberborn.EntityPanelSystem;
using Timberborn.Fields;
using Timberborn.Gathering;
using Timberborn.TemplateInstantiation;
using Timberborn.Workshops;
using Timberborn.Yielding;

namespace MadeItAteIt {
  [Context("Game")]
  public class MadeItAteItConfigurator : Configurator {

    protected override void Configure() {
      Bind<WorkersFirstSettings>().AsTransient();
      Bind<WorkerPriority>().AsTransient();
      Bind<HarvesterSnack>().AsTransient();
      Bind<WorkersFirstFragment>().AsSingleton();
      Bind<WorkersFirstBatchControl>().AsSingleton();
      MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
      MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
    }

    private static TemplateModule ProvideTemplateModule() {
      var builder = new TemplateModule.Builder();
      // Settings (the two toggles) on every workplace type the mod supports.
      builder.AddDecorator<ManufactorySpec, WorkersFirstSettings>();
      builder.AddDecorator<FarmHouse, WorkersFirstSettings>();
      builder.AddDecorator<GathererFlag, WorkersFirstSettings>();
      // Production buildings: eat from the building's inventory when production finishes.
      builder.AddDecorator<ManufactorySpec, WorkerPriority>();
      // Every character able to harvest/gather: eat from the yield in hands, right on the field/bush.
      builder.AddDecorator<YielderRemover, HarvesterSnack>();
      return builder.Build();
    }

    private class EntityPanelModuleProvider : IProvider<EntityPanelModule> {

      private readonly WorkersFirstFragment _fragment;

      public EntityPanelModuleProvider(WorkersFirstFragment fragment) {
        _fragment = fragment;
      }

      public EntityPanelModule Get() {
        var builder = new EntityPanelModule.Builder();
        builder.AddMiddleFragment(_fragment);
        return builder.Build();
      }

    }

  }
}
