using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using LiveSplit.Model.Input;

namespace Livesplit.CS3
{
    //The point of this class is to connect the paths and console to the game and update the paths.
    //Values should be gotten from the paths directly but through this class (hence why they're public)
    //A reminder that the DebugMonitor raises an event whenever a line is captured, and that's how the lines should be gotten (from the monitor's event directly)
    
    public class PointerAndConsoleManager : IDisposable
    {
        private DateTime _nextHookAttempt = DateTime.MinValue;
        private Process _game;
        private bool _disablePointer;
        
      
        public PointerPath<ushort> BattleId { get; private set; }
        public PointerPath<byte> Cheating { get; private set; }
        
        public LogFileMonitor Monitor { get; private set; }


        public bool IsHooked => _game != null && !_game.HasExited;

        public void Hook()
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

            //Pointer path initializations
            if (_game.MainModule?.ModuleMemorySize ==  0x01) //TODO placeholder for 1.02
            {

                BattleId = new PointerPath<ushort>(_game, 0x00A844E8, 0x5AA24); 
            }

            else if (_game.MainModule?.ModuleMemorySize ==  0x02) //TODO placeholder for 1.03
            {
                
            }

            else if (_game.MainModule?.ModuleMemorySize ==  0x03) //TODO placeholder for 1.04
            {
                
            }

            else if (_game.MainModule?.ModuleMemorySize == 0x1DEA000)
            {
                BattleId = new PointerPath<ushort>(_game, 0x016C2648, 0x5A408);
                Cheating = new PointerPath<byte>(_game, 0x00C53370, 0x8, 0x18, 0x18, 0x1AA8, 0x8, 0x2F98, 0x2C8, 0x2A0);
            }
            
            else
            {
                _disablePointer = true;
            }
            
            Thread.Sleep(500);
            Monitor = new LogFileMonitor(Path.Combine(Path.GetTempPath(), "sen3log.txt"));
            Monitor.Start();
            _game.Exited += StopMonitor;

            

        }

        private void StopMonitor(object sender, EventArgs e)
        {
            _game.Exited -= StopMonitor;
            Monitor.Stop();
            Monitor.Dispose();
        }

        public void UpdateValues()
        {
            if (!_disablePointer)
            {
                BattleId.UpdateAddressValue();
                Cheating.UpdateAddressValue();
            }

        }

        public void Dispose()
        {
            try
            {
                _game.Exited -= StopMonitor; //god I love duplicate code
                Monitor.Stop();
                Monitor.Dispose();
            }
            catch
            {
                // ignored
            }

            _game?.Dispose();

        }


    }
}