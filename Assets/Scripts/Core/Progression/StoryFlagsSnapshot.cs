using System;
using System.Collections.Generic;

namespace Adventure.Domain.Progression
{
    [Serializable]
    public sealed class StoryFlagsSnapshot
    {
        public List<string> Flags = new();
    }
}
