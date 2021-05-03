﻿using System;

namespace emulator
{
    public class Renderer
    {
        private readonly PPU PPU;
        public long TimeUntilWhichToPause;
        private readonly FrameSink fs;

        public Renderer(PPU ppu, FrameSink destination, long offset)
        {
            fs = destination;
            PPU = ppu;
            ppu.Mode = Mode.OAMSearch;
            fetcher = new PixelFetcher(PPU);
            TimeUntilWhichToPause += offset;
        }

        public PixelFetcher fetcher;
        public int Stage3TickCount;


        public int TotalTimeSpentInStage3 { get; private set; }

        private Mode? ScheduledModeChange;
        public void Render()
        {
            //Increment mode and set lock states
            if (ScheduledModeChange is not null)
            {
                PPU.Mode = (Mode)ScheduledModeChange;
                ScheduledModeChange = null;

                if (PPU.Mode == Mode.OAMSearch)
                {
                    PPU.OAM.Locked = true;
                    PPU.VRAM.Locked = false;
                }
                if (PPU.Mode == Mode.Transfer)
                {
                    PPU.OAM.Locked = true;
                    PPU.VRAM.Locked = true;
                }

                if (PPU.Mode == Mode.HBlank)
                {
                    PPU.OAM.Locked = false;
                    PPU.VRAM.Locked = false;
                }
                if (PPU.Mode == Mode.VBlank)
                {
                    PPU.OAM.Locked = false;
                    PPU.VRAM.Locked = false;

                    //According to TCAGBD the OAM flag is also triggering on this
                    if (PPU.Enable_VBlankInterrupt || PPU.Enable_OAM_Interrupt)
                    {
                        PPU.EnableLCDCStatusInterrupt();
                    }

                    PPU.EnableVBlankInterrupt();
                    fs.Draw();
                }
            }

            //We should be handling this during the transition from HBlank to OAMSearch
            if (PPU.Mode is Mode.OAMSearch or Mode.VBlank)
            {
                //We only want to increment the line register if we aren't on the very first line
                if (fs.Position != 0 || PPU.Mode == Mode.VBlank)
                {
                    PPU.LY++;
                }

                if (PPU.LY == PPU.LYC)
                {
                    PPU.LYCInterrupt = true;
                }

                if (PPU.LY == 154)
                {
                    PPU.LY = 0;
                    fetcher.FrameFinished();
                    ScheduledModeChange = Mode.OAMSearch;
                    return;
                }
            }

            switch (PPU.Mode)
            {
                case Mode.HBlank:
                if (PPU.Enable_HBlankInterrupt)
                    PPU.EnableLCDCStatusInterrupt();

                TimeUntilWhichToPause += graphics.Constants.ScanLineRemainderAfterOAMSearch - TotalTimeSpentInStage3;

                ScheduledModeChange = PPU.LY == 143 ? Mode.VBlank : Mode.OAMSearch;
                return;
                case Mode.OAMSearch:
                if (PPU.Enable_OAM_Interrupt)
                    PPU.EnableLCDCStatusInterrupt();

                fetcher.GetSprites();
                TimeUntilWhichToPause += graphics.Constants.OAMSearchDuration;
                ScheduledModeChange = Mode.Transfer;
                return;
                case Mode.VBlank:
                TimeUntilWhichToPause += graphics.Constants.ScanlineDuration;
                return;
                case Mode.Transfer:
                {
                    if (PPU.LY != 0 && Stage3TickCount == 0)
                    {
                        Stage3TickCount += 4;
                        TimeUntilWhichToPause += 4;
                        return;
                    }

                    fetcher.Fetch();
                    fetcher.AttemptToPushAPixel();

                    Stage3TickCount++;
                    TimeUntilWhichToPause++;

                    if (fetcher.PixelsSentToLCD == graphics.Constants.ScreenWidth)
                        ResetLineSpecificState();
                    return;
                }
            }
        }

        private void ResetLineSpecificState()
        {
            ScheduledModeChange = Mode.HBlank;

            Span<byte> output = stackalloc byte[graphics.Constants.ScreenWidth];

            for (int i = 0; i < output.Length; i++)
            {
                output[i] = ShadeToGray(fetcher.LineShadeBuffer[i]);
            }

            fs.Write(output);
            fetcher.LineFinished();
            TotalTimeSpentInStage3 = Stage3TickCount;
            Stage3TickCount = 0;
        }

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