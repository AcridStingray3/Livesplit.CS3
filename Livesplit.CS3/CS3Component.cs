using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using WindowsInput;
using WindowsInput.Native;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;


namespace Livesplit.CS3
{
    public class CS3Component : IComponent
    {
        private readonly TimerModel _model;
        private readonly PointerAndConsoleManager _manager;
        private readonly InputSimulator _keyboard;
        private readonly Settings _settings = new Settings();
        private bool _monitorHooked;
        private bool _drawStartLoad;
        private bool _initFieldLoad;


        public string ComponentName { get; }

        public CS3Component(LiveSplitState state, string name)
        {
            ComponentName = name;
            _manager = new PointerAndConsoleManager();
            
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            _model = new TimerModel()
            {
                CurrentState = state
            };
            
            _model.InitializeGameTime();
            _monitorHooked = false;
            _drawStartLoad = false;
            _initFieldLoad = false;
            _keyboard = new InputSimulator();


        }
        
        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            _manager.Hook();
            if (!_manager.IsHooked)
            {
                _monitorHooked = false;
                _drawStartLoad = false;
                _model.CurrentState.IsGameTimePaused = false;
                return;
            }

            if (!_monitorHooked)
            {
                _manager.Monitor.Handlers += CheckStart;
                _manager.Monitor.Handlers += CheckLoading;
                Debug.WriteLine("Subscribed events");
                _monitorHooked = true;
            }
            _manager.UpdateValues();
            
            CheckBattleSplit();
            CheckAnimSkip();

        }


        private void CheckStart(string text)
        {
            if (_model.CurrentState.CurrentSplitIndex != -1)
                return;
            //Rider wanted me to invert the if from == to this so I guess it's more efficient (probably stops at the first char)
            if (!text.StartsWith("exitField(\"title00\") - start: nextMap(\"f1000\")")) return;
            _model.CurrentState.IsGameTimePaused = true;
            _model.Start();
        }

        private void CheckLoading(string line)
        {
 
            if (!_model.CurrentState.IsGameTimePaused)
            {
                if (line.StartsWith("NOW LOADING Draw Start"))
                {
                    _model.CurrentState.IsGameTimePaused = true;
                    _drawStartLoad = true;
                    Debug.Print("Draw start load start");
                }

                else if (line.StartsWith("FieldMap::initField start") )
                {
                    _model.CurrentState.IsGameTimePaused = true;
                    _initFieldLoad = true;
                    
                    Debug.Print("Init field load start");
                }
                
                else if (line.StartsWith("exitField"))
                {
                    _model.CurrentState.IsGameTimePaused = true;
                    
                    Debug.Print("exit field load start");
                }
            }

            else
            {
                if (!_initFieldLoad && !_drawStartLoad && line.StartsWith("exitField - end"))
                {
                    _model.CurrentState.IsGameTimePaused = false;
                    
                }
                
                else if (!_drawStartLoad && line.StartsWith("FieldMap::initField end"))
                {
                    _model.CurrentState.IsGameTimePaused = false;
                    _initFieldLoad = false;
                    
                }
                
                else if (line.StartsWith("NOW LOADING Draw End")){
                    _model.CurrentState.IsGameTimePaused = false;
                    _drawStartLoad = false;
                                   
                }
            }
            
        }
        
        //TODO
        private void CheckBattleSplit()
        {
            //These are just so rider shuts up about making them private
            ushort a = _manager.BattleId.CurrentValue;
            ushort b = _manager.BattleId.LastValue;
            
        }

        private void CheckAnimSkip()
        {
            
            if (_manager.Cheating.CurrentValue == 1 && _settings.SkipBattleAnimations)
            {
                _keyboard.Keyboard.KeyDown(VirtualKeyCode.SPACE);
                Thread.Sleep(17);
                _keyboard.Keyboard.KeyUp(VirtualKeyCode.SPACE);
                
            }
        }
        public Control GetSettingsControl(LayoutMode mode)
        {
            return _settings;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            XmlElement xmlSettings = document.CreateElement("Settings");

            XmlElement skipBattleAnims = document.CreateElement(nameof(Settings.SkipBattleAnimations));
            skipBattleAnims.InnerText = _settings.SkipBattleAnimations.ToString();
            xmlSettings.AppendChild(skipBattleAnims);

            return xmlSettings;
        }

        public void SetSettings(XmlNode settings)
        {
            XmlNode skipBattleAnimsNode = settings.SelectSingleNode(".//" + nameof(Settings.SkipBattleAnimations));
            if (bool.TryParse(skipBattleAnimsNode?.InnerText, out bool skipBattleAnims))
            {
                _settings.SkipBattleAnimations = skipBattleAnims;
            }
        }

        public void Dispose()
        {
            //remember to unhook if I ever hook anything
            if (_monitorHooked)
            {
                _manager.Monitor.Handlers -= CheckStart;
                _manager.Monitor.Handlers -= CheckLoading;
            }

            _manager.Dispose();
        }

        #region Unused interface stuff
        
        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
  
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            
        }

        public float HorizontalWidth => 0;
        public float MinimumHeight => 0;
        public float VerticalHeight => 0;
        public float MinimumWidth => 0;
        public float PaddingTop => 0;
        public float PaddingBottom => 0;
        public float PaddingLeft => 0;
        public float PaddingRight => 0;
        public IDictionary<string, Action> ContextMenuControls => null;
        #endregion
    }
}