using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
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
        private bool _monitorHooked;
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
            _monitorHooked = false;


        }
        
        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            _manager.Hook();
            if (!_manager.IsHooked)
            {
                _monitorHooked = false;
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
            
        }


        private void CheckStart(string text)
        {
            if (_model.CurrentState.CurrentSplitIndex != -1)
                return;
            //Rider wanted me to invert the if from == to this so I guess it's more efficient (probably stops at the first char)
            if (!text.StartsWith("exitField(\"title00\") - start: nextMap(\"f1000\")")) return;
            //_model.CurrentState.IsGameTimePaused = true;
            _model.Start();
        }

        private void CheckLoading(string line)
        {
            if (line.StartsWith("LoadInfo"))
            {
   
                var match = Regex.Match(line, @"([0-9]*\.?[0-9]+)");
                if (match.Success)
                {
                    double timeToRemove = Convert.ToDouble(match.Groups[1].Value);
                    if(_model.CurrentState.CurrentTime.GameTime - TimeSpan.FromSeconds(timeToRemove) > TimeSpan.Zero)
                        _model.CurrentState.SetGameTime(_model.CurrentState.CurrentTime.GameTime - TimeSpan.FromSeconds(timeToRemove));
                    else _model.CurrentState.SetGameTime(TimeSpan.Zero);
                }
            

            }
            /*if (line.StartsWith("NOW LOADING Draw Start")  && !_model.CurrentState.IsGameTimePaused)
            {
                Debug.WriteLine("Stopped timer");
                _model.CurrentState.IsGameTimePaused = true;
            }
                    
            else if (line.StartsWith("NOW LOADING Draw End") && _model.CurrentState.IsGameTimePaused)
            {
                Debug.WriteLine(("started timer"));
                _model.CurrentState.IsGameTimePaused = false;
            }
            */
            
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