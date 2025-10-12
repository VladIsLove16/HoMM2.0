using NUnit.Framework;
using System.Numerics;
using UnityEngine.Rendering.VirtualTexturing;
namespace Tests.EditMode.ActionHandlers
{
    [TestFixture]
    public class ActionResolverTests
    {
        private ActionResolver _resolver;
        private GameModel _gameModel;
        private MovementSystem  _movementSystem;
        [SetUp]
        public void Setup()
        {
            _movementSystem =new MovementSystem();
            _gameModel = new(new(), _movementSystem);
            _resolver = new(_gameModel, _movementSystem);
        }
        [Test]
        public void ActionResolver_Resolve_TriggersActionResolveEvent()
        {
            bool resolved = false;
            _resolver.ActionResolved += (action) => resolved = true;


            Assert.IsTrue(resolved);
        }

    }
}