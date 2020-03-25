using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;


namespace Livesplit.CS3
{
    public class CS3Component : IComponent
    {
        private readonly TimerModel _model;
        private readonly PointerAndConsoleManager _manager;
        
        private readonly Settings _settings = new Settings();
        

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
            _model.CurrentState.IsGameTimePaused = true;
            
            DebugMonitor.OnOutputDebugString -=CheckLoading;
            DebugMonitor.OnOutputDebugString -=CheckStart;
            DebugMonitor.OnOutputDebugString +=CheckLoading;
            DebugMonitor.OnOutputDebugString +=CheckStart;

        }
        
        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            _manager.Hook();
            if (!_manager.IsHooked)
            {
                return;
            }
                
            _manager.UpdateValues();
            
            CheckBattleSplit();
            
        }

      

        private void CheckStart(int pid, string text)
        {
            if (_model.CurrentState.CurrentSplitIndex != -1)
                return;
            //Rider wanted me to invert the if from == to this so I guess it's more efficient (probably stops at the first char)
            if (text != "exitField(\"title00\") - start: nextMap(\"f1000\")") return;
            _model.CurrentState.IsGameTimePaused = true;
            _model.Start();
        }

        private void CheckLoading(int pid, string line)
        {
            
            if (line == "NOW LOADING Draw Start" && !_model.CurrentState.IsGameTimePaused)
            {
                System.Diagnostics.Debug.WriteLine("Stopped timer");
                _model.CurrentState.IsGameTimePaused = true;
            }
                    
            else if (line == "NOW LOADING Draw End" && _model.CurrentState.IsGameTimePaused)
            {
                System.Diagnostics.Debug.WriteLine(("started timer"));
                _model.CurrentState.IsGameTimePaused = false;
            }
            
        }
        
        //TODO
        private void CheckBattleSplit()
        {
            //These are just so rider shuts up about making them private
            ushort a = _manager.BattleId.CurrentValue;
            ushort b = _manager.BattleId.LastValue;
            a = _manager.BgmId.CurrentValue;
            b = _manager.BgmId.LastValue;
            return;
        }
        

        public Control GetSettingsControl(LayoutMode mode)
        {
            return _settings;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            XmlElement xmlSettings = document.CreateElement("Settings");

            XmlElement rndSkins = document.CreateElement(nameof(Settings.RandomizeSkins));
            rndSkins.InnerText = _settings.RandomizeSkins.ToString();
            xmlSettings.AppendChild(rndSkins);

            return xmlSettings;
        }

        public void SetSettings(XmlNode settings)
        {
            XmlNode rndSkinsNode = settings.SelectSingleNode(".//" + nameof(Settings.RandomizeSkins));
            if (bool.TryParse(rndSkinsNode?.InnerText, out bool rndSkins))
            {
                _settings.RandomizeSkins = rndSkins;
            }
        }

        public void Dispose()
        {
            //remember to unhook if I ever hook anything
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