using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/PlayerReachesTalkingDistanceEventChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "PlayerReachesTalkingDistanceEventChannel", message: "aPlayerReachesTalkingDistanceEventChannel", category: "Events", id: "2e0a239522ce87807c5bebac3bfc9438")]
public sealed partial class PlayerReachesTalkingDistanceEventChannel : EventChannel { }

