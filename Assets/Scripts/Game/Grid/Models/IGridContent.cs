public interface IGridContent
{
    public int X { get; }
    public int Y { get; }
    public UnitType UnitType { get; }
    string GetDescription(); // Описание объекта для вывода в отладке или интерфейсе
    void SetCoords(int x, int y);
}