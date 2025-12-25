using OlieBufr.Lib.Decoding;

namespace OlieBufr.Tests.Decoding;

public class DescriptorSectionTests
{
    private static byte[] BuildBuffer(int length, byte padding, ushort subsets, byte flags, byte[] descriptors)
    {
        // length is 3-byte big-endian
        var buffer = new byte[3 + 1 + 2 + 1 + (descriptors?.Length ?? 0)];
        buffer[0] = (byte)((length >> 16) & 0xFF);
        buffer[1] = (byte)((length >> 8) & 0xFF);
        buffer[2] = (byte)(length & 0xFF);
        buffer[3] = padding;
        buffer[4] = (byte)((subsets >> 8) & 0xFF);
        buffer[5] = (byte)(subsets & 0xFF);
        buffer[6] = flags;
        if (descriptors != null && descriptors.Length > 0)
        {
            Array.Copy(descriptors, 0, buffer, 7, descriptors.Length);
        }
        return buffer;
    }

    [Fact]
    public void Decode_ReturnsExpectedValues_WhenNoDescriptors_CompressionTrue()
    {
        var length = 7; // no descriptor bytes
        byte padding = 0x05;
        ushort subsets = 0x0002;
        byte flags = 0x40; // compression bit set
        var descriptors = Array.Empty<byte>();

        var buffer = BuildBuffer(length, padding, subsets, flags, descriptors);
        using var ms = new MemoryStream(buffer);
        using var br = new BinaryReader(ms);

        var result = DescriptorSection.Decode(br);

        Assert.Equal(length, result.Length);
        Assert.Equal(padding, result.Padding);
        Assert.Equal(subsets, result.Subsets);
        Assert.Equal(flags, result.Flags);
        Assert.True(result.IsCompression);
        Assert.Empty(result.Descriptors);
    }

    [Fact]
    public void Decode_ReturnsDescriptors_WhenPresent_CompressionFalse()
    {
        var descriptorBytes = new byte[] { 0x10, 0x20, 0x30 };
        var length = 7 + descriptorBytes.Length;
        byte padding = 0xFF;
        ushort subsets = 0x1234;
        byte flags = 0x00; // compression bit not set

        var buffer = BuildBuffer(length, padding, subsets, flags, descriptorBytes);
        using var ms = new MemoryStream(buffer);
        using var br = new BinaryReader(ms);

        var result = DescriptorSection.Decode(br);

        Assert.Equal(length, result.Length);
        Assert.Equal(padding, result.Padding);
        Assert.Equal(subsets, result.Subsets);
        Assert.Equal(flags, result.Flags);
        Assert.False(result.IsCompression);
        Assert.Equal(descriptorBytes, result.Descriptors);
    }

    [Fact]
    public void Decode_ThrowsEndOfStreamException_WhenNotEnoughDescriptorBytes()
    {
        // Declare length requires 4 descriptor bytes but provide only 2
        var declaredDescriptorsLength = 4;
        var actualDescriptors = new byte[] { 0xAA, 0xBB };
        var length = 7 + declaredDescriptorsLength;
        byte padding = 0x00;
        ushort subsets = 0x0001;
        byte flags = 0x00;

        // Build buffer that is too short (only supplies 2 descriptor bytes)
        var truncatedBuffer = new byte[3 + 1 + 2 + 1 + actualDescriptors.Length];
        var fullHeader = BuildBuffer(length, padding, subsets, flags, actualDescriptors);
        Array.Copy(fullHeader, 0, truncatedBuffer, 0, truncatedBuffer.Length);

        using var ms = new MemoryStream(truncatedBuffer);
        using var br = new BinaryReader(ms);

        Assert.Throws<EndOfStreamException>(() => DescriptorSection.Decode(br));
    }
}