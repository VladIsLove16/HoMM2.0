using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/AnimationEndedGrabbing")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "AnimationEndedGrabbing", message: "NPC grabbed an item", category: "Events", id: "69aa52ccfc99fe5320656e9100538041")]
public sealed partial class AnimationEndedGrabbing : EventChannel { }

