using System.Linq;

namespace Adventure.Domain.Dialog
{
    public sealed class DefaultArmyLineupFormatter : IArmyLineupFormatter
    {
        public string Format(ArmyLineupSO lineup)
        {
            if (lineup == null)
                return string.Empty;

            var stacks = lineup.Convert();
            if (stacks == null || stacks.Count == 0)
                return string.Empty;

            return string.Join(", ", stacks.Select(stack => $"{stack.UnitType} x{stack.Amount}"));
        }
    }
}
