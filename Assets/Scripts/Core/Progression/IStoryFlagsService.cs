using System.Collections.Generic;

namespace Adventure.Domain.Progression
{
    public interface IStoryFlagsService
    {
        IReadOnlyCollection<string> Flags { get; }

        bool Has(string flagId);
        bool HasAll(IEnumerable<string> flagIds);
        void Set(string flagId);
        void SetMany(IEnumerable<string> flagIds);
        void Clear(string flagId);
        void ReplaceAll(IEnumerable<string> flagIds);
        void ResetAll();
    }
}
