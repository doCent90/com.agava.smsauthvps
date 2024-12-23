using UnityEngine.Scripting;
using UnityEngine.UI;

[Preserve]
public class RaycastTarget : Graphic
{
    public override void SetMaterialDirty() { return; }
    public override void SetVerticesDirty() { return; }
}
