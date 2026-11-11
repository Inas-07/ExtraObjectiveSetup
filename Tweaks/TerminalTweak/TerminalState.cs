using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraObjectiveSetup.Tweaks.TerminalTweak
{
    public struct TerminalState
    {
        public bool Enabled = true;

        public bool Approached = false;

        private int _data;
        private byte _length;

        public TerminalState()
        {
            _data = 0;
            _length = 0;
        }

        public TerminalState(bool Enabled) { this.Enabled = Enabled; }

        public TerminalState(bool[] logFileRead) { Value = logFileRead; }

        public TerminalState(TerminalState o)
        {
            Enabled = o.Enabled;
            Approached = o.Approached;
            _data = o._data;
            _length = o._length;
        }

        /// <summary>
        /// Which logfiles have been read, packed into one int so the whole state fits into the replicator.
        /// </summary>
        public bool[] Value
        {
            readonly get
            {
                bool[] arr = new bool[_length];
                for (int i = 0; i < _length; i++)
                {
                    arr[i] = (_data & (1 << i)) != 0;
                }
                return arr;
            }
            set
            {
                if (value.Length > 32) return;

                _data = 0;
                _length = (byte)value.Length;
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i])
                    {
                        _data |= 1 << i;
                    }
                }
            }
        }
    }
}
