using System;
using System.Collections.Generic;
using UnityEngine;

public static class DeveloperConsoleParsing
{
    public static bool TryParseEnum<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        return Enum.TryParse(value, true, out result);
    }

    public static bool TryParseInt(string value, out int result, int minValue = int.MinValue)
    {
        if (int.TryParse(value, out result))
        {
            if (result < minValue)
            {
                return false;
            }
            return true;
        }
        return false;
    }

    public static bool TryParseVector2Int(IReadOnlyList<string> args, int startIndex, out Vector2Int result)
    {
        result = default;
        if (args == null)
        {
            return false;
        }
        if (startIndex < 0 || startIndex + 1 >= args.Count)
        {
            return false;
        }

        if (!int.TryParse(args[startIndex], out var x))
        {
            return false;
        }
        if (!int.TryParse(args[startIndex + 1], out var y))
        {
            return false;
        }

        result = new Vector2Int(x, y);
        return true;
    }
}
