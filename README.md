# NHessian

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://github.com/k4tan/NHessian/blob/master/LICENSE)
[![#](https://img.shields.io/nuget/v/NHessian)](https://www.nuget.org/packages/NHessian/)
[![Build Status](https://dev.azure.com/kataan83/NHessian/_apis/build/status/NHessian-CI?branchName=master)](https://dev.azure.com/kataan83/NHessian/_build/latest?definitionId=1)
[![Test Status](https://img.shields.io/azure-devops/tests/kataan83/NHessian/1)](https://dev.azure.com/kataan83/NHessian/_build/latest?definitionId=1)
[![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/kataan83/NHessian/1)](https://dev.azure.com/kataan83/NHessian/_build/latest?definitionId=1&view=codecoverage-tab)

Fast and efficient Hessian v1 and v2 client library.

## Table of Contents

- [NHessian](#nhessian)
  * [Table of Contents](#table-of-contents)
  * [Usage](#usage)
  * [Motivation](#motivation)
  * [Performance](#performance)
    + [Benchmarks](#benchmarks)
  * [Advanced Usages](#advanced-usages)
    + [Async support](#async-support)
    + [Custom type bindings](#custom-type-bindings)
    + [Error handling](#error-handling)
  * [Strings](#strings)
    + [CharBuffer Cache](#charbuffer-cache)
    + [`unsafe` code](#-unsafe--code)
  * [Test-Server](#test-server)
  * [Missing](#missing)

## Usage

```csharp
/*
 * public interface ITestService
 * {
 *     string hello();
 * }
 */

var service = new System.Net.Http.HttpClient()
    .HessianService<ITestService>(
        new Uri("https://nhessian-hessian-test.herokuapp.com/hessian/test"));

Console.WriteLine(service.hello());   // "Hello, World"
```

## Motivation

A project I am working on needed a fast and memory efficient hessian v2 client library to talk to a Java backend.

Existing .NET hessian libraries are a combination of "v1 only", "slow" or "memory ineffcient".

## Performance

As stated, the main motivation for creating this library was the inefficiency of existing .NET implementations.

Following is a comparison of three real world payloads deserialized with NHessian and [CZD.HessianCSharp](https://www.nuget.org/packages/CZD.HessianCSharp/).

[CZD.HessianCSharp](https://www.nuget.org/packages/CZD.HessianCSharp/) is the most downloaded hessian implementation 
 on nuget (and the one that worked best for me so far).  

### Benchmarks

The following benchmarks focus on deserialization for two reasons:
1. deserialization is a lot harder to implement efficiently
2. deserializing is presumably a lot more important for a client than serialization

Context:
- Payloads were taken from production and are Hessian v1 encoded data streams
- Measured is pure deserialization. Data is loaded into a `MemoryStream` pre-benchmark and deserialized straight from it.
- [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) was used for profiling.
- For the `NHessian v2` test, the data stream was converted into a v2 stream using NHessian.

Time | Memory Allocation
---------------|-------------------
![Time](./docs/benchmarks/execution_time.png) |![Memory Allocation](./docs/benchmarks/memory_allocation.png)



## Advanced Usages

### Async support

NHessian supports async execution out of the box. 
Simply use `Task` or `Task<T>` as the result type and the call is executed async.

```csharp
/*
 * public interface ITestService
 * {    
 *     Task<string> hello();
 * }
 */

var service = new System.Net.Http.HttpClient()
    .HessianService<ITestService>(
        new Uri("https://nhessian-hessian-test.herokuapp.com/hessian/test"));

Console.WriteLine(await service.hello())   // "Hello, World"
```

### Custom type bindings

Hessian doesn't really specify what remoted type strings look like.
Type strings usually refer to an actuall type name but they don't have to.

For example, java uses `[int` for int arrays (http://hessian.caucho.com/doc/hessian-java-binding-draft-spec.xtp).

The `TypeBindings` class and paramter allows it define custom bindings.

`JavaTypeBindings` are included by default and can be extended if required.

```csharp
/*
 * TypeBindings.Java  includes byndings for 
* "[int", "[long", "[boolean", "[double" and "[string"
 */

var service = new System.Net.Http.HttpClient()
    .HessianService<ITestService>(
        new Uri("https://nhessian-hessian-test.herokuapp.com/hessian/test"),
        TypeBindings.Java);

Console.WriteLine(await service.hello())   // "Hello, World"
```

### Error handling
Hessian specifies a set of [faults](http://hessian.caucho.com/doc/hessian-1.0-spec.xtp#Faults) that the server can report.

The most important one is `ServiceException` that indicates that the called method threw an exception. 

NHessian service proxy will throw the reported exception if:
- `unwrapServiceExceptions` is set to true (set by default)
- Backend remoted an exception that derives from `Exception`

If the above conditions do not apply and for any other fault type, NHessian will throw a `HessianRemoteException` providing relevant information.

## Strings

Strings are a major chalange during deserialization. 
Especially in v1 where the same type names are remoted over and over again.

In order to increase performance and memory usage, NHessian contains two optimizations.

### String Interning

Strings under a certain length (48 chars) and all hessian v1 type names are interned.

The interning works in the same way as the [Microsoft NameTable](https://docs.microsoft.com/en-us/dotnet/api/system.xml.nametable) class.
If the same set of characters has already been encountered, the previously created string is returned. 

### `unsafe` code

This library contains 3 unsafe code sections in `HessianStreamReader`.
1. `HessianStreamReader.ReadStringUnsafe`: UTF-8 parser
2. `StringInternPool.TextEqualsUnsafe`: Compare char[] with string
3. `StringInternPool.GetHashCodeUnsafe`: Calculate hashCode for char[]

Using unsafe code speeds up utf-8 parsing and char comparison significantly. 

It might be worth exploring "safe" alternatives in the future.


## Test-Server
NHessian includes a set of integration tests targeting a server hosted on Heruko.

The project can be found here: https://github.com/k4tan/NHessian-TestServer

## Missing

- Server library
- Method [overloading](http://hessian.caucho.com/doc/hessian-1.0-spec.xtp#MethodsandOverloading)
- Support for [remote](http://hessian.caucho.com/doc/hessian-1.0-spec.xtp#remote)