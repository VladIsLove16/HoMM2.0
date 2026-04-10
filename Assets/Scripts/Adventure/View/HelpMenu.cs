using Adventure.Application.VMs;
using SharedView;
using UnityEngine;
using Zenject;

public class HelpMenu : CanvasGroupPanelViewBase<HelpMenuViewModel>
{
    [Inject]
    public override void Construct(HelpMenuViewModel viewModel)
    {
        base.Construct(viewModel);
    }
}
