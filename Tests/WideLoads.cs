using System;

using generator;

using NUnit.Framework;

namespace Tests
{
    public class WideLoads
    {
        [Test]
        public void LD_BC_d16()
        {
            var dec = Setup0x4020BufferedDecoder();

            var before = dec.Registers.BC.Read();

            dec.StdOps[Unprefixed.LD_BC_d16]();

            var after = dec.Registers.BC.Read();

            Assert.AreNotEqual(before, after);
            Assert.AreEqual(after, 0x4020);
        }
        [Test]
        public void LD_DE_d16()
        {
            var dec = Setup0x4020BufferedDecoder();

            var before = dec.Registers.DE.Read();

            dec.StdOps[Unprefixed.LD_DE_d16]();

            var after = dec.Registers.DE.Read();

            Assert.AreNotEqual(before, after);
            Assert.AreEqual(after, 0x4020);
        }
        [Test]
        public void LD_HL_d16()
        {
            var dec = Setup0x4020BufferedDecoder();

            var before = dec.Registers.HL.Read();

            dec.StdOps[Unprefixed.LD_HL_d16]();

            var after = dec.Registers.HL.Read();

            Assert.AreNotEqual(before, after);
            Assert.AreEqual(after, 0x4020);
        }
        [Test]
        public void LD_SP_d16()
        {
            var dec = Setup0x4020BufferedDecoder();

            var before = dec.Registers.SP.Read();

            dec.StdOps[Unprefixed.LD_SP_d16]();

            var after = dec.Registers.SP.Read();

            Assert.AreNotEqual(before, after);
            Assert.AreEqual(after, 0x4020);
        }
        [Test]
        public void LD_AT_HL_d8()
        {
            var dec = Setup0x77BufferedDecoder();

            var before = dec.Registers.HL.Read();
            var memoryBefore = dec.Storage.Read(before);

            dec.StdOps[Unprefixed.LD_AT_HL_d8]();

            var after = dec.Registers.HL.Read();
            var memoryAfter = dec.Storage.Read(after);

            Assert.AreEqual(before, after);
            Assert.AreNotEqual(memoryBefore, memoryAfter);

            Assert.AreEqual(0x77, memoryAfter);
        }

        [Test]
        public void LDD_AT_HL_A()
        {
            var dec = new Decoder(() => 0);
            dec.Registers.Set(WideRegister.HL, 10);
            dec.Registers.Set(Register.A, 0x77);

            var before = dec.Registers.HL.Read();
            var memoryBefore = dec.Storage.Read(before);

            dec.StdOps[Unprefixed.LDD_AT_HL_A]();

            var after = dec.Registers.HL.Read();
            var memoryAfter = dec.Storage.Read(after);

            var memoryAfterButAtOldHL = dec.Storage.Read(before);

            Assert.AreEqual(before - 1, after);
            Assert.AreEqual(memoryBefore, memoryAfter);

            Assert.AreNotEqual(0x77, memoryAfter);
            Assert.AreEqual(0x77, memoryAfterButAtOldHL);
        }

        [Test]
        public void LDI_AT_HL_A()
        {
            var dec = new Decoder(() => 0);
            dec.Registers.Set(WideRegister.HL, 10);
            dec.Registers.Set(Register.A, 0x77);

            var before = dec.Registers.HL.Read();
            var memoryBefore = dec.Storage.Read(before);

            dec.StdOps[Unprefixed.LDI_AT_HL_A]();

            var after = dec.Registers.HL.Read();
            var memoryAfter = dec.Storage.Read(after);

            var memoryAfterButAtOldHL = dec.Storage.Read(before);

            Assert.AreEqual(before + 1, after);
            Assert.AreEqual(memoryBefore, memoryAfter);

            Assert.AreNotEqual(0x77, memoryAfter);
            Assert.AreEqual(0x77, memoryAfterButAtOldHL);
        }

        [Test]
        public void LD_AT_a16_SP()
        {
            var dec = Setup0x4020BufferedDecoder();
            dec.Registers.SP.Write(0x6688);
            dec.StdOps[Unprefixed.LD_AT_a16_SP]();

            var result = dec.Storage.ReadWide(0x4020);
            Assert.AreEqual(0x6688, result);
        }

        [Test]
        public void ADD_HL_BC()
        {
            var dec = new Decoder(() => 0);

            dec.Registers.HL.Write(0x8a23);
            dec.Registers.BC.Write(0x0605);

            dec.StdOps[Unprefixed.ADD_HL_BC]();

            Assert.AreEqual(0x9028, dec.Registers.HL.Read());
            Assert.IsTrue(dec.Registers.Get(Flag.H));
            Assert.IsTrue(dec.Registers.Get(Flag.NN));
            Assert.IsTrue(dec.Registers.Get(Flag.NC));
        }
        [Test]
        public void ADD_HL_HL()
        {
            var dec = new Decoder(() => 0);

            dec.Registers.HL.Write(0x8a23);

            dec.StdOps[Unprefixed.ADD_HL_HL]();

            Assert.AreEqual(0x1446, dec.Registers.HL.Read());
            Assert.IsTrue(dec.Registers.Get(Flag.H));
            Assert.IsTrue(dec.Registers.Get(Flag.NN));
            Assert.IsTrue(dec.Registers.Get(Flag.C));
        }

        private static Decoder Setup0x4020BufferedDecoder()
        {
            var mem = new byte[] { 0x20, 0x40 }; // little endian
            int memIndex = 0;

            var dec = new Decoder(() =>
             mem[memIndex++]
            );
            return dec;
        }
        private static Decoder Setup0x77BufferedDecoder()
        {
            var mem = new byte[] { 0x77 }; // little endian
            int memIndex = 0;

            var dec = new Decoder(() =>
             mem[memIndex++]
            );
            return dec;
        }
    }
}