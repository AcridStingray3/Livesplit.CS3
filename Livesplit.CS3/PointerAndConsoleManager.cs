using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Livesplit.CS3
{
    // The point of this class is to connect the paths and console to the game and update the paths.
    // Values should be gotten from the paths directly but through this class (hence why they're public)
    // A reminder that the DebugMonitor raises an event whenever a line is captured, and that's how the lines should be gotten (from the monitor's event directly)
    
    public class PointerAndConsoleManager : IDisposable
    {
        private DateTime _nextHookAttempt = DateTime.MinValue;
        private Process _game;
        private bool _disablePointer;

        private PointerPath<ushort> _battleID;
        public delegate void OnBattleEndHandler(BattleEnums endedBattle);
        public OnBattleEndHandler OnBattleEnd;

        private PointerPath<byte> _cheating;
        public delegate void OnBattleAnimationStartHandler();
        public OnBattleAnimationStartHandler OnBattleAnimationStart;
        
        public LogFileMonitor Monitor { get; private set; } // Honestly really tempted to make a dummy hook that is literally just a passthrough, so as to not have to access a.b.c. Ugly dependency
        
        public bool IsHooked => _game != null && !_game.HasExited;
        

        public void Hook() // This is honestly just a constructor I hate this design where the constructor is not a real constructor dude. 5 months ago me is an idiot
        {
            if (IsHooked || DateTime.Now < _nextHookAttempt)
            {
                _disablePointer = false;
                return;
            }

            _nextHookAttempt = DateTime.Now.AddSeconds(1);
            
            Process[] processes = Process.GetProcessesByName("ed8_3_PC");
            if (processes.Length == 0)
            {
                return;
            }

            _game = processes[0];
            MemoryReader.Update64Bit(_game);

            switch (_game.MainModule?.ModuleMemorySize)
            {
                //Pointer path initializations
                //TODO placeholder for 1.02
                case 0x01: _battleID = new PointerPath<ushort>(_game, 0x00A844E8, 0x5AA24);
                    break;
                //TODO placeholder for 1.03
                case 0x02: break;
                //TODO placeholder for 1.04
                case 0x03: break;
                // 1.05
                case 0x1DEA000:
                    _battleID = new PointerPath<ushort>(_game, 0x016C2648, 0x5A408);
                    _cheating = new PointerPath<byte>(_game, 0x00A1C6C0, 0x40, 0x150, 0x28, 0x70, 0x850, 0x78, 0x10, 0x40, 0x4B38, 0x2F98, 0x2C8, 0x2A0);
                    break;
                default: _disablePointer = true;
                    break;
            }
            
            Thread.Sleep(500);
            Monitor = new LogFileMonitor(Path.Combine(Path.GetTempPath(), "sen3log.txt"));
            Monitor.Start();
            _game.Exited += OnGameExit;
            if(_disablePointer)
                return;
            
            _battleID.OnPointerChange += CheckBattleSplit;
            _cheating.OnPointerChange += CheckSkipAnimation;



        }

        public void UpdateValues()
        {
            if (_disablePointer) return;
            _battleID.UpdateAddressValue();
            _cheating.UpdateAddressValue();

        }

        private void CheckBattleSplit(ushort oldID, ushort newID)
        {
            if(oldID == 0 || newID != 0) return; // A battle has started, not ended
            
            if (!Enum.IsDefined(typeof(BattleEnums), oldID))
            {
                Debug.Print($"The battle ID value {oldID} isn't defined!");
                return;
            }

            Debug.Print("Firing the Battle End Delegate");
            OnBattleEnd.Invoke((BattleEnums)oldID);
            
        }
        
        private void CheckSkipAnimation(byte lastvalue, byte currentvalue)
        {
            if(currentvalue != 1 || lastvalue != 0) return;
            
            Debug.Print("Firing the Animation Start Delegate");
            OnBattleAnimationStart.Invoke();
        }

        private void OnGameExit(object sender, EventArgs e)
        {
            Dispose();
        }
        
        public void Dispose()
        {
            try
            {
                if (!_disablePointer)
                {
                    _battleID.OnPointerChange -= CheckBattleSplit;
                    _cheating.OnPointerChange -= CheckSkipAnimation;
                }

                _game.Exited -= OnGameExit;
                Monitor.Stop();
                Monitor.Dispose();
            }
            catch
            {
                // ignored, else the component refuses to close
            }

            _game?.Dispose();

        }


    }
}