using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MVPUnitTurnPanel : MonoBehaviour
{
    [SerializeField] Image unitIconpf;
    [SerializeField] GameObject parent;
    [SerializeField] Image ActiveCharacterImage;
    [SerializeField] List<UnitDefinitionSO> unitDatas;
    private Dictionary<Image, UnitModel> unitModels = new();
    [SerializeField] private List<Image> unitIconsQueue;
    [Button]
    public void NextUnit()
    {
        RemoveCurrentUnit();
        UpdateActiveCharacter();
    }

    public void CreatePanelInfo(UnitModel[] unitcontents)
    {
        foreach (UnitModel model in unitcontents)
        {
            UnitDefinitionSO unitDefinitionSO = unitDatas.First(x => x.UnitType == model.UnitType);
            Sprite sprite = unitDefinitionSO.UnitIcon;
            Image image = AddIcon(sprite);
            Link(image, model);
        }
    }

    private void UpdateActiveCharacter()
    {
        if(unitIconsQueue.Count >0)
            ActiveCharacterImage.sprite = unitIconsQueue[0].sprite;
    }

    private void RemoveCurrentUnit()
    {
        Image icon = unitIconsQueue[0];
        Debug.Log(icon != null);
        if (unitModels != null)
        {
            unitModels.TryGetValue(icon, out var model);
            if (model != null)
            {
                unitModels.Remove(icon);
            }
        }
        if (icon != null)
        {
            unitIconsQueue.Remove(icon);
#if UNITY_EDITOR
            DestroyImmediate(icon.gameObject);
#else
            Destroy(icon.gameObject);
#endif
        }
    }

    [Button]
    private void AddIcons()
    {
        foreach (var item in unitDatas)
        {
            AddIcon(item.UnitIcon);
        }
        UpdateActiveCharacter();
    }
   
    private void Link(Image sprite, UnitModel model)
    {
        unitModels[sprite] = model;
    }

    private Image AddIcon(Sprite sprite)
    {
        Image image = Instantiate(unitIconpf,parent.transform);
        image.sprite = sprite;
        unitIconsQueue.Add(image);
        return image;
    }
}