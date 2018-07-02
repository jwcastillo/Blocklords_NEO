using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using AlphaECS;
using Zenject;
using UniRx;
using System;

public class ScreenSpaceCanvasCameraSystem : SystemBehaviour
{
	private IGroup UICanvases;
    //private Group MainCameras;

	[SerializeField] private Camera ScreenSpaceCamera;

	public override void Initialize (IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
	{
		base.Initialize (eventSystem, poolManager, groupFactory);

        Func<IEntity, ReactiveProperty<bool>> isScreenSpaceCanvas = (IEntity entity) =>
        {
            var isScreenSpace = new ReactiveProperty<bool>();
            isScreenSpace.Value = entity.GetComponent<Canvas>().renderMode != RenderMode.WorldSpace;
            return isScreenSpace;
        };
        UICanvases = this.CreateGroup (new HashSet<Type>() { typeof(Canvas) }, isScreenSpaceCanvas);

		//Func<IEntity, ReactiveProperty<bool>> isMainCamera = (IEntity entity) =>
		//{
		//	var isTaggedMainCamera = new ReactiveProperty<bool>();
        //  isTaggedMainCamera.Value = entity.GetComponent<Camera>().tag == StaticVariables.MainCamera;
		//	return isTaggedMainCamera;
		//};
        //MainCameras = this.CreateGroup(new HashSet<Type> { typeof(Camera), typeof(ViewComponent) }, isMainCamera);
	}

	public override void OnEnable ()
	{
		base.OnEnable ();

		UICanvases.OnAdd().Subscribe(entity =>
		{
			var canvas = entity.GetComponent<Canvas>();
            canvas.worldCamera = ScreenSpaceCamera;
		}).AddTo(this.Disposer);
	}
}
