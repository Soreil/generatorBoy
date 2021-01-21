﻿using System.Collections.Generic;
using System;

namespace emulator
{
    //MBC1 does not currently do multicart detection and as such won't work correctly since multicarts have different wiring
    internal class MBC1 : MBC
    {
        private readonly byte[] gameROM;
        private readonly List<byte[]> RAMBanks;

        private bool RAMEnabled = false;
        const int ROMBankSize = 0x4000;
        int RAMBankSize = RAMSize;

        int lowBank => GetLowBankNumber();

        //This can return 0/20/40/60h
        private int GetLowBankNumber() => BankingMode == 1 ? (UpperBitsOfROMBank << 5) & (ROMBankCount - 1) : 0;

        private int HighBank() => (LowerBitsOfROMBank | (UpperBitsOfROMBank << 5)) & (ROMBankCount - 1);
        int highBank => HighBank();

        int ramBank => RAMBankCount == 1 ? 0 : (BankingMode == 1 ? UpperBitsOfROMBank : 0);
        int RAMBankCount;
        int ROMBankCount;

        int LowerBitsOfROMBank = 1;
        int UpperBitsOfROMBank = 0;
        int BankingMode = 0;
        public MBC1(CartHeader header, byte[] gameROM)
        {
            this.gameROM = gameROM;
            ROMBankCount = this.gameROM.Length / 0x4000;
            if (header.Type == CartType.MBC1_RAM && header.RAM_Size == 0) header = header with { RAM_Size = 0x2000 };
            RAMBankCount = Math.Max(1, header.RAM_Size / RAMBankSize);
            RAMBanks = new List<byte[]>(RAMBankCount);

            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0)
                RAMBankSize = 0;

            //0x800 is the only alternative bank size
            if (header.RAM_Size == 0x800)
                RAMBankSize = 0x800;

            for (int i = 0; i < RAMBankCount; i++)
                RAMBanks.Add(new byte[RAMBankSize]);
        }

        public override byte this[int n]
        {
            get => n >= RAMStart ? GetRAM(n) : GetROM(n);
            set
            {
                switch (n)
                {
                    case var v when v < 0x2000:
                        RAMEnabled = (value & 0x0F) == 0x0A;
                        break;
                    case var v when v < 0x4000:
                        LowerBitsOfROMBank = (value & 0x1f) == 0 ? 1 : value & 0x1f; //0x1f should be parameterizable depending on if it's multicart
                        break;
                    case var v when v < 0x6000:
                        UpperBitsOfROMBank = value & 0x03;
                        break;
                    case var v when v < 0x8000:
                        BankingMode = value & 0x01;
                        break;
                    default:
                        SetRAM(n, value);
                        break;
                }
            }
        }

        public byte GetROM(int n) => IsUpperBank(n) ? ReadHighBank(n) : ReadLowBank(n);
        private byte ReadLowBank(int n) => gameROM[lowBank * ROMBankSize + n];
        private byte ReadHighBank(int n) => gameROM[highBank * ROMBankSize + (n - ROMBankSize)];

        private bool IsUpperBank(int n) => n >= ROMBankSize;

        public byte GetRAM(int n) => RAMEnabled ? RAMBanks[ramBank][n - RAMStart] : 0xff;
        public byte SetRAM(int n, byte v) => RAMEnabled ? RAMBanks[ramBank][n - RAMStart] = v : _ = v;
    }
}