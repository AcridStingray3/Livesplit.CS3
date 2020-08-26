using System.Diagnostics;

namespace Livesplit.CS3
{
    // Reads from memory using a pointer path with the game's address as a starting point to the offsets
    // Changes to the values are easily detected due to the buffer system of Last and Current values. 
    public class PointerPath<T> where T: unmanaged
    {
        private readonly Process _game;
        private readonly int[] _offsets;
        private T _lastValue;
        private T _currentValue;
        
        public delegate void OnPointerChangeHandler(T lastValue, T currentValue);
        public OnPointerChangeHandler OnPointerChange;

        /**
         * Should never be called before game is hooked or at least launched
         */
        
        public PointerPath(Process game, params int[] offsets)
        {
            _game = game;
            
            _offsets = new int[offsets.Length];
            for (int i = 0; i < offsets.Length; ++i)
            {
                _offsets[i] = offsets[i];
            }

        }

       
        // Updates the values and fires the hook if they have changed
        public void UpdateAddressValue()
        {
            _lastValue = _currentValue;
            _currentValue = _game.Read<T>(_game.MainModule.BaseAddress, _offsets);
            
            if(!_lastValue.Equals(_currentValue))
                OnPointerChange?.Invoke(_lastValue, _currentValue);
        }
     

    }
    
}