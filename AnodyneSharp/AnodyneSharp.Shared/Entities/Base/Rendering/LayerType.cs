using AnodyneSharp.Drawing;
using AnodyneSharp.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Base.Rendering
{
    public interface ILayerType
    {
        float Z { get; }
    }

    public class Layer : ILayerType
    {
        DrawOrder layer;
        Entity parent;
        public Layer(DrawOrder layer, Entity parent)
        {
            this.layer = layer;
            this.parent = parent;
        }
        public float Z { get { return DrawingUtilities.GetDrawingZ(layer, MapUtilities.GetInGridPosition(parent.Position).Y + parent.height); } }
    }

    public class RefLayer : ILayerType
    {
        ILayerType parent;
        int LayerOffset;
         public RefLayer(ILayerType parent, int layerOffset)
        {
            this.parent = parent;
            LayerOffset = layerOffset;
        }

        public float Z { get { return parent.Z - LayerOffset * 0.0001f; } }
    }
}
