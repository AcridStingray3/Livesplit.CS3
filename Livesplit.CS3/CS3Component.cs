using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using WindowsInput;
using WindowsInput.Native;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
// ReSharper disable DelegateSubtraction

//TODO cache settings onto a map to not reflect over them every time a battle split triggers, since Yusuf will yell at me that's slow


namespace Livesplit.CS3
{
    public class CS3Component : IComponent
    {
        private readonly TimerModel _model;
        private readonly PointerAndConsoleManager _manager;
        private readonly InputSimulator _keyboard;
        private readonly Settings _settings = new Settings();
        
        private bool _delegatesHooked; 
        
        // These two are related so you could make them a struct if you reeeeeeeeeeeeally wanted to but like it's 2 bools dude
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
            _delegatesHooked = false;
            _drawStartLoad = false;
            _initFieldLoad = false;
            _keyboard = new InputSimulator();


        }
        
        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            _manager.Hook();
            if (!_manager.IsHooked)
            {
                _drawStartLoad = true;
                _initFieldLoad = false;
                _model.CurrentState.IsGameTimePaused = true;
                
                UnhookDelegates();
                
                return;
            }

            if (!_delegatesHooked) 
            {
                HookDelegates();
                Debug.WriteLine("Subscribed events");
            }
            
            _manager.UpdateValues();

        }


        private void CheckStart(string text)
        {
            if (_model.CurrentState.CurrentSplitIndex != -1)
                return;
            
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
                }

                else if (line.StartsWith("FieldMap::initField start") )
                {
                    _model.CurrentState.IsGameTimePaused = true;
                    _initFieldLoad = true;

                }
                
                else if (line.StartsWith("exitField"))
                {
                    _model.CurrentState.IsGameTimePaused = true;
                    
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
        
        private void CheckBattleSplit(BattleEnums endedBattle)
        {
            try {            
                if( (bool)typeof(Settings).GetField( endedBattle.ToString()).GetValue(_settings) )
                    _model.Split(); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        private void SkipBattleAnimation()
        {
            Thread.Sleep(70);
            _keyboard.Keyboard.KeyDown(VirtualKeyCode.SPACE);
            Thread.Sleep(1000/60);
            _keyboard.Keyboard.KeyUp(VirtualKeyCode.SPACE);
        }
        
        
        public Control GetSettingsControl(LayoutMode mode)
        {
            return _settings;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            // This runs in a fucking loop apparently so Reflection over it is awful but like typing all the settings out is awful too dude
            
            XmlElement xmlSettings = document.CreateElement("Settings");

            foreach (FieldInfo setting in typeof(Settings).GetFields().Where(field => field.FieldType == typeof(bool)))
            {
                XmlElement element = document.CreateElement(setting.Name);
                element.InnerText = ((bool)setting.GetValue(_settings)).ToString();
                xmlSettings.AppendChild(element);
            }
            
            /*////
            XmlElement skipBattleAnims = document.CreateElement(nameof(Settings.SkipBattleAnimations));
            skipBattleAnims.InnerText = _settings.SkipBattleAnimations.ToString();
            xmlSettings.AppendChild(skipBattleAnims);
            */

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
      
            UnhookDelegates();
            _manager.Dispose();
        }

        #region UtilityMethods

        private void HookDelegates()
        {
            if(_delegatesHooked)
                return;
            
            _manager.Monitor.Handlers += CheckStart;
            _manager.Monitor.Handlers += CheckLoading;
            _manager.OnBattleEnd += CheckBattleSplit;
            if (_settings.SkipBattleAnimations)
                _manager.OnBattleAnimationStart += SkipBattleAnimation;
            
            _delegatesHooked = true;
        }
        
        private void UnhookDelegates()
        {
            if(!_delegatesHooked)
                return;
            _manager.OnBattleEnd -= CheckBattleSplit;
            
            if (_settings.SkipBattleAnimations)
                _manager.OnBattleAnimationStart -= SkipBattleAnimation;
            if (_manager.Monitor.Handlers != null)
            {
                _manager.Monitor.Handlers -= CheckStart;
                _manager.Monitor.Handlers -= CheckLoading;
            }


            _delegatesHooked = false;

        }

        #endregion
        
        #region Unused interface stuff
        
        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
  
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            
        }

        public float                       HorizontalWidth     => 0;
        public float                       MinimumHeight       => 0;
        public float                       VerticalHeight      => 0;
        public float                       MinimumWidth        => 0;
        public float                       PaddingTop          => 0;
        public float                       PaddingBottom       => 0;
        public float                       PaddingLeft         => 0;
        public float                       PaddingRight        => 0;
        public IDictionary<string, Action> ContextMenuControls => null;
        #endregion
    }
}