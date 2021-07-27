using MoonSharp.Interpreter;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DU_Screen_Simulator
{
    public partial class Form1 : Form
    {

        public Script _script;
        public string _playerLua = null;
        private Timer _timer;
        public ScreenUnit _screenUnit;

        private int _timerInterval = 17; // in ms, nearly 60fps

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            
            UserData.RegisterAssembly();
            UserData.RegisterType<ValueTuple<float,float>>();
            UserData.RegisterType<ValueTuple<int, int>>();
            InitLua();
            SetupTimers();
        }



        private void SetupTimers()
        {
            if (_timer != null)
                _timer.Dispose();
            _timer = new Timer();
            _timer.Interval = _timerInterval;
            _timer.Tick += (object sender, EventArgs e) =>
            {
                if (_screenUnit.FrameCountBeforeRedraw == 1 && _playerLua != null)
                {
                    _screenUnit.FrameCountBeforeRedraw--;
                    screenPanel.Refresh();
                }
            };
            _timer.Start();
        }


        private void InitLua()
        {
            _script = new Script();
            _screenUnit = new ScreenUnit(this);


            _script.Globals["_SCREEN_UNIT"] = _screenUnit;
            _script.Globals["addBox"] = (Action<int,float,float,float,float>)_screenUnit.addBox;
            _script.Globals["addBoxRounded"] = (Action<int, float, float, float, float, float>)_screenUnit.addBoxRounded;
            _script.Globals["addCircle"] = (Action<int, float, float, float>)_screenUnit.addCircle;
            _script.Globals["addImage"] = (Action<int, int, float, float, float, float>)_screenUnit.addImage;
            _script.Globals["addLine"] = (Action<int, float, float, float, float>)_screenUnit.addLine;
            _script.Globals["addQuad"] = (Action<int, float, float, float, float, float, float, float, float>)_screenUnit.addQuad;
            _script.Globals["addText"] = (Action<int, int, string, float, float>)_screenUnit.addText;
            _script.Globals["addTriangle"] = (Action<int, float, float, float, float, float, float>)_screenUnit.addTriangle;
            _script.Globals["createLayer"] = (Func<int>)_screenUnit.createLayer;
            _script.Globals["setBackgroundColor"] = (Action<float, float, float>)_screenUnit.setBackgroundColor;
            _script.Globals["getCursor"] = (Func<(int,int)>)_screenUnit.getCursor;
            _script.Globals["getCursorDown"] = (Func<bool>)_screenUnit.getCursorDown;
            _script.Globals["getCursorPressed"] = (Func<bool>)_screenUnit.getCursorPressed;
            _script.Globals["getCursorReleased"] = (Func<bool>)_screenUnit.getCursorReleased;
            _script.Globals["getDeltaTime"] = (Func<float>)_screenUnit.getDeltaTime;
            _script.Globals["getRenderCost"] = (Func<int>)_screenUnit.getRenderCost;
            _script.Globals["getRenderCostMax"] = (Func<int>)_screenUnit.getRenderCostMax;
            _script.Globals["getResolution"] = (Func<DynValue>)_screenUnit.getResolution;
            _script.Globals["logMessage"] = (Action<string>)_screenUnit.logMessage;
            _script.Globals["loadImage"] = (Func<string, int?>)_screenUnit.loadImage;
            _script.Globals["loadFont"] = (Func<string, float, int?>)_screenUnit.loadFont;
            _script.Globals["isImageLoaded"] = (Func<int, bool>)_screenUnit.isImageLoaded;
            _script.Globals["getTextBounds"] = (Func<int, string, DynValue>)_screenUnit.getTextBounds;
            _script.Globals["getFontMetrics"] = (Func<int, DynValue>)_screenUnit.getFontMetrics;
            _script.Globals["requestAnimationFrame"] = (Action<int>)_screenUnit.requestAnimationFrame;

            _script.Globals["setDefaultFillColor"] = (Action<int, string, float, float, float, float>)_screenUnit.setDefaultFillColor;
            _script.Globals["setDefaultStrokeColor"] = (Action<int, string, float, float, float, float>)_screenUnit.setDefaultStrokeColor;
            _script.Globals["setDefaultRotation"] = (Action<int, string, float>)_screenUnit.setDefaultRotation;
            _script.Globals["setDefaultStrokeWidth"] = (Action<int, string, float>)_screenUnit.setDefaultStrokeWidth;
            _script.Globals["setDefaultShadow"] = (Action<int, string, float, float, float, float, float>)_screenUnit.setDefaultShadow;

            _script.Globals["setNextFillColor"] = (Action<int, float, float, float, float>)_screenUnit.setNextFillColor;
            _script.Globals["setNextStrokeColor"] = (Action<int, float, float, float, float>)_screenUnit.setNextStrokeColor;
            _script.Globals["setNextRotationDegrees"] = (Action<int, float>)_screenUnit.setNextRotationDegrees;
            _script.Globals["setNextStrokeWidth"] = (Action<int, float>)_screenUnit.setNextStrokeWidth;
            _script.Globals["setNextShadow"] = (Action<int, float, float, float, float, float>)_screenUnit.setNextShadow;
            _script.Globals["setNextTextAlign"] = (Action<int, string, string>)_screenUnit.setNextTextAlign;


        }


        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                _playerLua = File.ReadAllText(openFileDialog1.FileName);
            }
        }

    }
}
