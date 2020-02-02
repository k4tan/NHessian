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

TODO: Attach benchmarks

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

### CharBuffer Cache

Strings under a certain length (currently hard coded as 150 characters) are cached and de-duplified.
This works as follows:
- string is read from stream and stored as char-array (no allocations)
- char array is used directly as key into the cache
  - if cache hit: return cached String
  - if cache miss: create a String (allocation) and put it into the cache

This effectivly de-duplifies strings and therefore reduces allocations.

In scenarios where strings repeat often, this has a massive effect on memory allocations.

### `unsafe` code

This library contains 3 unsafe code sections in `HessianStreamReader`.
1. UTF-8 parser (`ReadStringUnsafe`)
2. `CharBufferEqualityComparer.Equals`
3. `CharBufferEqualityComparer.GetHashCode`

Using unsafe code speeds up utf-8 parsing and char comparison significantly. 

It might be worth exploring "safe" alternatives in the future.


## Test-Server
NHessian includes a set of integration tests targeting a server hosted on Heruko.

The project can be found here: https://github.com/k4tan/NHessian-TestServer

## Missing

- Server library
- Method [overloading](http://hessian.caucho.com/doc/hessian-1.0-spec.xtp#MethodsandOverloading)
- Support for [remote](http://hessian.caucho.com/doc/hessian-1.0-spec.xtp#remote)