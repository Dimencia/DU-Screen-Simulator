using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DU_Screen_Simulator
{
    public class Layer
    {
        public List<Action> Actions = new List<Action>();
        public Dictionary<ShapeType, Color> DefaultFillColor = new Dictionary<ShapeType, Color>();
        public Dictionary<ShapeType, float> DefaultRotation = new Dictionary<ShapeType, float>();
        public Dictionary<ShapeType, ShadowInformation> DefaultShadow = new Dictionary<ShapeType, ShadowInformation>();
        public Dictionary<ShapeType, Color> DefaultStrokeColor = new Dictionary<ShapeType, Color>();
        public Dictionary<ShapeType, float> DefaultStrokeWidth = new Dictionary<ShapeType, float>();

        public Color? NextFillColor = null;
        public float? NextRotation = null;
        public ShadowInformation NextShadow = null;
        public Color? NextStrokeColor = null;
        public float? NextStrokeWidth = null;
        public TextAlign NextTextAlign = new TextAlign(); // Has no default, so this is fine

        public Layer()
        {
            // Populate the defaults
            foreach(ShapeType t in Enum.GetValues(typeof(ShapeType)))
            {
                DefaultFillColor[t] = Color.Black;
                DefaultRotation[t] = 0;
                DefaultShadow[t] = new ShadowInformation();
                DefaultStrokeColor[t] = Color.Black;
                DefaultStrokeWidth[t] = 1;
            }
        }


        public Color GetStrokeColor(ShapeType shape)
        {
            if (NextStrokeColor.HasValue)
            {
                var color = NextStrokeColor.Value;
                NextStrokeColor = null;
                return color;
            }
            return DefaultStrokeColor[shape];
        }

        public Color GetFillColor(ShapeType shape)
        {
            if (NextFillColor.HasValue)
            {
                var color = NextFillColor.Value;
                NextFillColor = null;
                return color;
            }
            return DefaultFillColor[shape];
        }

        public float GetRotation(ShapeType shape)
        {
            if (NextRotation.HasValue)
            {
                var rotation = NextRotation.Value;
                NextRotation = null;
                return rotation;
            }
            return DefaultRotation[shape];
        }

        public float GetStrokeWidth(ShapeType shape)
        {
            if (NextStrokeWidth.HasValue)
            {
                var width = NextStrokeWidth.Value;
                NextStrokeWidth = null;
                return width;
            }
            return DefaultStrokeWidth[shape];
        }

        public ShadowInformation GetShadow(ShapeType shape)
        {
            if (NextShadow != null)
            {
                var shadow = NextShadow;
                NextShadow = null;
                return shadow;
            }
            return DefaultShadow[shape];
        }

        public TextAlign GetTextAlign()
        {
            return NextTextAlign;
        }
    }

    public class ShadowInformation
    {
        public float Radius { get; set; }
        public Color Color { get; set; } = Color.Black;
    }

    public class TextAlign
    {
        public AlignH AlignH { get; set; }
        public AlignV AlignV { get; set; }
    }

    public enum AlignH
    {
        AlignH_Left,
        AlignH_Center,
        AlignH_Right
    }

    public enum AlignV
    {
        AlignV_Ascender,
        AlignV_Baseline,
        AlignV_Top,
        AlignV_Middle,
        AlignV_Bottom,
        AlignV_Descender
    }

    public enum ShapeType
    {
        Shape_Box,
        Shape_BoxRounded,
        Shape_Circle,
        Shape_Image,
        Shape_Line,
        Shape_Polygon,
        Shape_Text
    }
}
