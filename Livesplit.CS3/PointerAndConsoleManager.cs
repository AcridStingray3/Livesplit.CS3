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
        
      
        public PointerPath<ushort> BattleId;
        public PointerPath<ushort> BgmId;
        public LogFileMonitor Monitor { get; private set; }


        public bool IsHooked => _game != null && !_game.HasExited;
        
        public void Hook()
        {
            if (IsHooked || DateTime.Now < _nextHookAttempt)
            {
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
            BattleId = new PointerPath<ushort>(_game, 0x00A844E8, 0x5AA24); //This was for 1.02 aka demo it's outdated now
            BgmId = new PointerPath<ushort>(_game, 0x18CB932); //This was for 1.03 it's outdated now
            
            Thread.Sleep(500);
            Monitor = new LogFileMonitor(Path.Combine(Path.GetTempPath(), "sen3log.txt"));
            Monitor.Start();;
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
            
            //BattleId.UpdateAddressValue();
            BgmId.UpdateAddressValue();
            
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