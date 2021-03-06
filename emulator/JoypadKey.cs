﻿using System.Windows.Input;

namespace Hardware
{
    public enum JoypadKey
    {
        A,
        B,
        Select,
        Start,
        Right,
        Left,
        Up,
        Down
    }

    internal class Abstraction
    {
        public static JoypadKey? Map(Key k) => k switch
        {
            Key.A => JoypadKey.A,
            Key.S => JoypadKey.B,
            Key.D => JoypadKey.Select,
            Key.F => JoypadKey.Start,
            Key.Right => JoypadKey.Right,
            Key.Left => JoypadKey.Left,
            Key.Up => JoypadKey.Up,
            Key.Down => JoypadKey.Down,
            _ => null
        };
    }
}