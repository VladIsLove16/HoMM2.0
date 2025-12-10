using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sets the rotation speed of the global LightRotator via the developer console.
/// </summary>
public sealed class LightRotationSpeedConsoleCommand : IDeveloperConsoleCommand
{
    private static readonly string[] s_aliases =
    {
        "light.speed"
    };

    public string Key => "light.set_speed";
    public string Description => "Задаёт скорость вращения света (LightRotator).";
    public string Usage => "light.set_speed <float>";
    public IReadOnlyList<string> Aliases => s_aliases;

    public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
    {
        if (args == null || args.Count < 1)
        {
            output.AppendError($"Использование: {Usage}");
            return;
        }

        if (!float.TryParse(args[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var speed))
        {
            output.AppendError($"Не удалось разобрать значение скорости '{args[0]}' как число.");
            return;
        }

        var rotator = Object.FindObjectOfType<LightRotator>();
        if (rotator == null)
        {
            output.AppendError("Объект LightRotator не найден на сцене.");
            return;
        }

        rotator.SetSpeed(speed);
        output.AppendLine($"Скорость вращения света установлена в {speed}.");
    }
}

