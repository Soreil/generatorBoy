﻿
using NUnit.Framework;

namespace Tests
{
    class FIFORenderer
    {
        [Test]
        public void RenderBackGroundTile()
        {
            //gradient from white to black and back
            byte[] expected = new byte[8] { 0, 1, 2, 3, 3, 2, 1, 0 };
            byte[] got = new byte[8];

            int clock = 0;
            var ppu = new emulator.PPU(() => clock, () => { }, () => { });
            var fetcher = new emulator.PixelFetcher(ppu);

            //black dark light white
            ppu.BGP = 0b11100100;
            //Screen and background on
            ppu.LCDC = 0b10010011;

            //gradient from white to black and back
            ppu.VRAM[emulator.VRAM.Start + 0] = 0b01011010;
            ppu.VRAM[emulator.VRAM.Start + 1] = 0b00111100;

            var totalElapsed = 0;

            var elapsed = fetcher.Fetch();
            totalElapsed += elapsed;

            Assert.AreEqual(2, elapsed);
            Assert.AreEqual(1, fetcher.FetcherStep);
            elapsed = fetcher.Fetch();
            totalElapsed += elapsed;

            Assert.AreEqual(2, elapsed);
            Assert.AreEqual(2, fetcher.FetcherStep);
            elapsed = fetcher.Fetch();
            totalElapsed += elapsed;

            Assert.AreEqual(2, elapsed);
            Assert.AreEqual(3, fetcher.FetcherStep);
            elapsed = fetcher.Fetch();
            totalElapsed += elapsed;

            Assert.AreEqual(2, elapsed);
            Assert.AreEqual(0, fetcher.FetcherStep);

            Assert.AreEqual(8, totalElapsed);

            for (int i = 0; i < 8; i++)
            {
                var s = fetcher.RenderPixel();
                Assert.NotNull(s);
                got[i] = (byte)s;
            }

            Assert.AreEqual(expected, got);
        }
        [Test]
        public void RenderBackGroundLine()
        {
            byte[] expected = new byte[160] {
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            3, 3, 3, 3, 3, 3, 3, 3,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0
            };
            byte[] expectedLY1 = new byte[160] {
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0
            };
            byte[] got = new byte[160];

            int clock = 0;
            var ppu = new emulator.PPU(() => clock, () => { }, () => { });
            var fetcher = new emulator.PixelFetcher(ppu);

            //black dark light white
            ppu.BGP = 0b11100100;
            //Screen and background on
            ppu.LCDC = 0b10010011;

            //gradient from white to black and back
            ppu.VRAM[emulator.VRAM.Start + 0x00] = 0b01011010;
            ppu.VRAM[emulator.VRAM.Start + 0x01] = 0b00111100;
            //Second line
            ppu.VRAM[emulator.VRAM.Start + 0x02] = 0b01011010;
            ppu.VRAM[emulator.VRAM.Start + 0x03] = 0b00111100;
            //pure black tile (top line)
            ppu.VRAM[emulator.VRAM.Start + 0x10] = 0b00000000;
            ppu.VRAM[emulator.VRAM.Start + 0x11] = 0b00000000;
            //second line
            ppu.VRAM[emulator.VRAM.Start + 0x12] = 0b00000000;
            ppu.VRAM[emulator.VRAM.Start + 0x13] = 0b00000000;
            //pure white tile (top line)
            ppu.VRAM[emulator.VRAM.Start + 0x20] = 0b11111111;
            ppu.VRAM[emulator.VRAM.Start + 0x21] = 0b11111111;
            //no second line
            ppu.VRAM[emulator.VRAM.Start + 0x22] = 0b00000000;
            ppu.VRAM[emulator.VRAM.Start + 0x23] = 0b00000000;

            //Make the second map entry point to the second tile (pure black)
            ppu.VRAM[ppu.TileMapDisplaySelect + 1] = 0x01;
            //Make the second to last map entry point to the third tile (pure white)
            ppu.VRAM[ppu.TileMapDisplaySelect + 2] = 0x02;

            for (int tile = 0; tile < 20; tile++)
            {
                var totalElapsed = 0;

                var elapsed = fetcher.Fetch();
                totalElapsed += elapsed;

                Assert.AreEqual(2, elapsed);
                Assert.AreEqual(1, fetcher.FetcherStep);
                elapsed = fetcher.Fetch();
                totalElapsed += elapsed;

                Assert.AreEqual(2, elapsed);
                Assert.AreEqual(2, fetcher.FetcherStep);
                elapsed = fetcher.Fetch();
                totalElapsed += elapsed;

                Assert.AreEqual(2, elapsed);
                Assert.AreEqual(3, fetcher.FetcherStep);
                elapsed = fetcher.Fetch();
                totalElapsed += elapsed;

                Assert.AreEqual(2, elapsed);
                Assert.AreEqual(0, fetcher.FetcherStep);

                Assert.AreEqual(8, totalElapsed);

                for (int i = 0; i < 8 && fetcher.scanlineX < 160; i++)
                {
                    var s = fetcher.RenderPixel();
                    Assert.NotNull(s);
                    got[(tile * 8) + i] = (byte)s;
                    fetcher.scanlineX++;
                }

            }
            string line = "";
            foreach (var v in expected) line += v.ToString();
            System.Console.WriteLine(line);
            line = "";
            foreach (var v in got) line += v.ToString();
            System.Console.WriteLine(line);
            Assert.AreEqual(expected, got);

            ppu.LY++;
            fetcher.scanlineX = 0;

            for (int tile = 0; tile < 20; tile++)
            {
                var totalElapsed = 0;

                var elapsed = fetcher.Fetch();
                totalElapsed += elapsed;

                Assert.AreEqual(2, elapsed);
                Assert.AreEqual(1, fetcher.FetcherStep);
                elapsed = fetcher.Fetch();
                totalElapsed += elapsed;

                Assert.AreEqual(2, elapsed);
                Assert.AreEqual(2, fetcher.FetcherStep);
                elapsed = fetcher.Fetch();
                totalElapsed += elapsed;

                Assert.AreEqual(2, elapsed);
                Assert.AreEqual(3, fetcher.FetcherStep);
                elapsed = fetcher.Fetch();
                totalElapsed += elapsed;

                Assert.AreEqual(2, elapsed);
                Assert.AreEqual(0, fetcher.FetcherStep);

                Assert.AreEqual(8, totalElapsed);

                for (int i = 0; i < 8 && fetcher.scanlineX < 160; i++)
                {
                    var s = fetcher.RenderPixel();
                    Assert.NotNull(s);
                    got[(tile * 8) + i] = (byte)s;
                    fetcher.scanlineX++;
                }

            }
            line = "";
            foreach (var v in expectedLY1) line += v.ToString();
            System.Console.WriteLine(line);
            line = "";
            foreach (var v in got) line += v.ToString();
            System.Console.WriteLine(line);
            Assert.AreEqual(expectedLY1, got);
        }
    }
}