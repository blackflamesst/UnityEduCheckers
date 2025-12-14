using Checkers;
using Checkers.Interfaces;
using Checkers.Settings;
using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    private Controls _controls;

    [SerializeField]
    private SceneController _sceneController;

    [SerializeField, Space(15f)]
    private CellPaletteSettings _cellPaletteSettings;

    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<GameEvent>();
        Container.DeclareSignal<GameStatus>();

        Container.Bind<ISharedData>().To<SharedData>().AsSingle();

        _controls = new Controls();
        _controls.Game.Enable();
        Container.BindInstance(_controls.Game).AsSingle();

        Container.BindInterfacesAndSelfTo<Battlefield>().AsSingle();

        Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle();
        Container.BindInterfacesAndSelfTo<BattleController>().AsSingle();
        //Container.BindInstance(_battlefield).AsSingle();
        Container.BindInstance(_sceneController).AsSingle();

        Container.BindInstance(_cellPaletteSettings).AsSingle();

    }

    private void OnDestroy()
    {
        _controls.Dispose();
    }
}