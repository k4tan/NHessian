# Changelog

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