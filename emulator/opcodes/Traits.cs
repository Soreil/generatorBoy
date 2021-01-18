﻿namespace emulator
{
    public record Traits
    {
        public bool Immediate;
        public Postfix Postfix;
        public int duration;
        public int? branchDuration;

        public Traits(Operand o)
        {
            Immediate = o.Pointer;
            Postfix = o.Postfix;
        }
        public Traits(bool b, Postfix p)
        {
            Immediate = b;
            Postfix = p;
        }
    }
}