using System.Collections.Generic;
using System.Text;
using UnityEngine;

public struct ActionPreview
{
    public List<Vector2Int> MoveRoute;
    public List<Vector2Int> InaccessibleRoute;
    public List<Vector2Int> ReachableCells;
    public bool IsActionAvailable;
    public bool IsMyTurn;
    public Vector2Int? HoveredCell;
    public ActionType ActionType;
    // Optional combat info
    public DamageContext Damage;
    public static ActionPreview notAvailable =>
         new ActionPreview
            {
                IsActionAvailable = false,
                Damage = null
            };

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("ActionPreview {");
        sb.Append(" ActionType=").Append(ActionType);
        sb.Append(", IsMyTurn=").Append(IsMyTurn);
        sb.Append(", IsActionAvailable=").Append(IsActionAvailable);

        sb.Append(", HoveredCell=");
        sb.Append(HoveredCell.HasValue ? FormatCell(HoveredCell.Value) : "null");

        sb.Append(", MoveRoute=").Append(FormatRoute(MoveRoute));
        sb.Append(", InaccessibleRoute=").Append(FormatRoute(InaccessibleRoute));
        sb.Append(", ReachableCells=").Append(FormatRoute(ReachableCells));

        if (Damage != null)
        {
            sb.Append(", Damage={ amount=").Append(Damage.DamageAmount)
              .Append(", type=").Append(Damage.Type)
              .Append(", die=").Append(Damage.DieAmount)
              .Append(", effects=").Append(Damage.AppliedEffects?.Count ?? 0)
              .Append(" }");
        }

        sb.Append(" }");
        return sb.ToString();
    }

    private static string FormatRoute(List<Vector2Int> route)
    {
        if (route == null) return "null";
        if (route.Count == 0) return "[]";
        var sb = new StringBuilder();
        sb.Append('[');
        for (int i = 0; i < route.Count; i++)
        {
            if (i > 0) sb.Append(" -> ");
            sb.Append(FormatCell(route[i]));
        }
        sb.Append(']');
        sb.Append("(count=").Append(route.Count).Append(')');
        return sb.ToString();
    }

    private static string FormatCell(Vector2Int cell)
    {
        return "(" + cell.x + "," + cell.y + ")";
    }
}


