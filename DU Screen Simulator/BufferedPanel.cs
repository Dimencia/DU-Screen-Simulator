using MoonSharp.Interpreter;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DU_Screen_Simulator
{
    class BufferedPanel : System.Windows.Forms.Panel
    {
        private Form1 _parent;
        public BufferedPanel(Form1 parent)
        {
            _parent = parent;
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_parent._playerLua != null)
            {
                
                _parent._screenUnit.Graphics = e.Graphics;
                e.Graphics.Clear(_parent._screenUnit.BackgroundColor);
                _parent._screenUnit.Reset(); // TODO: Do layers persist between runs?
                try
                {
                    _parent._script.DoString(_parent._playerLua);
                }
                catch (ScriptRuntimeException ex)
                {
                    Console.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
                }
                _parent._screenUnit.RenderScreen();
            }
            //e.Graphics.Dispose(); // Since we didn't make it, we shouldn't dispose it
        }
    }
}
