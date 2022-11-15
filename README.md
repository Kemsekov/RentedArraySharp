# Motivation
Sometimes we need to create and reuse a lot of large arrays of different types, which is inconvenient.

There is already a solution to this problem, like `ArrayPool`, but it has drawbacks:
1) It is required to return arrays manually to array pool, which is unsafe and can lead to bad memory utilization
2) It is not flexible in underlying memory utilization in such a way, that arrays of different types cannot be reused independently.

So by `RentedArray` you 
1) `RentedArray` is `IDisposable` and will return used array in 100% cases even if you didn't disposed it manually(will do by garbage collector)
2) `RentedArray` always uses `byte` arrays to map any unmanaged data type you use, so even `RentedArray` of different types can 
reuse the same memory.

# Sample

It's as simple as following:
```cs
void Foo()
{
  using var array = ArrayPoolStorage.RentArray<int>(100000);
  //....do stuff with array
}
```
