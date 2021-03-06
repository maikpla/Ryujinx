using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Tests.Cpu
{
    public class CpuTestSimdArithmetic : CpuTest
    {
        [TestCase(0x1E224820u, 0x0000000000000000ul, 0x0000000080000000ul, 0x0000000000000000ul)] // FMAX S0, S1, S2
        [TestCase(0x1E224820u, 0x0000000080000000ul, 0x0000000000000000ul, 0x0000000000000000ul)]
        [TestCase(0x1E224820u, 0x0000000080000000ul, 0x0000000080000000ul, 0x0000000080000000ul)]
        [TestCase(0x1E224820u, 0x0000000080000000ul, 0x000000003DCCCCCDul, 0x000000003DCCCCCDul)]
        [TestCase(0x1E224820u, 0x000000003DCCCCCDul, 0x000000003C9623B1ul, 0x000000003DCCCCCDul)]
        [TestCase(0x1E224820u, 0x000000008BA98D27ul, 0x0000000000000076ul, 0x0000000000000076ul)]
        [TestCase(0x1E224820u, 0x00000000807FFFFFul, 0x000000007F7FFFFFul, 0x000000007F7FFFFFul)]
        [TestCase(0x1E224820u, 0x000000007F7FFFFFul, 0x00000000807FFFFFul, 0x000000007F7FFFFFul)]
        [TestCase(0x1E224820u, 0x000000007FC00000ul, 0x000000003F800000ul, 0x000000007FC00000ul)]
        [TestCase(0x1E224820u, 0x000000003F800000ul, 0x000000007FC00000ul, 0x000000007FC00000ul)]
        [TestCase(0x1E224820u, 0x000000007F800001ul, 0x000000007FC00042ul, 0x000000007FC00001ul)]
        [TestCase(0x1E224820u, 0x000000007FC00042ul, 0x000000007F800001ul, 0x000000007FC00001ul)]
        [TestCase(0x1E224820u, 0x000000007FC0000Aul, 0x000000007FC0000Bul, 0x000000007FC0000Aul)]
        [TestCase(0x1E624820u, 0x0000000000000000ul, 0x8000000000000000ul, 0x0000000000000000ul)] // FMAX D0, D1, D2
        [TestCase(0x1E624820u, 0x8000000000000000ul, 0x0000000000000000ul, 0x0000000000000000ul)]
        [TestCase(0x1E624820u, 0x8000000000000000ul, 0x8000000000000000ul, 0x8000000000000000ul)]
        [TestCase(0x1E624820u, 0x8000000000000000ul, 0x3FF3333333333333ul, 0x3FF3333333333333ul)]
        public void Fmax_S(uint Opcode, ulong A, ulong B, ulong Result)
        {
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x80000000u, 0x80000000u, 0x00000000u, 0x00000000u, 0x00000000u, 0x00000000u)]
        [TestCase(0x00000000u, 0x00000000u, 0x80000000u, 0x80000000u, 0x00000000u, 0x00000000u)]
        [TestCase(0x80000000u, 0x80000000u, 0x80000000u, 0x80000000u, 0x80000000u, 0x80000000u)]
        [TestCase(0x80000000u, 0x80000000u, 0x3DCCCCCDu, 0x3DCCCCCDu, 0x3DCCCCCDu, 0x3DCCCCCDu)]
        [TestCase(0x3DCCCCCDu, 0x3DCCCCCDu, 0x3C9623B1u, 0x3C9623B1u, 0x3DCCCCCDu, 0x3DCCCCCDu)]
        [TestCase(0x8BA98D27u, 0x8BA98D27u, 0x00000076u, 0x00000076u, 0x00000076u, 0x00000076u)]
        [TestCase(0x807FFFFFu, 0x807FFFFFu, 0x7F7FFFFFu, 0x7F7FFFFFu, 0x7F7FFFFFu, 0x7F7FFFFFu)]
        [TestCase(0x7F7FFFFFu, 0x7F7FFFFFu, 0x807FFFFFu, 0x807FFFFFu, 0x7F7FFFFFu, 0x7F7FFFFFu)]
        [TestCase(0x7FC00000u, 0x7FC00000u, 0x3F800000u, 0x3F800000u, 0x7FC00000u, 0x7FC00000u)]
        [TestCase(0x3F800000u, 0x3F800000u, 0x7FC00000u, 0x7FC00000u, 0x7FC00000u, 0x7FC00000u)]
        [TestCase(0x7F800001u, 0x7F800001u, 0x7FC00042u, 0x7FC00042u, 0x7FC00001u, 0x7FC00001u)]
        [TestCase(0x7FC00042u, 0x7FC00042u, 0x7F800001u, 0x7F800001u, 0x7FC00001u, 0x7FC00001u)]
        [TestCase(0x7FC0000Au, 0x7FC0000Au, 0x7FC0000Bu, 0x7FC0000Bu, 0x7FC0000Au, 0x7FC0000Au)]
        public void Fmax_V(uint A, uint B, uint C, uint D, uint Result0, uint Result1)
        {
            uint Opcode = 0x4E22F420; // FMAX V0.4S, V1.4S, V2.4S

            Vector128<float> V1 = MakeVectorE0E1(A, B);
            Vector128<float> V2 = MakeVectorE0E1(C, D);

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(Result1));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0x1E225820u, 0x0000000000000000ul, 0x0000000080000000ul, 0x0000000080000000ul)] // FMIN S0, S1, S2
        [TestCase(0x1E225820u, 0x0000000080000000ul, 0x0000000000000000ul, 0x0000000080000000ul)]
        [TestCase(0x1E225820u, 0x0000000080000000ul, 0x0000000080000000ul, 0x0000000080000000ul)]
        [TestCase(0x1E225820u, 0x0000000080000000ul, 0x000000003DCCCCCDul, 0x0000000080000000ul)]
        [TestCase(0x1E225820u, 0x000000003DCCCCCDul, 0x000000003C9623B1ul, 0x000000003C9623B1ul)]
        [TestCase(0x1E225820u, 0x000000008BA98D27ul, 0x0000000000000076ul, 0x000000008BA98D27ul)]
        [TestCase(0x1E225820u, 0x00000000807FFFFFul, 0x000000007F7FFFFFul, 0x00000000807FFFFFul)]
        [TestCase(0x1E225820u, 0x000000007F7FFFFFul, 0x00000000807FFFFFul, 0x00000000807FFFFFul)]
        [TestCase(0x1E225820u, 0x000000007FC00000ul, 0x000000003F800000ul, 0x000000007FC00000ul)]
        [TestCase(0x1E225820u, 0x000000003F800000ul, 0x000000007FC00000ul, 0x000000007FC00000ul)]
        [TestCase(0x1E225820u, 0x000000007F800001ul, 0x000000007FC00042ul, 0x000000007FC00001ul)]
        [TestCase(0x1E225820u, 0x000000007FC00042ul, 0x000000007F800001ul, 0x000000007FC00001ul)]
        [TestCase(0x1E225820u, 0x000000007FC0000Aul, 0x000000007FC0000Bul, 0x000000007FC0000Aul)]
        [TestCase(0x1E625820u, 0x0000000000000000ul, 0x8000000000000000ul, 0x8000000000000000ul)] // FMIN D0, D1, D2
        [TestCase(0x1E625820u, 0x8000000000000000ul, 0x0000000000000000ul, 0x8000000000000000ul)]
        [TestCase(0x1E625820u, 0x8000000000000000ul, 0x8000000000000000ul, 0x8000000000000000ul)]
        [TestCase(0x1E625820u, 0x8000000000000000ul, 0x3FF3333333333333ul, 0x8000000000000000ul)]
        public void Fmin_S(uint Opcode, ulong A, ulong B, ulong Result)
        {
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x80000000u, 0x80000000u, 0x00000000u, 0x00000000u, 0x80000000u, 0x80000000u)]
        [TestCase(0x00000000u, 0x00000000u, 0x80000000u, 0x80000000u, 0x80000000u, 0x80000000u)]
        [TestCase(0x80000000u, 0x80000000u, 0x80000000u, 0x80000000u, 0x80000000u, 0x80000000u)]
        [TestCase(0x80000000u, 0x80000000u, 0x3DCCCCCDu, 0x3DCCCCCDu, 0x80000000u, 0x80000000u)]
        [TestCase(0x3DCCCCCDu, 0x3DCCCCCDu, 0x3C9623B1u, 0x3C9623B1u, 0x3C9623B1u, 0x3C9623B1u)]
        [TestCase(0x8BA98D27u, 0x8BA98D27u, 0x00000076u, 0x00000076u, 0x8BA98D27u, 0x8BA98D27u)]
        [TestCase(0x807FFFFFu, 0x807FFFFFu, 0x7F7FFFFFu, 0x7F7FFFFFu, 0x807FFFFFu, 0x807FFFFFu)]
        [TestCase(0x7F7FFFFFu, 0x7F7FFFFFu, 0x807FFFFFu, 0x807FFFFFu, 0x807FFFFFu, 0x807FFFFFu)]
        [TestCase(0x7FC00000u, 0x7FC00000u, 0x3F800000u, 0x3F800000u, 0x7FC00000u, 0x7FC00000u)]
        [TestCase(0x3F800000u, 0x3F800000u, 0x7FC00000u, 0x7FC00000u, 0x7FC00000u, 0x7FC00000u)]
        [TestCase(0x7F800001u, 0x7F800001u, 0x7FC00042u, 0x7FC00042u, 0x7FC00001u, 0x7FC00001u)]
        [TestCase(0x7FC00042u, 0x7FC00042u, 0x7F800001u, 0x7F800001u, 0x7FC00001u, 0x7FC00001u)]
        [TestCase(0x7FC0000Au, 0x7FC0000Au, 0x7FC0000Bu, 0x7FC0000Bu, 0x7FC0000Au, 0x7FC0000Au)]
        public void Fmin_V(uint A, uint B, uint C, uint D, uint Result0, uint Result1)
        {
            uint Opcode = 0x4EA2F420; // FMIN V0.4S, V1.4S, V2.4S

            Vector128<float> V1 = MakeVectorE0E1(A, B);
            Vector128<float> V2 = MakeVectorE0E1(C, D);

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(Result1));
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("FMUL S6, S1, V0.S[2]")]
        public void Fmul_Se([Random(10)] float A, [Random(10)] float B)
        {
            AThreadState ThreadState = SingleOpcode(0x5F809826,
                V1: Sse.SetVector128(0, 0, 0, A),
                V0: Sse.SetVector128(0, B, 0, 0));

            Assert.That(Sse41.Extract(ThreadState.V6, (byte)0), Is.EqualTo(A * B));

            CompareAgainstUnicorn();
        }

        [TestCase(0x00000000u, 0x7F800000u)]
        [TestCase(0x80000000u, 0xFF800000u)]
        [TestCase(0x00FFF000u, 0x7E000000u)]
        [TestCase(0x41200000u, 0x3DCC8000u)]
        [TestCase(0xC1200000u, 0xBDCC8000u)]
        [TestCase(0x001FFFFFu, 0x7F800000u)]
        [TestCase(0x007FF000u, 0x7E800000u)]
        public void Frecpe_S(uint A, uint Result)
        {
            uint Opcode = 0x5EA1D820; // FRECPE S0, S1

            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1);

            Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result));

            CompareAgainstUnicorn();
        }

        [Test, Description("FRECPS D0, D1, D2"), Ignore("Not accurate enough.")]
        public void Frecps_S([Random(10)] double A, [Random(10)] double B)
        {
            AThreadState ThreadState = SingleOpcode(0x5E62FC20,
                V1: MakeVectorE0(A),
                V2: MakeVectorE0(B));

            Assert.That(VectorExtractDouble(ThreadState.V0, (byte)0), Is.EqualTo(2 - (A * B)));

            CompareAgainstUnicorn();
        }

        [Test, Description("FRECPS V4.4S, V2.4S, V0.4S")]
        public void Frecps_V([Random(10)] float A, [Random(10)] float B)
        {
            AThreadState ThreadState = SingleOpcode(0x4E20FC44,
                V2: Sse.SetAllVector128(A),
                V0: Sse.SetAllVector128(B));

            float Result = (float)(2 - ((double)A * (double)B));

            Assert.Multiple(() =>
            {
                Assert.That(Sse41.Extract(ThreadState.V4, (byte)0), Is.EqualTo(Result));
                Assert.That(Sse41.Extract(ThreadState.V4, (byte)1), Is.EqualTo(Result));
                Assert.That(Sse41.Extract(ThreadState.V4, (byte)2), Is.EqualTo(Result));
                Assert.That(Sse41.Extract(ThreadState.V4, (byte)3), Is.EqualTo(Result));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0x3FE66666u, false, 0x40000000u)]
        [TestCase(0x3F99999Au, false, 0x3F800000u)]
        [TestCase(0x404CCCCDu, false, 0x40400000u)]
        [TestCase(0x40733333u, false, 0x40800000u)]
        [TestCase(0x3FC00000u, false, 0x40000000u)]
        [TestCase(0x40200000u, false, 0x40400000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        public void Frinta_S(uint A, bool DefaultNaN, uint Result)
        {
            uint Opcode = 0x1E264020; // FRINTA S0, S1

            Vector128<float> V1 = MakeVectorE0(A);

            int FpcrTemp = 0x0;
            if(DefaultNaN)
            {
                FpcrTemp = 0x2000000;
            }

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

            Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result));

            CompareAgainstUnicorn();
        }

        [Ignore("Wrong opcodes.")]
        [TestCase(0x6E618820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)] // FRINTA V0.2D, V1.2D
        [TestCase(0x6E618820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6E618820u, 0x3FF8000000000000ul, 0x3FF8000000000000ul, false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F80000040000000ul, 0x3F80000040000000ul)] // FRINTX V0.4S, V1.4S
        [TestCase(0x6E219820u, 0x3FC000003FC00000ul, 0x3FC000003FC00000ul, false, 0x4000000040000000ul, 0x4000000040000000ul)]
        [TestCase(0x2E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F80000040000000ul, 0x0000000000000000ul)] // FRINTX V0.2S, V1.2S
        [TestCase(0x2E219820u, 0x3FC000003FC00000ul, 0x3FC000003FC00000ul, false, 0x4000000040000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E218820u, 0x0000000080000000ul, 0x0000000000000000ul, false, 0x0000000080000000ul, 0x0000000000000000ul)] // FRINTA V0.2S, V1.2S
        [TestCase(0x2E218820u, 0x7F800000FF800000ul, 0x0000000000000000ul, false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E218820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E218820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frinta_V(uint Opcode, ulong A, ulong B, bool DefaultNaN, ulong Result0, ulong Result1)
        {
            Vector128<float> V1 = MakeVectorE0E1(A, B);

            int FpcrTemp = 0x0;
            if(DefaultNaN)
            {
                FpcrTemp = 0x2000000;
            }

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(Result1));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0x3FE66666u, 'N', false, 0x40000000u)]
        [TestCase(0x3F99999Au, 'N', false, 0x3F800000u)]
        [TestCase(0x404CCCCDu, 'P', false, 0x40800000u)]
        [TestCase(0x40733333u, 'P', false, 0x40800000u)]
        [TestCase(0x404CCCCDu, 'M', false, 0x40400000u)]
        [TestCase(0x40733333u, 'M', false, 0x40400000u)]
        [TestCase(0x3F99999Au, 'Z', false, 0x3F800000u)]
        [TestCase(0x3FE66666u, 'Z', false, 0x3F800000u)]
        [TestCase(0x00000000u, 'N', false, 0x00000000u)]
        [TestCase(0x00000000u, 'P', false, 0x00000000u)]
        [TestCase(0x00000000u, 'M', false, 0x00000000u)]
        [TestCase(0x00000000u, 'Z', false, 0x00000000u)]
        [TestCase(0x80000000u, 'N', false, 0x80000000u)]
        [TestCase(0x80000000u, 'P', false, 0x80000000u)]
        [TestCase(0x80000000u, 'M', false, 0x80000000u)]
        [TestCase(0x80000000u, 'Z', false, 0x80000000u)]
        [TestCase(0x7F800000u, 'N', false, 0x7F800000u)]
        [TestCase(0x7F800000u, 'P', false, 0x7F800000u)]
        [TestCase(0x7F800000u, 'M', false, 0x7F800000u)]
        [TestCase(0x7F800000u, 'Z', false, 0x7F800000u)]
        [TestCase(0xFF800000u, 'N', false, 0xFF800000u)]
        [TestCase(0xFF800000u, 'P', false, 0xFF800000u)]
        [TestCase(0xFF800000u, 'M', false, 0xFF800000u)]
        [TestCase(0xFF800000u, 'Z', false, 0xFF800000u)]
        [TestCase(0xFF800001u, 'N', false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'P', false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'M', false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'Z', false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'N', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'P', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'M', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'Z', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'N', false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'P', false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'M', false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'Z', false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'N', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'P', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'M', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'Z', true,  0x7FC00000u, Ignore = "NaN test.")]
        public void Frinti_S(uint A, char RoundType, bool DefaultNaN, uint Result)
        {
            uint Opcode = 0x1E27C020; // FRINTI S0, S1

            Vector128<float> V1 = MakeVectorE0(A);

            int FpcrTemp = 0x0;
            switch(RoundType)
            {
                case 'N': FpcrTemp = 0x0;      break;
                case 'P': FpcrTemp = 0x400000; break;
                case 'M': FpcrTemp = 0x800000; break;
                case 'Z': FpcrTemp = 0xC00000; break;
            }
            if(DefaultNaN)
            {
                FpcrTemp |= 1 << 25;
            }

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

            Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x6EE19820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'N', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)] // FRINTI V0.2D, V1.2D
        [TestCase(0x6EE19820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'N', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'P', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'P', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'M', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'M', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'Z', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'Z', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'N', false, 0x3F80000040000000ul, 0x3F80000040000000ul)] // FRINTI V0.4S, V1.4S
        [TestCase(0x6EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'P', false, 0x4000000040000000ul, 0x4000000040000000ul)]
        [TestCase(0x6EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'M', false, 0x3F8000003F800000ul, 0x3F8000003F800000ul)]
        [TestCase(0x6EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'Z', false, 0x3F8000003F800000ul, 0x3F8000003F800000ul)]
        [TestCase(0x2EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'N', false, 0x3F80000040000000ul, 0x0000000000000000ul)] // FRINTI V0.2S, V1.2S
        [TestCase(0x2EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'P', false, 0x4000000040000000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'M', false, 0x3F8000003F800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'Z', false, 0x3F8000003F800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x0000000080000000ul, 0x0000000000000000ul, 'N', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x0000000080000000ul, 0x0000000000000000ul, 'P', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x0000000080000000ul, 0x0000000000000000ul, 'M', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x0000000080000000ul, 0x0000000000000000ul, 'Z', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'N', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'P', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'M', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'Z', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'N', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'P', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'M', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'Z', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'N', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'P', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'M', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'Z', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frinti_V(uint Opcode, ulong A, ulong B, char RoundType, bool DefaultNaN, ulong Result0, ulong Result1)
        {
            Vector128<float> V1 = MakeVectorE0E1(A, B);

            int FpcrTemp = 0x0;
            switch(RoundType)
            {
                case 'N': FpcrTemp = 0x0;      break;
                case 'P': FpcrTemp = 0x400000; break;
                case 'M': FpcrTemp = 0x800000; break;
                case 'Z': FpcrTemp = 0xC00000; break;
            }
            if(DefaultNaN)
            {
                FpcrTemp |= 1 << 25;
            }

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(Result1));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0x3FE66666u, false, 0x3F800000u)]
        [TestCase(0x3F99999Au, false, 0x3F800000u)]
        [TestCase(0x404CCCCDu, false, 0x40400000u)]
        [TestCase(0x40733333u, false, 0x40400000u)]
        [TestCase(0x3FC00000u, false, 0x3F800000u)]
        [TestCase(0x40200000u, false, 0x40000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        public void Frintm_S(uint A, bool DefaultNaN, uint Result)
        {
            uint Opcode = 0x1E254020; // FRINTM S0, S1

            Vector128<float> V1 = MakeVectorE0(A);

            int FpcrTemp = 0x0;
            if(DefaultNaN)
            {
                FpcrTemp = 0x2000000;
            }

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

            Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x4E619820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)] // FRINTM V0.2D, V1.2D
        [TestCase(0x4E619820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x4E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F8000003F800000ul, 0x3F8000003F800000ul)] // FRINTM V0.4S, V1.4S
        [TestCase(0x0E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F8000003F800000ul, 0x0000000000000000ul)] // FRINTM V0.2S, V1.2S
        [TestCase(0x0E219820u, 0x0000000080000000ul, 0x0000000000000000ul, false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x0E219820u, 0x7F800000FF800000ul, 0x0000000000000000ul, false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x0E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x0E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frintm_V(uint Opcode, ulong A, ulong B, bool DefaultNaN, ulong Result0, ulong Result1)
        {
            Vector128<float> V1 = MakeVectorE0E1(A, B);

            int FpcrTemp = 0x0;
            if(DefaultNaN)
            {
                FpcrTemp = 0x2000000;
            }

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(Result1));
            });

            CompareAgainstUnicorn();
        }

        [Ignore("Wrong opcode.")]
        [TestCase(0x3FE66666u, false, 0x40000000u)]
        [TestCase(0x3F99999Au, false, 0x3F800000u)]
        [TestCase(0x404CCCCDu, false, 0x40400000u)]
        [TestCase(0x40733333u, false, 0x40800000u)]
        [TestCase(0x3FC00000u, false, 0x40000000u)]
        [TestCase(0x40200000u, false, 0x40400000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        public void Frintn_S(uint A, bool DefaultNaN, uint Result)
        {
            uint Opcode = 0x1E264020; // FRINTA S0, S1

            Vector128<float> V1 = MakeVectorE0(A);

            int FpcrTemp = 0x0;
            if(DefaultNaN)
            {
                FpcrTemp = 0x2000000;
            }

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

            Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x4E618820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)] // FRINTN V0.2D, V1.2D
        [TestCase(0x4E618820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x4E618820u, 0x3FF8000000000000ul, 0x3FF8000000000000ul, false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x4E218820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F80000040000000ul, 0x3F80000040000000ul)] // FRINTN V0.4S, V1.4S
        [TestCase(0x4E218820u, 0x3FC000003FC00000ul, 0x3FC000003FC00000ul, false, 0x4000000040000000ul, 0x4000000040000000ul)]
        [TestCase(0x0E218820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F80000040000000ul, 0x0000000000000000ul)] // FRINTN V0.2S, V1.2S
        [TestCase(0x0E218820u, 0x3FC000003FC00000ul, 0x3FC000003FC00000ul, false, 0x4000000040000000ul, 0x0000000000000000ul)]
        [TestCase(0x0E218820u, 0x0000000080000000ul, 0x0000000000000000ul, false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x0E218820u, 0x7F800000FF800000ul, 0x0000000000000000ul, false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x0E218820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x0E218820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frintn_V(uint Opcode, ulong A, ulong B, bool DefaultNaN, ulong Result0, ulong Result1)
        {
            Vector128<float> V1 = MakeVectorE0E1(A, B);

            int FpcrTemp = 0x0;
            if(DefaultNaN)
            {
                FpcrTemp = 0x2000000;
            }

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(Result1));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0x3FE66666u, false, 0x40000000u)]
        [TestCase(0x3F99999Au, false, 0x40000000u)]
        [TestCase(0x404CCCCDu, false, 0x40800000u)]
        [TestCase(0x40733333u, false, 0x40800000u)]
        [TestCase(0x3FC00000u, false, 0x40000000u)]
        [TestCase(0x40200000u, false, 0x40400000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        public void Frintp_S(uint A, bool DefaultNaN, uint Result)
        {
            uint Opcode = 0x1E24C020; // FRINTP S0, S1

            Vector128<float> V1 = MakeVectorE0(A);

            int FpcrTemp = 0x0;
            if(DefaultNaN)
            {
                FpcrTemp = 0x2000000;
            }

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

            Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x4EE18820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, false, 0x4000000000000000ul, 0x4000000000000000ul)] // FRINTP V0.2D, v1.2D
        [TestCase(0x4EE18820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x4EA18820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x4000000040000000ul, 0x4000000040000000ul)] // FRINTP V0.4S, v1.4S
        [TestCase(0x0EA18820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x4000000040000000ul, 0x0000000000000000ul)] // FRINTP V0.2S, v1.2S
        [TestCase(0x0EA18820u, 0x0000000080000000ul, 0x0000000000000000ul, false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x0EA18820u, 0x7F800000FF800000ul, 0x0000000000000000ul, false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x0EA18820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x0EA18820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frintp_V(uint Opcode, ulong A, ulong B, bool DefaultNaN, ulong Result0, ulong Result1)
        {
            Vector128<float> V1 = MakeVectorE0E1(A, B);

            int FpcrTemp = 0x0;
            if(DefaultNaN)
            {
                FpcrTemp = 0x2000000;
            }

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(Result1));
            });

            CompareAgainstUnicorn();
        }

    	[TestCase(0x3FE66666u, 'N', false, 0x40000000u)]
    	[TestCase(0x3F99999Au, 'N', false, 0x3F800000u)]
    	[TestCase(0x404CCCCDu, 'P', false, 0x40800000u)]
    	[TestCase(0x40733333u, 'P', false, 0x40800000u)]
    	[TestCase(0x404CCCCDu, 'M', false, 0x40400000u)]
    	[TestCase(0x40733333u, 'M', false, 0x40400000u)]
    	[TestCase(0x3F99999Au, 'Z', false, 0x3F800000u)]
    	[TestCase(0x3FE66666u, 'Z', false, 0x3F800000u)]
    	[TestCase(0x00000000u, 'N', false, 0x00000000u)]
    	[TestCase(0x00000000u, 'P', false, 0x00000000u)]
    	[TestCase(0x00000000u, 'M', false, 0x00000000u)]
    	[TestCase(0x00000000u, 'Z', false, 0x00000000u)]
    	[TestCase(0x80000000u, 'N', false, 0x80000000u)]
    	[TestCase(0x80000000u, 'P', false, 0x80000000u)]
    	[TestCase(0x80000000u, 'M', false, 0x80000000u)]
    	[TestCase(0x80000000u, 'Z', false, 0x80000000u)]
    	[TestCase(0x7F800000u, 'N', false, 0x7F800000u)]
    	[TestCase(0x7F800000u, 'P', false, 0x7F800000u)]
    	[TestCase(0x7F800000u, 'M', false, 0x7F800000u)]
    	[TestCase(0x7F800000u, 'Z', false, 0x7F800000u)]
    	[TestCase(0xFF800000u, 'N', false, 0xFF800000u)]
    	[TestCase(0xFF800000u, 'P', false, 0xFF800000u)]
    	[TestCase(0xFF800000u, 'M', false, 0xFF800000u)]
    	[TestCase(0xFF800000u, 'Z', false, 0xFF800000u)]
    	[TestCase(0xFF800001u, 'N', false, 0xFFC00001u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'P', false, 0xFFC00001u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'M', false, 0xFFC00001u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'Z', false, 0xFFC00001u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'N', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'P', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'M', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'Z', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'N', false, 0x7FC00002u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'P', false, 0x7FC00002u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'M', false, 0x7FC00002u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'Z', false, 0x7FC00002u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'N', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'P', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'M', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'Z', true,  0x7FC00000u, Ignore = "NaN test.")]
    	public void Frintx_S(uint A, char RoundType, bool DefaultNaN, uint Result)
    	{
            uint Opcode = 0x1E274020; // FRINTX S0, S1

            Vector128<float> V1 = MakeVectorE0(A);

        	int FpcrTemp = 0x0;
        	switch(RoundType)
        	{
        		case 'N': FpcrTemp = 0x0;      break;
        		case 'P': FpcrTemp = 0x400000; break;
        		case 'M': FpcrTemp = 0x800000; break;
        		case 'Z': FpcrTemp = 0xC00000; break;
        	}
        	if(DefaultNaN)
        	{
        		FpcrTemp |= 1 << 25;
        	}

        	AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

        	Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x6E619820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'N', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)] // FRINTX V0.2D, V1.2D
        [TestCase(0x6E619820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'N', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6E619820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'P', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6E619820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'P', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6E619820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'M', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6E619820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'M', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6E619820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'Z', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6E619820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'Z', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'N', false, 0x3F80000040000000ul, 0x3F80000040000000ul)] // FRINTX V0.4S, V1.4S
        [TestCase(0x6E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'P', false, 0x4000000040000000ul, 0x4000000040000000ul)]
        [TestCase(0x6E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'M', false, 0x3F8000003F800000ul, 0x3F8000003F800000ul)]
        [TestCase(0x6E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'Z', false, 0x3F8000003F800000ul, 0x3F8000003F800000ul)]
        [TestCase(0x2E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'N', false, 0x3F80000040000000ul, 0x0000000000000000ul)] // FRINTX V0.2S, V1.2S
        [TestCase(0x2E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'P', false, 0x4000000040000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'M', false, 0x3F8000003F800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'Z', false, 0x3F8000003F800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x0000000080000000ul, 0x0000000000000000ul, 'N', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x0000000080000000ul, 0x0000000000000000ul, 'P', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x0000000080000000ul, 0x0000000000000000ul, 'M', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x0000000080000000ul, 0x0000000000000000ul, 'Z', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'N', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'P', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'M', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'Z', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'N', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'P', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'M', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'Z', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'N', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'P', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'M', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'Z', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frintx_V(uint Opcode, ulong A, ulong B, char RoundType, bool DefaultNaN, ulong Result0, ulong Result1)
        {
            Vector128<float> V1 = MakeVectorE0E1(A, B);

            int FpcrTemp = 0x0;
            switch(RoundType)
            {
                case 'N': FpcrTemp = 0x0;      break;
                case 'P': FpcrTemp = 0x400000; break;
                case 'M': FpcrTemp = 0x800000; break;
                case 'Z': FpcrTemp = 0xC00000; break;
            }
            if(DefaultNaN)
            {
                FpcrTemp |= 1 << 25;
            }

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, Fpcr: FpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(Result1));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0x41200000u, 0x3EA18000u)]
        public void Frsqrte_S(uint A, uint Result)
        {
            uint Opcode = 0x7EA1D820; // FRSQRTE S0, S1

            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1);

            Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(Result));

            CompareAgainstUnicorn();
        }
    }
}
