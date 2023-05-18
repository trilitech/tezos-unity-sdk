# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [1.4.0] - 2023-05-18
### Fixed
- [Bug](https://github.com/trilitech/tezos-unity-sdk/issues/57) with BeaconConnectorWebGl
- [Bug](https://github.com/trilitech/tezos-unity-sdk/issues/63), updated Netezos to `v2.8.0`

### Added
- Better [coroutine error handling](https://github.com/trilitech/tezos-unity-sdk/issues/39)
- API methods: `GetOwnersForToken`, `GetOwnersForContract`, `IsHolderOfContract`, `GetTokenMetadata`, `GetContractMetadata`, `GetTokensForContract`, `GetOperationStatus`
- User ability to specify logging level
- Tezos config singleton class
- `IDataProviderConfig` and `TzKTProviderConfig` implementation
- Test runner with GH actions

### Changed
- Refactor: Tezos divided into two parts: Wallet and API


## [1.3.1] - 2023-04-27
### Changed
- Beacon SDK version to [1.0.23](https://github.com/baking-bad/beacon-dotnet-sdk/releases/tag/v1.0.23)


## [1.3.0] - 2023-04-25
### Added
- Custom unity readme

### Fixed
- Bug with unnecessary `RequestPermission()` call while clicking on `Deeplink connect` button on mobiles

### Changed
- Package structure
- SignPayload method: now users can pass payload as plaintext without hex-encoding
- Readme images


## [1.2.1] - 2023-04-17
### Fixed
- Bug with Newtonsoft reference


## [1.2.0] - 2023-04-17
### Added
- Authentication.prefab
- WebGl unitypackage plugin
- API single entrypoint


## [1.1.0] - 2023-03-31
### Added
- UPM-compatible Package
- CI to publish package to NPM registry
- Cross-Session-Persistence to authentication

### Fixed
- Bugs on sample project

### Changed
- Beacon SDK version


## [0.0.2] - 2023-02-28
### Fixed
- Fixed correct network type resolving in dotnet BeaconConnector

### Removed
- Removed redundant `.dll` binary files from beacon-dotnet-sdk plugins


## [0.0.1] - 2023-02-28
### Added
- The project has now a CHANGELOG
- Added auto releases with GH actions


[unreleased]: https://github.com/trilitech/tezos-unity-sdk/compare/1.4.0...HEAD
[1.4.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.4.0
[1.3.1]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.3.1
[1.3.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.3.0
[1.2.1]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.2.1
[1.2.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.2.0
[1.1.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.1.0
[0.0.2]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/0.0.2
[0.0.1]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/0.0.1
