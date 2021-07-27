using MoonSharp.Interpreter;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DU_Screen_Simulator
{
    [MoonSharpUserData]
    public class ScreenUnit
    {
        private Form _parent;
        public Graphics Graphics = null;

        public Color BackgroundColor = Color.Black;

        public int FrameCountBeforeRedraw = 1;

        // So layers make things complicated
        // We can't just draw when they tell us to draw, we need to store the instruction in a collection of Layers
        // Then after we've run the script, we iterate that collection in order and *then* draw

        private List<Layer> Layers = new List<Layer>();
        private List<Font> Fonts = new List<Font>();


        public ScreenUnit(Form parent)
        {
            _parent = parent;
        }

        public void Reset()
        {
            foreach (var l in Layers)
                l.Actions = new List<Action>();
        }

        public void RenderScreen()
        {
            foreach (var actionList in Layers)
            {
                foreach (var action in actionList.Actions)
                    action.Invoke();
            }
        }



        public class DrawingParameters
        {
            public Layer layer { get; set; }
            public ShapeType shape { get; set; }
            public float x { get; set; }
            public float y { get; set; }
            public float x2 { get; set; }
            public float y2 { get; set; }
            public float x3 { get; set; }
            public float y3 { get; set; }
            public float? x4 { get; set; }
            public float? y4 { get; set; }
            public float width { get; set; }
            public float height { get; set; }
            public float radius { get; set; }
            public Brush shadowBrush { get; set; }
            public Brush fillBrush { get; set; }
            public Pen pen { get; set; }
            public float strokeWidth { get; set; }
            public float rotation { get; set; }
            public ShadowInformation shadow { get; set; }
            public Graphics Graphics { get; set; }
            public Font Font { get; set; }
            public string message { get; set; }
        }





        // TODO: !
        // We have problems with NextFillColor and the like
        // Because it only processes them when it draws, not when you add the shape to the Actions
        // We need to process when we add them to actions.  

        // There is also some problem with our x/y values I think, our box is not bouncing correctly on the right side





        private Dictionary<ShapeType, Action<DrawingParameters>> shadowDrawDictionary = new Dictionary<ShapeType, Action<DrawingParameters>>()
        { {ShapeType.Shape_Box, (DrawingParameters dp) => { dp.Graphics.FillRectangle(dp.shadowBrush, dp.x - dp.shadow.Radius, dp.y - dp.shadow.Radius, dp.width, dp.height); } },
          {ShapeType.Shape_BoxRounded, (DrawingParameters dp) => { dp.Graphics.FillRoundedRectangle(dp.shadowBrush, dp.x - dp.shadow.Radius, dp.y - dp.shadow.Radius, dp.width, dp.height, dp.radius); } },
          {ShapeType.Shape_Circle, (DrawingParameters dp) => { dp.Graphics.FillEllipse(dp.shadowBrush, dp.x - dp.radius - dp.shadow.Radius, dp.y - dp.radius - dp.shadow.Radius, dp.radius*2, dp.radius*2); } },
          {ShapeType.Shape_Line, (DrawingParameters dp) => { dp.Graphics.DrawLine(dp.pen, dp.x - dp.shadow.Radius, dp.y - dp.shadow.Radius, dp.x2 - dp.shadow.Radius, dp.y2 - dp.shadow.Radius); } },
          {ShapeType.Shape_Polygon, (DrawingParameters dp) => { 
              // This is a litle more complicated, if x4 or y4 are null we only use 3 points, otherwise 4
              if (!dp.x4.HasValue || !dp.y4.HasValue)
                dp.Graphics.FillPolygon(dp.shadowBrush, new PointF[] { new PointF(dp.x - dp.shadow.Radius, dp.y - dp.shadow.Radius), new PointF(dp.x2 - dp.shadow.Radius, dp.y2 - dp.shadow.Radius), 
                    new PointF(dp.x3 - dp.shadow.Radius, dp.y3 - dp.shadow.Radius) });
              else
                  dp.Graphics.FillPolygon(dp.shadowBrush, new PointF[] { new PointF(dp.x - dp.shadow.Radius, dp.y - dp.shadow.Radius), new PointF(dp.x2 - dp.shadow.Radius, dp.y2 - dp.shadow.Radius), 
                      new PointF(dp.x3 - dp.shadow.Radius, dp.y3 - dp.shadow.Radius), new PointF(dp.x4.Value - dp.shadow.Radius, dp.y4.Value - dp.shadow.Radius) });
          } },
          {ShapeType.Shape_Text, (DrawingParameters dp) => { dp.Graphics.DrawString(dp.message, dp.Font, dp.shadowBrush, dp.x - dp.shadow.Radius, dp.y - dp.shadow.Radius); } }
        };


        private Dictionary<ShapeType, Action<DrawingParameters>> fillDrawDictionary = new Dictionary<ShapeType, Action<DrawingParameters>>()
        { {ShapeType.Shape_Box, (DrawingParameters dp) => { dp.Graphics.FillRectangle(dp.fillBrush, dp.x, dp.y, dp.width, dp.height); } },
          {ShapeType.Shape_BoxRounded, (DrawingParameters dp) => { dp.Graphics.FillRoundedRectangle(dp.fillBrush, dp.x, dp.y, dp.width, dp.height, dp.radius); } },
          {ShapeType.Shape_Circle, (DrawingParameters dp) => { dp.Graphics.FillEllipse(dp.fillBrush, dp.x - dp.radius, dp.y - dp.radius, dp.radius*2, dp.radius*2); } },
          {ShapeType.Shape_Line, (DrawingParameters dp) => {} },
          {ShapeType.Shape_Polygon, (DrawingParameters dp) => { 
              // This is a litle more complicated, if x4 or y4 are null we only use 3 points, otherwise 4
              if (!dp.x4.HasValue || !dp.y4.HasValue)
                dp.Graphics.FillPolygon(dp.fillBrush, new PointF[] { new PointF(dp.x, dp.y), new PointF(dp.x2, dp.y2), new PointF(dp.x3, dp.y3) });
              else
                  dp.Graphics.FillPolygon(dp.fillBrush, new PointF[] { new PointF(dp.x, dp.y), new PointF(dp.x2, dp.y2), new PointF(dp.x3, dp.y3), new PointF(dp.x4.Value, dp.y4.Value) });
          } },
          {ShapeType.Shape_Text, (DrawingParameters dp) => { dp.Graphics.DrawString(dp.message, dp.Font, dp.fillBrush, dp.x, dp.y); } }
        };


        private Dictionary<ShapeType, Action<DrawingParameters>> strokeDrawDictionary = new Dictionary<ShapeType, Action<DrawingParameters>>()
        { {ShapeType.Shape_Box, (DrawingParameters dp) => { dp.Graphics.DrawRectangle(dp.pen, dp.x, dp.y, dp.width, dp.height); } },
          {ShapeType.Shape_BoxRounded, (DrawingParameters dp) => { dp.Graphics.DrawRoundedRectangle(dp.pen, dp.x, dp.y, dp.width, dp.height, dp.radius); } },
          {ShapeType.Shape_Circle, (DrawingParameters dp) => { dp.Graphics.DrawEllipse(dp.pen, dp.x - dp.radius, dp.y - dp.radius, dp.radius*2, dp.radius*2); } },
          {ShapeType.Shape_Line, (DrawingParameters dp) => { dp.Graphics.DrawLine(dp.pen, dp.x, dp.y, dp.x2, dp.y2); } },
          {ShapeType.Shape_Polygon, (DrawingParameters dp) => { 
              // This is a litle more complicated, if x4 or y4 are null we only use 3 points, otherwise 4
              if (!dp.x4.HasValue || !dp.y4.HasValue)
                dp.Graphics.DrawPolygon(dp.pen, new PointF[] { new PointF(dp.x, dp.y), new PointF(dp.x2, dp.y2), new PointF(dp.x3, dp.y3) }); 
              else
                  dp.Graphics.DrawPolygon(dp.pen, new PointF[] { new PointF(dp.x, dp.y), new PointF(dp.x2, dp.y2), new PointF(dp.x3, dp.y3), new PointF(dp.x4.Value, dp.y4.Value) });
          } },
          {ShapeType.Shape_Text, (DrawingParameters dp) => {  } } // Do nothing, we can't do text outlines
        };


        private void DrawShape(int layerId, ShapeType shape, float x, float y, float width = 0, float height = 0, float radius = 0, float x2 = 0, float y2 = 0, float x3 = 0, float y3 = 0, float x4 = 0, float y4 = 0, int font = 0, string message = null)
        {
            var parameters = new DrawingParameters();
            var layer = Layers[layerId];
            parameters.layer = layer;
            var strokeColor = layer.GetStrokeColor(shape);
            var fillColor = layer.GetFillColor(shape);
            parameters.rotation = layer.GetRotation(shape);
            float strokeWidth = layer.GetStrokeWidth(shape);
            var shadow = layer.GetShadow(shape);
            parameters.shadow = shadow;
            parameters.pen = new Pen(strokeColor, strokeWidth);
            parameters.shadowBrush = new SolidBrush(shadow.Color);
            parameters.fillBrush = new SolidBrush(fillColor);
            parameters.Graphics = Graphics;
            parameters.x2 = x2;
            parameters.y2 = y2;
            parameters.x3 = x3;
            parameters.y3 = y3;
            parameters.x4 = x4;
            parameters.y4 = y4;
            parameters.x = x;
            parameters.y = y;
            parameters.width = width;
            parameters.height = height;
            parameters.radius = radius;
            if (Fonts.Count > font)
                parameters.Font = Fonts[font];
            parameters.message = message;

            // Draw
            if (shadow.Radius > 0)
                shadowDrawDictionary[shape](parameters);
            fillDrawDictionary[shape](parameters);
            strokeDrawDictionary[shape](parameters);


            parameters.pen.Dispose(); // TODO: reuse?
            parameters.shadowBrush.Dispose();
            parameters.fillBrush.Dispose();
        }


        public void addBox(int layer, float x, float y, float width, float height)
        {
            Layers[layer].Actions.Add(() => DrawShape(layer, ShapeType.Shape_Box, x, y, width, height));
        }


        public void addBoxRounded(int layer, float x, float y, float width, float height, float radius)
        {
            Layers[layer].Actions.Add(() => DrawShape(layer, ShapeType.Shape_BoxRounded, x, y, width, height, radius));
        }


        public void addCircle(int layer, float x, float y, float radius)
        {
            Layers[layer].Actions.Add(() => DrawShape(layer, ShapeType.Shape_Circle, x, y, radius: radius));
        }

        public void addImage(int layer, int imageId, float x, float y, float width, float height)
        {
            Layers[layer].Actions.Add(() => DrawShape(layer, ShapeType.Shape_Image, x, y, width, height));
        }

        public void addLine(int layer, float x1, float y1, float x2, float y2)
        {
            Layers[layer].Actions.Add(() => DrawShape(layer, ShapeType.Shape_Line, x1, y1, x2: x2, y2: y2));
        }

        public void addQuad(int layer, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            Layers[layer].Actions.Add(() => DrawShape(layer, ShapeType.Shape_Polygon, x1, y1, x2: x2, y2: y2, x3: x3, y3: y3, x4: x4, y4: y4));
        }

        public void addText(int layer, int fontId, string text, float x, float y)
        {
            Layers[layer].Actions.Add(() => DrawShape(layer, ShapeType.Shape_Text, x, y, message: text, font: fontId));
        }

        public void addTriangle(int layer, float x1, float y1, float x2, float y2, float x3, float y3)
        {
            Layers[layer].Actions.Add(() => DrawShape(layer, ShapeType.Shape_Polygon, x1, y1, x2: x2, y2: y2, x3: x3, y3: y3));
        }

        public int createLayer()
        {
            Layers.Add(new Layer());
            return Layers.Count - 1;
        }

        public void setBackgroundColor(float r, float g, float b)
        {
            // rgb are 0-1
            BackgroundColor = Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public (int, int) getCursor()
        {
            return (0, 0);
        }

        public bool getCursorDown()
        {
            return false; // Return true if the mouse cursor is currently pressed down on the screen, false otherwise. 

            // Retains its state if dragged out of the screen.
        }

        public bool getCursorPressed()
        {
            return false; // Return true if the mouse button changed from being released to being pressed at any point since the last update.
        }

        public bool getCursorReleased()
        {
            return false;
            // Return true if the mouse button changed from being pressed to being released at any point since the last update.

            // Note that it is possible for both getCursorPressed() and getCursorReleased() to return true in the same script execution, if the mouse button was both pressed and released since the last execution.
        }

        public float getDeltaTime()
        {
            return 0;
            // Return the time, in seconds, since the screen was last updated. Useful for timing-based animations, since screens are not guaranteed to be updated at any specific time interval, it is more reliable to update animations based on this timer than based on a frame counter.
        }

        public int getRenderCost()
        {
            return 0; // Render cost is some constant plus the area of the drawn shape.  Mine won't match DU but they can use it to avoid overcomputation in the same ways
        }

        public int getRenderCostMax()
        {
            return 50000; // IDK
        }

        public DynValue getResolution()
        {
            return DynValue.NewTuple( DynValue.NewNumber(1024), DynValue.NewNumber(612) );
        }

        public void logMessage(string message)
        {
            // Log message to 'lua chat', or some other window.
        }

        public int? loadImage(string path)
        {
            // Return an image handle that can be used with addImage. If the image is not yet loaded, a sentinel value will be returned that will cause addImage to fail silently, so that the rendered image will not appear until it is loaded. Only images that have gone through image validation are available.
            return null; // I think this means null would be the sentinel value?  Or like, maxint or something
        }

        public int? loadFont(string name, float size)
        {
            //Return a font handle that can be used with addText. The font size is size vertical pixels. If the font is not yet loaded, a sentinel value will be returned that will cause addText to fail silently, so that the rendered text will not appear until the font is loaded.  A maximum of 8 fonts can be loaded for each render.
            Fonts.Add(new Font(name, size));
            return Fonts.Count-1;
        }

        public bool isImageLoaded(int imageHandle)
        {
            return true;
        }

        public DynValue getTextBounds(int fontId, string text)
        {
            return DynValue.NewTuple(DynValue.NewNumber(0), DynValue.NewNumber(0));
        }

        public DynValue getFontMetrics(int fontId)
        {
            return DynValue.NewTuple(DynValue.NewNumber(0), DynValue.NewNumber(0));
            // Compute and return the ascender and descender height of given font.
            // I have no idea what that means.  
        }

        public void requestAnimationFrame(int frames)
        {
            // Notify the screen manager that this screen should be redrawn in 'frames' frames.
            FrameCountBeforeRedraw = frames;
        }

        public void setDefaultFillColor(int layer, string shapeType, float r, float g, float b, float a)
        {
            Layers[layer].DefaultFillColor[(ShapeType)Enum.Parse(typeof(ShapeType), shapeType)] = Color.FromArgb((int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public void setDefaultStrokeColor(int layer, string shapeType, float r, float g, float b, float a)
        {
            Layers[layer].DefaultStrokeColor[(ShapeType)Enum.Parse(typeof(ShapeType), shapeType)] = Color.FromArgb((int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public void setDefaultRotation(int layer, string shapeType, float radians)
        {
            Layers[layer].DefaultRotation[(ShapeType)Enum.Parse(typeof(ShapeType), shapeType)] = radians;
        }

        public void setDefaultStrokeWidth(int layer, string shapeType, float width)
        {
            Layers[layer].DefaultStrokeWidth[(ShapeType)Enum.Parse(typeof(ShapeType), shapeType)] = width;
        }

        public void setDefaultShadow(int layer, string shapeType, float radius, float r, float g, float b, float a)
        {
            Layers[layer].DefaultShadow[(ShapeType)Enum.Parse(typeof(ShapeType), shapeType)] = new ShadowInformation() { Color = Color.FromArgb((int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255)), Radius = radius };
        }

        public void setNextFillColor(int layer, float r, float g, float b, float a)
        {
            Layers[layer].NextFillColor = Color.FromArgb((int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public void setNextStrokeColor(int layer, float r, float g, float b, float a)
        {
            Layers[layer].NextStrokeColor = Color.FromArgb((int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public void setNextRotationDegrees(int layer, float radians)
        {
            Layers[layer].NextRotation = radians;
        }

        public void setNextStrokeWidth(int layer, float width)
        {
            Layers[layer].NextStrokeWidth = width;
        }

        public void setNextShadow(int layer, float radius, float r, float g, float b, float a)
        {
            Layers[layer].NextShadow = new ShadowInformation() { Color = Color.FromArgb((int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255)), Radius = radius };
        }

        public void setNextTextAlign(int layer, string alignH, string alignV)
        {
            Layers[layer].NextTextAlign = new TextAlign() { AlignH = (AlignH)Enum.Parse(typeof(AlignH), alignH), AlignV = (AlignV)Enum.Parse(typeof(AlignV), alignV) };
        }
    }
}
