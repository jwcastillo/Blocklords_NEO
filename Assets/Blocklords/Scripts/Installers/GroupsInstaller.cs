using Zenject;

public class GroupsInstaller : MonoInstaller<GroupsInstaller>
{
    public override void InstallBindings()
    {
        Container.Bind<Players>().To<Players>().AsSingle();
        Container.Bind<CorePlayers>().To<CorePlayers>().AsSingle();

        Container.Bind<Heroes>().To<Heroes>().AsSingle();
        Container.Bind<SerializableHeroes>().To<SerializableHeroes>().AsSingle();
        Container.Bind<NonSerializableHeroes>().To<NonSerializableHeroes>().AsSingle();
    }
}
