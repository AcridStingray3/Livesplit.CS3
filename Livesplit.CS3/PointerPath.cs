using System;
using System.Diagnostics;

namespace Livesplit.CS3
{
    //Reads from memory using a pointer path with the game's address as a starting point to the offsets
    //Changes to the values are easily detected due to the buffer system of Last and Current values. 
    public class PointerPath<T> where T: unmanaged
    {
        private readonly Process _game;
        private readonly int[] _offsets;
        public T LastValue { get; private set; }
        public T CurrentValue { get; private set; }

        /**
         * Should never be called before game is hooked or at least launched
         */
        
        public PointerPath(Process game, params int[] offsets)
        {
            this._game = game;
            
            this._offsets = new int[offsets.Length];
            for (int i = 0; i < offsets.Length; ++i)
            {
                this._offsets[i] = offsets[i];
            }

        }

       
        public void UpdateAddressValue()
        {
            this.LastValue = this.CurrentValue;
            this.CurrentValue = this._game.Read<T>(_game.MainModule.BaseAddress, this._offsets);
        }
     

    }
    
}