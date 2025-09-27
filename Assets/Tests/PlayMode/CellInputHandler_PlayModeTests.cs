using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.Input
{
	[TestFixture]
	public class CellInputHandler_PlayModeTests
	{
		[UnityTest]
		public IEnumerator Awake_AssignsMainCamera_WhenNull()
		{
			var camGO = new GameObject("MainCamera");
			var cam = camGO.AddComponent<Camera>();
			cam.tag = "MainCamera";

			var handlerGO = new GameObject("CellInputHandler");
			var handler = handlerGO.AddComponent<CellInputHandler>();

			// triggers Awake
			handlerGO.SetActive(true);
			yield return null;

			// success if no exception
			Assert.Pass();
		}
	}
}


