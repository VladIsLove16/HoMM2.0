using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UnitStatePanel : MonoBehaviour
{
    [SerializeField] Canvas Canvas;
    [SerializeField] Image StatusEffectIconpf;
    [SerializeField] TextMeshProUGUI Health;
    [SerializeField] TextMeshProUGUI MaxHealth;
    [SerializeField] TextMeshProUGUI AttackDamage;
    [SerializeField] TextMeshProUGUI MoveSpeed;
    [SerializeField] Transform StatusEffectIconsParent;
    [SerializeField] Transform StatusEffectNamesParent;
    [SerializeField] Vector3 showOffset;

    private List<TextMeshProUGUI> statusEffectsTexts = new();
    private List<Image> statusEffectsImages = new();
    private Dictionary<string, StatusEffectData> statusEffectDictionary;

    [Inject] private List<StatusEffectData> statusEffectDatas;
    [Inject] IGridCellRenderer cellRenderer;
    [Inject] GameModel gameModel;
    private UnitModel currentmodel;
    private void Update()
    {
        Mouse3D.GetMouseWorldPosition(out Vector3 position);
        cellRenderer.ToGrid(position, out Vector2Int coords);
        IGridCell gameCell = gameModel.GetCell(coords);
        UnitModel unitModel = gameCell.Unit;
        if (unitModel != null)
        {
           if(unitModel != currentmodel)
           {
                Init(unitModel);
                Show(position);
           }
        }
        else
            Hide();
    }

    private void Start()
    {
        statusEffectDictionary = statusEffectDatas.ToDictionary(se => se.name, se => se);
    }

    public void Init(UnitModel model)
    {
        currentmodel = model;
        UnitState unitState = model.UnitState;
        Health.text = unitState.Health.ToString();
        MaxHealth.text = unitState.MaxHealth.ToString();
        AttackDamage.text = unitState.Damage.ToString();
        MoveSpeed.text = unitState.MoveSpeed.ToString();


        IReadOnlyList<StatusEffect> effects = model.ActiveEffects;
        ClearEffectView();
        if(effects.Count == 0)
        {
            TextMeshProUGUI textMeshProUGUI = Instantiate(Health, StatusEffectNamesParent);
            statusEffectsTexts.Add(textMeshProUGUI);
            textMeshProUGUI.text = "no Status Effects";
        }
        else
            foreach (var effect in effects)
            {
                CreateStatusEffectInfo(effect);
            }
    }

    public void Hide()
    {
        Canvas.enabled = false;
        currentmodel = null;
    }

    public void Show()
    {
        Canvas.enabled = true;
    }

    public void Show(Vector3 position)
    {
        Show();

        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 offsetPosition = showOffset + Camera.main.WorldToScreenPoint(position);
        transform.position = offsetPosition;
    }

    private void CreateStatusEffectInfo(StatusEffect effect)
    {
        Sprite sprite = statusEffectDictionary[effect.Name].Sprite;
        string name = effect.Name;

        //TextMeshProUGUI textMeshProUGUI = Instantiate(Health, StatusEffectNamesParent);
        //statusEffectsTexts.Add(textMeshProUGUI);
        //textMeshProUGUI.text = name;

        Image image = Instantiate(StatusEffectIconpf, StatusEffectIconsParent);
        statusEffectsImages.Add(image);
        image.sprite = sprite;
    }

    private void ClearEffectView()
    {
       foreach(var icon in statusEffectsImages)
        {

            Destroy(icon.gameObject);
        }
        statusEffectsImages.Clear();
        foreach (var text in statusEffectsTexts)
        {
            Destroy(text.gameObject);
        }
        statusEffectsTexts.Clear();
    }
}
