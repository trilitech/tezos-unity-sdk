# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [unreleased] - 2023-04-25
### Fixed
- Readme images

### Added
- Custom unity readme


## [unreleased] - 2023-04-19
### Fixed
- Bug with unnecessary `RequestPermission()` call while clicking on `Deeplink connect` button on mobiles
- Package structure
- Reworked SignPayload method: now users can pass payload as plaintext without hex-encoding;


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


[unreleased]: https://github.com/trilitech/tezos-unity-sdk/compare/1.2.1...HEAD
[1.2.1]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.2.1
[1.2.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.2.0
[1.1.0]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/1.1.0
[0.0.2]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/0.0.2
[0.0.1]: https://github.com/trilitech/tezos-unity-sdk/releases/tag/0.0.1
