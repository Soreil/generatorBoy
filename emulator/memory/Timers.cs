﻿using System;

namespace emulator
{
    //Timer system handles all Gekkio timer tests except for tima_write_reloading and tma_write_reloading
    public class Timers
    {
        public ushort InternalCounter;

        readonly Action EnableTimerInterrupt;
        public Timers(Action enableTimerInterrupt) => EnableTimerInterrupt = enableTimerInterrupt;

        public void Tick()
        {
            if (DelayTicks > 0)
                if (DelayTicks-- == 0) Tima = TMA;

            if (TimerEnabled)
            {
                var before = (InternalCounter & (1 << TACBitSelected)) == 0;
                InternalCounter++;
                var overflow = (InternalCounter & (1 << TACBitSelected)) == 0;
                if (before == false && overflow == true)
                    IncrementTIMA();
            }
            else InternalCounter++;
        }

        public byte DIV
        {
            get => (byte)((InternalCounter & 0xff00) >> 8);
            set
            {
                var overflow = (InternalCounter & (1 << TACBitSelected)) != 0;
                if (overflow) IncrementTIMA();
                InternalCounter = 0;
            }
        }

        private byte _tac = 0xf8;
        public byte TAC
        {
            get => _tac;
            set
            {
                bool glitch;
                if (!TimerEnabled) glitch = false;
                else
                {
                    if (!value.GetBit(2))
                        glitch = (InternalCounter & (1 << (TACBitSelected))) != 0;
                    else
                        glitch = ((InternalCounter & (1 << (TACBitSelected))) != 0) &&
                                 ((InternalCounter & (1 << (BitPosition(value)))) == 0);
                }
                if (glitch) IncrementTIMA();
                _tac = (byte)((value & 0x7) | 0xf8);
            }
        }

        bool TimerEnabled => TAC.GetBit(2);
        int TACBitSelected => BitPosition(TAC);

        private static byte BitPosition(byte b) => (b & 0x03) switch
        {
            0 => 9,
            1 => 3,
            2 => 5,
            3 => 7,
            _ => throw new NotImplementedException(),
        };

        private byte _tma;
        public byte TMA
        {
            get => _tma;
            set => _tma = value;
        }

        private byte Tima
        {
            get;
            set;
        }
        public byte TIMA
        {
            get => Tima;
            set => Tima = value;
        }

        //When TIMA overflows it should delay writing the value for 4 cycles
        const int DelayDuration = 4;

        int DelayTicks = 0;
        private void IncrementTIMA()
        {
            if (Tima == 0xff)
            {
                DelayTicks = DelayDuration;
                EnableTimerInterrupt();
            }
            Tima++;
        }
    }
}
