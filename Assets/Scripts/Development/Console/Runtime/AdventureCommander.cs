using System;
using System.Collections.Generic;
using System.Linq;
using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Dialog;
using Adventure.Infrastructure.Inventory;
using Adventure.Infrastructure.Movement;
using Adventure.Integration.Battle;
using UnityEngine;
using Zenject;

public sealed class AdventureCommander
{
    private readonly DiContainer _container;
    private readonly PlayerMovementController _playerController;
    private readonly MushroomInventoryModel _inventoryModel;
    private readonly UnitDefinitionSOCollection _catalog;

    public AdventureCommander(DiContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _playerController = container.TryResolve<PlayerMovementController>();
        _inventoryModel = container.TryResolve<MushroomInventoryModel>();
        _catalog = container.TryResolve<UnitDefinitionSOCollection>();
    }

    public void CreateNpc(IDeveloperConsoleOutput output)
    {
        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        var template = FindNpcTemplate();
        if (template == null)
        {
            output.AppendError("No NPC prefab found to clone.");
            return;
        }

        var parent = template.transform.parent;
        var spawnPosition = GetSpawnPosition(template.transform.position);
        var rotation = template.transform.rotation;

        var clone = UnityEngine.Object.Instantiate(template.gameObject, spawnPosition, rotation, parent);
        clone.name = $"{template.gameObject.name}_Runtime_{Time.frameCount}";
        _container.InjectGameObject(clone);

        output.AppendLine($"NPC '{clone.name}' spawned at {spawnPosition}.");
    }

    public void AddArmyToClosestNpc(IDeveloperConsoleOutput output)
    {
        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        if (_inventoryModel == null)
        {
            output.AppendError("Player inventory is not available in this scene.");
            return;
        }

        var npcTriggers = UnityEngine.Object.FindObjectsByType<NpcDialogueTrigger>(FindObjectsInactive.Include);
        if (npcTriggers == null || npcTriggers.Length == 0)
        {
            output.AppendError("No NPCs found in the scene.");
            return;
        }

        var playerUnits = _inventoryModel.GetData();
        if (playerUnits == null || playerUnits.Count == 0)
        {
            output.AppendError("Player army is empty.");
            return;
        }

        var target = SelectClosestNpc(npcTriggers);
        if (target == null)
        {
            output.AppendError("Unable to locate the closest NPC.");
            return;
        }

        var lineup = CreateLineup(playerUnits);
        if (lineup == null)
        {
            output.AppendError("Failed to build an army lineup for the NPC.");
            return;
        }

        target.SetLineup(lineup);
        output.AppendLine($"NPC '{target.name}' received {playerUnits.Count} stacks.");
    }

    public void AddArmyToPlayer(UnitType unitType, int amount, IDeveloperConsoleOutput output)
    {
        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        if (_inventoryModel == null)
        {
            output.AppendError("Player inventory is not available in this scene.");
            return;
        }

        if (amount <= 0)
        {
            output.AppendError("Amount must be positive.");
            return;
        }

        if (_catalog != null && !_catalog.TryGet(unitType, out _))
        {
            output.AppendError($"Unit type '{unitType}' is not present in the catalog.");
            return;
        }

        _inventoryModel.Add(unitType, amount);
        output.AppendLine($"Player received {amount} units of '{unitType}'.");
    }

    public void SpawnFungus(UnitType unitType, int count, IDeveloperConsoleOutput output)
    {
        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        if (count <= 0)
        {
            output.AppendError("Amount must be positive.");
            return;
        }

        var template = FindMushroomTemplate();
        var createdPlaceholder = false;
        if (template == null)
        {
            template = CreatePlaceholderCollectible();
            createdPlaceholder = template != null;
        }

        if (template == null)
        {
            output.AppendError("Unable to find a mushroom prefab to clone.");
            return;
        }

        var parent = template.transform.parent;
        var origin = GetSpawnPosition(template.transform.position);
        var forward = _playerController != null ? _playerController.transform.forward : Vector3.forward;
        var right = _playerController != null ? _playerController.transform.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }
        if (right.sqrMagnitude < 0.001f)
        {
            right = Vector3.right;
        }
        forward.Normalize();
        right.Normalize();

        const float spacing = 1.2f;
        var spawned = new List<MushroomCollectible>();
        for (int i = 0; i < count; i++)
        {
            var row = i / 3;
            var column = i % 3;
            var offset = right * ((column - 1) * spacing) + forward * (row * spacing);
            var spawnPosition = origin + offset;
            spawnPosition.y = template.transform.position.y;

            var instance = UnityEngine.Object.Instantiate(template.gameObject, spawnPosition, template.transform.rotation, parent);
            instance.name = $"{template.gameObject.name}_Spawned_{Time.frameCount}_{i}";
            _container.InjectGameObject(instance);

            var collectible = instance.GetComponent<MushroomCollectible>();
            if (collectible == null)
            {
                UnityEngine.Object.Destroy(instance);
                continue;
            }

            collectible.Configure(unitType, true);
            spawned.Add(collectible);
        }

        if (createdPlaceholder)
        {
            UnityEngine.Object.Destroy(template.gameObject);
        }

        if (spawned.Count == 0)
        {
            output.AppendError("No mushrooms were spawned.");
            return;
        }

        output.AppendLine($"Spawned {spawned.Count} mushrooms of type '{unitType}'.");
    }

    private NpcDialogueTrigger FindNpcTemplate()
    {
        var triggers = UnityEngine.Object.FindObjectsOfType<NpcDialogueTrigger>();
        if (triggers == null || triggers.Length == 0)
        {
            return null;
        }

        if (_playerController == null)
        {
            return triggers[0];
        }

        var playerPos = _playerController.transform.position;
        return triggers
            .OrderBy(t => Vector3.SqrMagnitude(t.transform.position - playerPos))
            .FirstOrDefault();
    }

    private NpcDialogueTrigger SelectClosestNpc(IEnumerable<NpcDialogueTrigger> triggers)
    {
        if (triggers == null)
        {
            return null;
        }

        if (_playerController == null)
        {
            return triggers.FirstOrDefault();
        }

        var playerPos = _playerController.transform.position;
        return triggers
            .OrderBy(t => Vector3.SqrMagnitude(t.transform.position - playerPos))
            .FirstOrDefault();
    }

    private Vector3 GetSpawnPosition(Vector3 fallback)
    {
        if (_playerController == null)
        {
            return fallback + Vector3.right * 2f;
        }

        var origin = _playerController.transform.position;
        var forward = _playerController.transform.forward;
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        forward.Normalize();
        return origin + forward * 2f;
    }

    private ArmyLineupSO CreateLineup(IReadOnlyList<UnitStackData> stacks)
    {
        if (stacks == null || stacks.Count == 0)
        {
            return null;
        }

        return ArmyLineupSO.CreateRuntimeLineup(stacks);
    }

    private MushroomCollectible FindMushroomTemplate()
    {
        var collectibles = UnityEngine.Object.FindObjectsOfType<MushroomCollectible>();
        if (collectibles == null || collectibles.Length == 0)
        {
            return null;
        }

        return collectibles.FirstOrDefault(c => c.gameObject.activeInHierarchy) ?? collectibles[0];
    }

    private MushroomCollectible CreatePlaceholderCollectible()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "RuntimeMushroomTemplate";
        go.hideFlags = HideFlags.HideAndDontSave;
        go.SetActive(false);

        var collectible = go.AddComponent<MushroomCollectible>();
        _container.InjectGameObject(go);
        return collectible;
    }
}
