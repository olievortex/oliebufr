using OlieBufr.Lib.Decoding;

namespace OlieBufr.Tests.Decoding
{
    public class IndicatorSectionTests
    {
        [Fact]
        public void Decode_ReturnsSection_WhenValidVersion3()
        {
            var data = new byte[] {
                (byte)'B', (byte)'U', (byte)'F', (byte)'R',
                0x00, 0x00, 0x10, // length = 16
                0x03 // version = 3
            };
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            var section = IndicatorSection.Decode(br);

            Assert.Equal("BUFR", section.Magic);
            Assert.Equal(0x10, section.Length);
            Assert.Equal(3, section.Version);
        }

        [Fact]
        public void Decode_ReturnsSection_WhenValidVersion4_WithNonZeroLength()
        {
            var data = new byte[]
            {
                (byte)'B', (byte)'U', (byte)'F', (byte)'R',
                0x01, 0x02, 0x03, // length = 0x010203
                0x04 // version = 4
            };

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            var section = IndicatorSection.Decode(br);

            Assert.Equal("BUFR", section.Magic);
            Assert.Equal(0x010203, section.Length);
            Assert.Equal(4, section.Version);
        }

        [Fact]
        public void Decode_ThrowsApplicationException_WhenInvalidMagic()
        {
            var data = new byte[]
            {
                (byte)'X', (byte)'X', (byte)'X', (byte)'X',
                0x00, 0x00, 0x01,
                0x03
            };

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            var ex = Assert.Throws<ApplicationException>(() => IndicatorSection.Decode(br));
            Assert.Equal("Not a BUFR file", ex.Message);
        }

        [Fact]
        public void Decode_ThrowsApplicationException_WhenVersionTooLow()
        {
            var data = new byte[]
            {
                (byte)'B', (byte)'U', (byte)'F', (byte)'R',
                0x00, 0x00, 0x05,
                0x02 // version too low
            };

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            var ex = Assert.Throws<ApplicationException>(() => IndicatorSection.Decode(br));
            Assert.Equal("Not a BUFR version 3 or 4 file", ex.Message);
        }

        [Fact]
        public void Decode_ThrowsApplicationException_WhenVersionTooHigh()
        {
            var data = new byte[]
            {
                (byte)'B', (byte)'U', (byte)'F', (byte)'R',
                0x00, 0x00, 0x05,
                0x05 // version too high
            };

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            var ex = Assert.Throws<ApplicationException>(() => IndicatorSection.Decode(br));
            Assert.Equal("Not a BUFR version 3 or 4 file", ex.Message);
        }

        [Fact]
        public void Decode_ReadsBigEndianInt24_MaxValue()
        {
            var data = new byte[]
            {
                (byte)'B', (byte)'U', (byte)'F', (byte)'R',
                0xFF, 0xFF, 0xFF, // length = 0xFFFFFF
                0x04
            };

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            var section = IndicatorSection.Decode(br);

            Assert.Equal("BUFR", section.Magic);
            Assert.Equal(0xFFFFFF, section.Length);
            Assert.Equal(4, section.Version);
        }
    }
}
