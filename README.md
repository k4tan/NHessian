# NHessian

[![#](https://img.shields.io/nuget/v/NHessian)](https://www.nuget.org/packages/NHessian/)
[![Build Status](https://dev.azure.com/kataan83/NHessian/_apis/build/status/NHessian-CI?branchName=master)](https://dev.azure.com/kataan83/NHessian/_build/latest?definitionId=1)
![Test Status](https://img.shields.io/azure-devops/tests/kataan83/NHessian/1)
[![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/kataan83/NHessian/1)](https://dev.azure.com/kataan83/NHessian/_build/latest?definitionId=1&view=codecoverage-tab)

Fast and efficient Hessian v1 and v2 client library.

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

## Missing

- Server library
- Method [overloading](http://hessian.caucho.com/doc/hessian-1.0-spec.xtp#MethodsandOverloading)
- Support for [remote](http://hessian.caucho.com/doc/hessian-1.0-spec.xtp#remote)