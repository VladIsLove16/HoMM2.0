using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace Tests.EditMode.Actions
{
	[TestFixture]
	public class ActionResolver_EditModeTests
	{
		[Test]
		public void Resolve_ReturnsAtLeastOneHandler_ForSimpleContext()
		{
			var model = new GameModel(new UnitModelFactory(), new MovementSystem());
			model.InitializeGrid(3, 3);
			var resolver = new ActionResolver(model, new MovementSystem());
			var ctx = new ActionContext(new Vector2Int(0,0), new Vector2Int(1,0), default, new Vector2Int(0,0));

			Assert.That(resolver.Resolve(ctx, out var handler), Is.True);
			Assert.That(handler, Is.Not.Null);
		}
	}
}


