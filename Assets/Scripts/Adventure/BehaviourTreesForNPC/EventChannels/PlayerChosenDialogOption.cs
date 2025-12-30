using Adventure.Domain.Dialog;
using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Target chosen dialog option")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Target chosen dialog option", message: "Target chosen dialog option [option]", category: "Events", id: "5c00fef408f15b78a2370152b9ce2909")]
public sealed partial class PlayerChosenDialogOption : EventChannel<DialogueChoiceAction> { }

