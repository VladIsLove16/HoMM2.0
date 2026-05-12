using Adventure.Domain.Progression;
using Zenject;

namespace Adventure.Infrastructure.Progression
{
    public sealed class StoryFlagsRuntimeStateSync : IInitializable, ITickable
    {
        private readonly IStoryFlagsService _storyFlagsService;
        private readonly StoryFlagsRuntimeStateSO _runtimeState;

        public StoryFlagsRuntimeStateSync(
            IStoryFlagsService storyFlagsService,
            StoryFlagsRuntimeStateSO runtimeState)
        {
            _storyFlagsService = storyFlagsService;
            _runtimeState = runtimeState;
        }

        public void Initialize()
        {
            _runtimeState.SetSnapshot(_storyFlagsService.Flags);
        }

        public void Tick()
        {
            if (_runtimeState.TryConsumeInspectorChanges(out var flags))
            {
                _storyFlagsService.ReplaceAll(flags);
            }
        }
    }
}
