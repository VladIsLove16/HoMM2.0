using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Player talking with npc state channel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Player starts talking with npc", message: "Player starts talking with npc", category: "Events", id: "b4fbe6d080c650f728e3e84c1ea0e400")]
public sealed partial class PlayerTalkingWithNpcStateChannel : EventChannel { }

