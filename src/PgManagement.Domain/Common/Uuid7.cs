using System.Security.Cryptography;

namespace PgManagement.Domain.Common
{
    /// <summary>
    /// Pure C# RFC 9562 UUID v7 Generator compatible with .NET 8 / 9 / 10 / 11.
    /// Provides time-ordered GUIDs based on 48-bit millisecond epoch + random bits.
    /// </summary>
    public static class Uuid7
    {
        public static Guid NewGuid()
        {
            Span<byte> bytes = stackalloc byte[16];
            RandomNumberGenerator.Fill(bytes);

            long ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Set 48-bit timestamp into high 6 bytes (Big Endian)
            bytes[0] = (byte)((ms >> 40) & 0xFF);
            bytes[1] = (byte)((ms >> 32) & 0xFF);
            bytes[2] = (byte)((ms >> 24) & 0xFF);
            bytes[3] = (byte)((ms >> 16) & 0xFF);
            bytes[4] = (byte)((ms >> 8) & 0xFF);
            bytes[5] = (byte)(ms & 0xFF);

            // Version 7: high nibble of byte 6 = 0111 (0x70)
            bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70);

            // Variant 1: high 2 bits of byte 8 = 10 (0x80)
            bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

            return new Guid(bytes);
        }
    }
}
