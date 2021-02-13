﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace emulator
{
    public class Renderer
    {
        readonly PPU PPU;
        readonly Func<long> Clock;
        public int TimeUntilWhichToPause;
        readonly Stream fs = Stream.Null;

        public const int TileWidth = 8;
        public const int DisplayWidth = 160;
        public const int TilesPerLine = DisplayWidth / TileWidth;
        public const int DisplayHeight = 144;
        public const int ScanlinesPerFrame = DisplayHeight + 10;

        public const int TicksPerScanline = 456;
        public const int TicksPerFrame = ScanlinesPerFrame * TicksPerScanline;

        public Renderer(PPU ppu, Stream destination = null)
        {
            fs = destination ?? Stream.Null;
            PPU = ppu;
            ppu.Mode = Mode.OAMSearch;
            var startTime = PPU.Clock();
            Clock = () => ppu.Clock() - startTime;
        }

        public List<SpriteAttributes> SpriteAttributes = new();
        public PixelFetcher fetcher;
        public int Stage3TickCount = 0;

        public int PixelsPopped = 0;
        public int PixelsSentToLCD = 0;
        public void Render()
        {
            if (PPU.Mode == Mode.HBlank)
            {
                SetStatInterruptForMode();
                TimeUntilWhichToPause += 376 - Stage3TickCount;
                PPU.Mode = PPU.LY == 143 ? Mode.VBlank : PPU.Mode = Mode.OAMSearch;
                return;
            }
            //We only want to increment the line register if we aren't on the very first line
            //We should be handling this during the transition from HBlank to OAMSearch
            if ((PPU.Mode == Mode.OAMSearch && fs.Position != 0) || PPU.Mode == Mode.VBlank)
            {
                PPU.LY++;
                if (PPU.LY == PPU.LYC) PPU.LYCInterrupt = true;
                if (PPU.LY == 144)
                {
                    PPU.EnableVBlankInterrupt();
                    fs.Flush();
                }
                if (PPU.LY == 154)
                {
                    PPU.Mode = Mode.OAMSearch;
                    PPU.LY = 0;
                }
            }
            if (PPU.Mode == Mode.OAMSearch)
            {
                SetStatInterruptForMode();
                SpriteAttributes = PPU.OAM.SpritesOnLine(PPU.LY, PPU.SpriteHeight);
                TimeUntilWhichToPause += 80;
                PPU.Mode = Mode.Transfer;
                return;
            }
            if (PPU.Mode == Mode.VBlank)
            {
                SetStatInterruptForMode();
                TimeUntilWhichToPause += TicksPerScanline;
                return;
            }

            if (PPU.Mode == Mode.Transfer && fetcher == null)
            {
                PPU.OAM.Locked = true;
                PPU.VRAM.Locked = true;
                fetcher = new PixelFetcher(PPU);
                Stage3TickCount = 0;
                PixelsPopped = 0;
                //fallthrough
            }
            if (PPU.Mode == Mode.Transfer)
            {
                var count = fetcher.Fetch();
                for (int i = 0; i < count && PixelsSentToLCD < 160; i++)
                {
                    var pix = fetcher.RenderPixel();
                    if (pix != null)
                    {
                        PixelsPopped++;
                        if (PixelsPopped > (PPU.SCX & 7))
                            background[PixelsSentToLCD++] = (Shade)pix;
                    }
                }

                Stage3TickCount += count;
                TimeUntilWhichToPause += count;
                //We have to execute this until the full line is drawn by renderpixel calls
            }
            if (PPU.Mode == Mode.Transfer && PixelsSentToLCD == 160)
            {
                PPU.OAM.Locked = false;
                PPU.VRAM.Locked = false;
                var output = new byte[background.Length];
                for (int i = 0; i < output.Length; i++)
                    output[i] = ShadeToGray(background[i]);
                fs.Write(output);
                PPU.Mode = Mode.HBlank;
                fetcher = null;
                return;
            }
            else if (PPU.Mode == Mode.Transfer)
            {
                return; //This is to let it keep drawing the line
            }

            throw new Exception("");
        }

        private void SetStatInterruptForMode()
        {
            if (PPU.Mode == Mode.OAMSearch && PPU.STAT.GetBit(5)) PPU.EnableLCDCStatusInterrupt();
            else if (PPU.Mode == Mode.VBlank && PPU.STAT.GetBit(4) || PPU.STAT.GetBit(5)) PPU.EnableLCDCStatusInterrupt();
            else if (PPU.Mode == Mode.HBlank && PPU.STAT.GetBit(3)) PPU.EnableLCDCStatusInterrupt();
        }

        private readonly Shade[] background = new Shade[DisplayWidth];
        public static byte ShadeToGray(Shade s) => s switch
        {
            Shade.White => 0xff,
            Shade.LightGray => 0xc0,
            Shade.DarkGray => 0x40,
            Shade.Black => 0,
            _ => throw new Exception(),
        };
    }
}