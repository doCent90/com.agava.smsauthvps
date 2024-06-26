using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class ModifiedShadow : Shadow
    {
        public override void ModifyMesh(VertexHelper vh)

        {
            if (!this.IsActive())
                return;

            var list = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(list);

            ModifyVertices(list);

#if UNITY_5_2_1pX || UNITY_5_3_OR_NEWER
            vh.Clear();
#endif
            vh.AddUIVertexTriangleStream(list);
            ListPool<UIVertex>.Release(list);
        }

        public virtual void ModifyVertices(List<UIVertex> verts)
        {
        }
    }
}
