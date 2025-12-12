using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/DialogueEventChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "DialogueEventChannel", message: "DialogueEventChannel", category: "Events", id: "2c528cbe94c3b6c16c70f42221ef5919")]
public sealed partial class DialogueEventChannel : EventChannel { }

