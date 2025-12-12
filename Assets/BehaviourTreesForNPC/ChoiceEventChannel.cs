using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/ChoiceEventChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "ChoiceEventChannel", message: "ChoiceEventChannel", category: "Events", id: "267e59de3baed24530f78428fe9d54a8")]
public sealed partial class ChoiceEventChannel : EventChannel { }

