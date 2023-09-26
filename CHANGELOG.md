# Changelog

## 0.4.3

- fix: rash when unknown compact class has object child #11

## 0.4.2

- perf: use internal buffer in HessianStreamWriter #10

## 0.4.1

- fix: compact representations of an unknown class could not be deserialized

## 0.4.0

- all `unsafe` methods replaced with managed versions
- improve Unicode support (deal better with surrogate pairs)
- expose `HessianContent` and `ReadAsHessianAsync` for lower level control of hessian calls
- reduce allocations

## 0.3.1
- Fix DateTime serialization issues in Hessian V2. This issues effect the [Short DateTime](http://hessian.caucho.com/doc/hessian-serialization.html#anchor8) representation
  - Fallback to long representation for dates > 02:08:00 Jan 23, 6053 UTC (Int32 overflow)
  - Fix Int32 overflow for deserialized short dates close to Int32.Max

## 0.3.0
- Refactored string interning (https://github.com/k4tan/NHessian/pull/2)
  - simpler, faster and more memory efficient
- Refactored field de-/serialization (https://github.com/k4tan/NHessian/pull/4)
  - NonSerialized fields ignored per default
  - readonly fields ignored during de-serialization per default (caused exception before)
  - `TypeInformationProvider` allows for customization regarding serialized fields

## 0.2.0
- Fix: EndOfStream not correctly recognized
- Fix: enum values caused issues in some situations

## 0.1.0
Initial release

- v1 and v2 support
- rpc client 
- support for async calls (Task)