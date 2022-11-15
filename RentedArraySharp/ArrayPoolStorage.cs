using System.Buffers;
namespace RentedArraySharp
{

    /// <summary>
    /// Storage for array pools of required types and rent class for simpler resource control.
    /// </summary>
    public static class ArrayPoolStorage
    {
        public const int MaxArrayLength = 33554432;
        /// <summary>
        /// Parameter to how much similar size arrays will be close to fetch in created array pool.<br/>
        /// </summary>
        public const int MaxArraysPerBucket = 128;
        static readonly ArrayPool<byte> ByteArrayPool = ArrayPool<byte>.Create(MaxArrayLength, MaxArraysPerBucket);
        public static RentedArray<T> RentArray<T>(int length)
        where T : unmanaged
        {
            return new(length, ByteArrayPool);
        }
    }
}