using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RentedArraySharp
{
    /// <summary>
    /// Object that holds byte <paramref name="Array"/> from byte
    /// <see cref="ArrayPool{}"/> and returns it back to pool when disposed.<br/>
    /// Will be disposed by <paramref name="GC"/> (in destructor) if not disposed manually. <br/>
    /// Treats given byte array as storage for any unmanaged type you need
    /// </summary>
    public unsafe class RentedArray<T> : IDisposable, IEnumerable<T>
    where T : unmanaged
    {
        /// <summary>
        /// How much elements T can be stored here
        /// </summary>
        public int Length { get; }
        public ref T this[int index]
        {
            get
            {
                if (index < Length && index >= 0)
                    return ref arrayPtr[index];
                throw new IndexOutOfRangeException();
            }
        }
        ArrayPool<byte> pool;
        GCHandle handle;
        T* arrayPtr;
        object lockObject;
        /// <summary>
        /// Underlying array. Be cautions when working with it. Don't return it to pool manually, but call <see cref="Dispose"/>.
        /// </summary>
        public readonly byte[] RawArray;

        /// <summary>
        /// Creates new instance of <see cref="RentedArray"/>
        /// </summary>
        /// <param name="length">Length of rented array. How many elements of type <paramref name="T"/> need to be stored.</param>
        /// <param name="pool">Pool which will be used to create and return array</param>
        public RentedArray(int length, ArrayPool<byte> pool)
        {
            //Here we calc required size for array to be rented
            int size = Unsafe.SizeOf<T>();
            RawArray = pool.Rent(size * length);
            //after we rented an array we need to pin it so GC won't move
            //it's position in VRAM
            handle = GCHandle.Alloc(RawArray, GCHandleType.Pinned);
            //after it pinned we can easily take a pointer to array first element
            //and index it like we do it in C/C++
            arrayPtr = (T*)Unsafe.AsPointer(ref RawArray[0]);
            //because array pool contains generally random arrays
            //it means we will get array with sufficient size but it will be filled with
            //random stuff, so here we clear our work area with default value
            Array.Fill(RawArray, (byte)0);
            Length = length;
            this.pool = pool;
            lockObject = new();
        }

        ~RentedArray()
        {
            Dispose();
        }
        bool returned = false;
        public void Dispose()
        {
            lock (lockObject)
            {
                if (returned) return;
                pool.Return(RawArray);
                //don't forget to free pinned object so GC can move it a bit,
                //do memory defragmentation and so on...
                handle.Free();
                arrayPtr = (T*)IntPtr.Zero;
                returned = true;
            }
        }
        /// <summary>
        /// Fills whole array with given value
        /// </summary>
        public void Fill(T value)
        {
            AsSpan(0, Length).Fill(value);
        }
        /// <summary>
        /// Method to return element at some index by <see langword="ref"/>
        /// </summary>
        public ref T At(int index)
        {
            if (index >= Length || index < 0) throw new IndexOutOfRangeException();
            return ref arrayPtr[index];
        }
        /// <summary>
        /// Method to get span of underlying array. Be caution, this method doesn't check
        /// span borders, so it is up to you.
        /// </summary>
        /// <param name="start">Start index of array you want to span</param>
        /// <param name="length">How many elements to span</param>
        /// <returns>Specified chunk of array memory as span </returns>
        public Span<T> AsSpan(int start, int length)
        {
            return new Span<T>(arrayPtr + start, length);
        }
        /// <summary>
        /// Gets ref value by index without bounds-checking.<br/>
        /// A bit faster. Use it when you sure about your index bounds.
        /// </summary>
        public ref T UnsafeAt(int index) => ref arrayPtr[index];

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
                yield return UnsafeAt(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return RawArray.GetEnumerator();
        }
    }
}