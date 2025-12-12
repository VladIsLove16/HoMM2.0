using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/BattleResultChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "BattleResultChannel", message: "BattleResultChannel", category: "Events", id: "59285213bf328151b3272ae4c3ffa51c")]
public sealed partial class BattleResultChannel : EventChannel { }

