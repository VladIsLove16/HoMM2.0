using System.Collections.Generic;
using Adventure.Domain.Inventory;
using UnityEngine;
using UnityEngine.UI;

public class MushroomBookController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UnitDefinitionCollection collection;
    [SerializeField] private int entriesPerPage = 4;

    [Header("UI")]
    [SerializeField] private List<MushroomEntryView> entrySlots = new List<MushroomEntryView>();
    [SerializeField] private Text pageNumberText;

    private int currentPageIndex;
    private PresentationMode currentMode = PresentationMode.Normal;
    private readonly List<MushroomBookEntryViewData> runtimeEntries = new List<MushroomBookEntryViewData>();
    private bool useRuntimeEntries;

    private void Start()
    {
        RenderPage();
    }

    public void SetRuntimeEntries(IReadOnlyList<MushroomBookEntryViewData> entries)
    {
        useRuntimeEntries = true;
        runtimeEntries.Clear();
        if (entries != null)
        {
            runtimeEntries.AddRange(entries);
        }
        var total = GetTotalPages();
        if (total > 0)
            currentPageIndex = Mathf.Clamp(currentPageIndex, 0, total - 1);
        else
            currentPageIndex = 0;
        RenderPage();
    }

    public void SetModeHumanized(bool humanized)
    {
        currentMode = humanized ? PresentationMode.Humanized : PresentationMode.Normal;
        foreach (var slot in entrySlots)
        {
            if (slot != null) slot.SetPresentationMode(currentMode);
        }
    }

    public void NextPage()
    {
        var total = GetTotalPages();
        if (total <= 0) return;
        currentPageIndex = Mathf.Clamp(currentPageIndex + 1, 0, total - 1);
        RenderPage();
    }

    public void PrevPage()
    {
        var total = GetTotalPages();
        if (total <= 0) return;
        currentPageIndex = Mathf.Clamp(currentPageIndex - 1, 0, total - 1);
        RenderPage();
    }

    public void GoToPage(int pageIndex)
    {
        var total = GetTotalPages();
        if (total <= 0) return;
        currentPageIndex = Mathf.Clamp(pageIndex, 0, total - 1);
        RenderPage();
    }

    private void RenderPage()
    {
        var entries = GetPage(currentPageIndex);
        if (entries == null) return;

        var i = 0;
        foreach (var entry in entries)
        {
            if (i < entrySlots.Count && entrySlots[i] != null)
            {
                entrySlots[i].Bind(entry);
                entrySlots[i].SetPresentationMode(currentMode);
            }
            i++;
        }

        // Clear remaining slots
        for (; i < entrySlots.Count; i++)
        {
            if (entrySlots[i] == null) continue;
            entrySlots[i].Bind(null);
        }

        UpdatePageNumber();
    }

    private void UpdatePageNumber()
    {
        if (pageNumberText == null) return;
        var total = GetTotalPages();
        pageNumberText.text = total > 0 ? (currentPageIndex + 1).ToString() : "0";
    }

    private int GetTotalPages()
    {
        if (useRuntimeEntries)
        {
            if (entriesPerPage <= 0) return 0;
            return Mathf.CeilToInt(runtimeEntries.Count / (float)entriesPerPage);
        }

        return collection != null ? collection.GetTotalPages(entriesPerPage) : 0;
    }

    private IEnumerable<MushroomBookEntryViewData?> GetPage(int pageIndex)
    {
        if (useRuntimeEntries)
        {
            if (entriesPerPage <= 0) yield break;
            var start = pageIndex * entriesPerPage;
            for (var i = 0; i < entriesPerPage; i++)
            {
                var idx = start + i;
                if (idx >= 0 && idx < runtimeEntries.Count)
                    yield return runtimeEntries[idx];
                else
                    yield return null;
            }
            yield break;
        }

        if (collection == null) yield break;
        foreach (var def in collection.GetPage(pageIndex, entriesPerPage))
        {
            yield return def != null ? (MushroomBookEntryViewData?)ConvertDefinition(def) : null;
        }
    }

    private static MushroomBookEntryViewData ConvertDefinition(UnitDefinitionSO def)
    {
        var characteristics = new List<string>();
        if (def != null && def.Characteristics != null)
        {
            foreach (var c in def.Characteristics)
            {
                characteristics.Add(string.IsNullOrEmpty(c.Key) ? c.Value : $"{c.Key}: {c.Value}");
            }
        }

        return new MushroomBookEntryViewData(
            def != null ? def.Name : string.Empty,
            def != null ? def.Description : string.Empty,
            def != null ? def.UnitIcon : null,
            def != null ? def.UnitIconHovered : null,
            def != null ? def.HumanizedIcon : null,
            def != null ? def.HumanizedIconHovered : null,
            characteristics);
    }
}


