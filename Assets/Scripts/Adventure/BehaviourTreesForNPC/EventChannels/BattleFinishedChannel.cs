using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/BattleFinishedChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "BattleFinishedChannel", message: "Battle finishes with the result [result]", category: "Events", id: "736533c08e5312fd69e0849f51109d18")]
public sealed partial class BattleFinishedChannel : EventChannel<BattleOutcome> { }

