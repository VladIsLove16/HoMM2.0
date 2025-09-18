using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionHandler
{
    void Execute(ActionContext ctx);
    ActionPreview GetPreview(ActionContext ctx);
}
