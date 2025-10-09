using System.Collections.Generic;
using System.Linq;
using UniRx;

public class TurnQueue : ITurnQueue
{
    private readonly Dictionary<int, List<ICombatObject>> _turns = new();
    private readonly int _lookAhead;

    public TurnQueue(int lookAhead = 3)
    {
        _lookAhead = lookAhead;
    }

    public ICombatObject PeekNext()
    {
        var currentTurn = _turns.Keys.OrderBy(k => k).FirstOrDefault();
        return _turns.TryGetValue(currentTurn, out var list) && list.Count > 0
            ? list[0]
            : null;
    }

    public void Enqueue(int turn, ICombatObject unit)
    {
        if (!_turns.ContainsKey(turn))
            _turns[turn] = new List<ICombatObject>();

        _turns[turn].Add(unit);
    }

    public ICombatObject DequeueCurrent(ICombatObject current)
    {
        if (_turns.Count == 0) return null;

        var key = _turns.Keys.OrderBy(k => k).First();
        _turns[key].Remove(current);
        if (_turns[key].Count == 0)
            _turns.Remove(key);

        return PeekNext();
    }

    public bool EnsureValid(int currentTurn, IList<ICombatObject> activeUnits)
    {
        var keys = _turns.Keys.OrderBy(k => k).ToList();
        foreach (var k in keys)
        {
            _turns[k].RemoveAll(u => u == null || !activeUnits.Contains(u));
            if (_turns[k].Count == 0) _turns.Remove(k);
        }
        return _turns.Count > 0;
    }

    public void AdvanceTurn()
    {
        if (_turns.Count == 0) return;
        var firstKey = _turns.Keys.OrderBy(k => k).First();
        _turns.Remove(firstKey);
    }

    public void Clear() => _turns.Clear();
}

