using Xunit;

using RentedArraySharp;
namespace RentedArraySharp.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        using var array = ArrayPoolStorage.RentArray<int>(100000);
        //....do stuff with array
    }
}