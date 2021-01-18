﻿using System;

namespace emulator
{
    public class PPU
    {
        public readonly Func<long> Clock;
        public readonly Action EnableVBlankInterrupt;
        public readonly Action EnableLCDCStatusInterrupt;
        public FrameSink Writer = new((x) => { });
        public PPU(Func<long> clock, Action enableVBlankInterrupt, Action enableLCDCStatusInterrupt)
        {
            Clock = clock;
            OAM = new OAM();
            VRAM = new VRAM();
            EnableVBlankInterrupt = enableVBlankInterrupt;
            EnableLCDCStatusInterrupt = enableLCDCStatusInterrupt;
        }

        public readonly OAM OAM;
        public readonly VRAM VRAM;

        //FF40 - FF4B, PPU control registers
        //FF40 
        private byte _LCDC;
        public byte LCDC
        {
            get => _LCDC;
            set
            {
                _LCDC = value;
                if (ScreenJustTurnedOn)
                    Renderer = new Renderer(this, Writer); //We want a new renderer so all the internal state resets including clocking
                else if ((!LCDEnable) && Renderer is not null)
                {
                    Writer.Flush(); //If there is a partially written frame when we delete the old renderer the next
                                    //instantiation of renderer will overwrite the end of the buffer because LY starts at 0 despite
                                    //there already being data written to the output buffer

                    Renderer = null; //We want to destroy the old renderer so it can't keep running after requested to turn off
                }
            }
        }


        public byte STAT { get; set; } //FF41

        public byte SCY; //FF42
        public byte SCX; //FF43

        private byte _ly;
        //FF44
        public byte LY
        {
            get => _ly;
            set
            {
                if (value > 154) throw new Exception("monkas");
                _ly = value;
            }
        }
        public byte LYC; //FF45

        public byte DMA; //FF46

        public byte BGP = 0xff; //FF47
        public byte OBP0 = 0xff; //FF48
        public byte OBP1 = 0xff; //FF49

        public byte WY; //FF4A
        public byte WX; //FF4B

        public Shade SpritePalette0(int n) => n switch
        {
            0 => throw new Exception("Unused"),
            1 => (Shade)((OBP0 & 0xC) >> 2),
            2 => (Shade)((OBP0 & 0x30) >> 4),
            3 => (Shade)((OBP0 & 0xC0) >> 6),
            _ => throw new IndexOutOfRangeException()
        };
        public Shade SpritePalette1(int n) => n switch
        {
            0 => throw new Exception("Unused"),
            1 => (Shade)((OBP1 & 0xC) >> 2),
            2 => (Shade)((OBP1 & 0x30) >> 4),
            3 => (Shade)((OBP1 & 0xC0) >> 6),
            _ => throw new IndexOutOfRangeException()
        };

        public Shade BackgroundColor(int n) => n switch
        {
            0 => (Shade)((BGP & 0x3) >> 0),
            1 => (Shade)((BGP & 0xC) >> 2),
            2 => (Shade)((BGP & 0x30) >> 4),
            3 => (Shade)((BGP & 0xC0) >> 6),
            _ => throw new IndexOutOfRangeException()
        };

        public ushort TileMapDisplaySelect => LCDC.GetBit(6) ? 0x9C00 : 0x9800;
        public ushort BGAndWindowTileDataSelect => LCDC.GetBit(4) ? 0x8000 : 0x8800;
        public ushort BGTileMapDisplaySelect => LCDC.GetBit(3) ? 0x9c00 : 0x9800;
        public int SpriteHeight => LCDC.GetBit(2) ? 16 : 8;
        public bool LCDEnable => LCDC.GetBit(7);
        public bool WindowDisplayEnable => LCDC.GetBit(5);
        public bool OBJDisplayEnable => LCDC.GetBit(1);
        public bool BGDisplayEnable => LCDC.GetBit(0);

        public Mode Mode
        {
            get => (Mode)(STAT & 0x03);
            set => STAT = (byte)(STAT & 0xFC | (int)value & 0x3);
        }

        public bool LYCInterrupt
        {
            get => STAT.GetBit(2);
            set
            {
                STAT = STAT.SetBit(2, value);
                if (STAT.GetBit(6))
                    EnableLCDCStatusInterrupt();
            }
        }

        private Renderer Renderer;
        public void Do()
        {
            if (Renderer is not null)
                Renderer.Render();
        }

        //We could have more calls to SetLCDC for other bits in the LCDC register.
        //The LCDCEnable flag is only interesting at the moment it flips and the renderer null check should mean a recent flip
        private bool ScreenJustTurnedOn => LCDEnable && Renderer is null;
    }
}