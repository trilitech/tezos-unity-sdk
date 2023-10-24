# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [2.0.7] - 2023-10-24
### Added
- Configurable parameters (`networkType`, `rpcUrl`, `dAppMetadata` and `logLevel`) for Tezos singleton instance.  

### Changed
- Default RPC node to tezos.marigold.dev

## [2.0.6] - 2023-10-16
### Changed
- Disabled `Kukai Embed` silent signing
- Updated `Kukai Embed` to `0.8.9`
- Updated `@airgap/beacon-sdk` to `4.0.12`

### Added
- Persistent auth on WebGL platform


## [2.0.5] - 2023-08-12
### Changed
- Unity readme


## [2.0.4] - 2023-08-11
### Fixed
- Asset store validator tool warnings for WebGLCopyAndPaste feature

### Added
- .unitypackage release with GH actions


## [2.0.3] - 2023-08-01
### Changed
- Mint button in inventory rolled back to old behaviour that uses contracts on view
- Added Mint FA2 Token button on welcome screen
- Removed upload file feature usage from DemoExample sample


## [2.0.2] - 2023-07-28
### Fixed
- WebGL `GetActiveAccountAddress` function.


## [2.0.1] - 2023-07-28
### Fixed
- NftApi sample bug with incorrect key error.


## [2.0.0] - 2023-07-11
### Fixed
- [Bug](https://github.com/trilitech/tezos-unity-sdk/issues/102) with standalone IL2CPP build fails
- [Bug](https://github.com/trilitech/tezos-unity-sdk/issues/103) with closing Beacon database connections before app exit

### Added
- FA2 token contract (deploy, mint and transfer features)
- Implemented IPFS and on-chain image uploaders for WebGL and Editor platforms

### Changed
- Issue Report Form menu item path changed (new path: `Tools/Tezos SDK for Unity/Report an Issue`)
- ITezosAPI renamed to ITezos, refactored and divided in Wallet and API parts
- Missing namespaces


## [1.5.1] - 2023-06-27
### Fixed
- [Bug](https://github.com/trilitech/tezos-unity-sdk/issues/99) with game items transfer not working

### Added
- Ipfs web uploader
- Ipfs editor uploader

## [1.5.0] - 2023-06-22
### Fixed
- [Bug](https://github.com/trilitech/tezos-unity-sdk/issues/68) with UnityBeacon gameobject
- [Bug](https://github.com/trilitech/tezos-unity-sdk/issues/61) fix contracts compilation on Ligo `0.65.0`
- [Bug](https://github.com/trilitech/tezos-unity-sdk/issues/88) Marketplace is not getting loaded on WebGL
- [Bug](https://github.com/trilitech/tezos-unity-sdk/issues/89) with incorrect Micheline params serialization on WebGL
- [Bug](https://github.com/trilitech/tezos-unity-sdk/issues/47) with sharing violation on `beacon.db`

### Added
- Kukai embed login option for WebGL builds
- Ability to build WebGL frontend bundle with Webpack, refactored WebGL fronted app structure
- API method: `GetLatestBlockLevel`
- Editor Issue Report Form window
- Readme section about mobile-platforms building with disabled `Managed Stripping Level`

### Changed
- [Updated](https://github.com/trilitech/tezos-unity-sdk/issues/70) Beacon TypeScript dependency
- Refactored namespaces, root namespace now is `TezosSDK`


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


[unreleased]: https://github.com/trilitech/tezos-unity-sdk/compare/2.0.7...HEAD
[2.0.7]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/2.0.7
[2.0.6]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/2.0.6
[2.0.5]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/2.0.5
[2.0.4]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/2.0.4
[2.0.3]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/2.0.3
[2.0.2]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/2.0.2
[2.0.1]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/2.0.1
[2.0.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/2.0.0
[1.5.1]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.5.1
[1.5.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.5.0
[1.4.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.4.0
[1.3.1]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.3.1
[1.3.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.3.0
[1.2.1]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.2.1
[1.2.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.2.0
[1.1.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.1.0
[0.0.2]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/0.0.2
[0.0.1]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/0.0.1
