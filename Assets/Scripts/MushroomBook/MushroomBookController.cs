using System.Collections.Generic;
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

    private void Start()
    {
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
        var total = collection != null ? collection.GetTotalPages(entriesPerPage) : 0;
        if (total <= 0) return;
        currentPageIndex = Mathf.Clamp(currentPageIndex + 1, 0, total - 1);
        RenderPage();
    }

    public void PrevPage()
    {
        var total = collection != null ? collection.GetTotalPages(entriesPerPage) : 0;
        if (total <= 0) return;
        currentPageIndex = Mathf.Clamp(currentPageIndex - 1, 0, total - 1);
        RenderPage();
    }

    public void GoToPage(int pageIndex)
    {
        var total = collection != null ? collection.GetTotalPages(entriesPerPage) : 0;
        if (total <= 0) return;
        currentPageIndex = Mathf.Clamp(pageIndex, 0, total - 1);
        RenderPage();
    }

    private void RenderPage()
    {
        if (collection == null) return;

        var i = 0;
        foreach (var def in collection.GetPage(currentPageIndex, entriesPerPage))
        {
            if (i < entrySlots.Count && entrySlots[i] != null)
            {
                entrySlots[i].Bind(def);
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
        if (pageNumberText == null || collection == null) return;
        var total = collection.GetTotalPages(entriesPerPage);
        pageNumberText.text = total > 0 ? (currentPageIndex + 1).ToString() : "0";
    }
}


