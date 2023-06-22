<div align="center">

<a href="https://tezos.com/unity/"><img height="200x" src="https://tezos.com/brand/NFTsTezos.png" /></a>

  <h1>Tezos Unity SDK</h1>

  <p>
    <strong>Tezos SDK for Unity</strong>
  </p>

  <p>
    <a href="https://opentezos.com/gaming/unity-sdk/"><img alt="Intro" src="https://img.shields.io/badge/docs-tutorials-blueviolet" /></a>
    <a href="https://github.com/trilitech/tezos-unity-sdk/issues"><img alt="Issues" src="https://img.shields.io/github/issues/trilitech/tezos-unity-sdk?color=blueviolet" /></a>
    <a href="https://opensource.org/licenses/MIT"><img alt="License" src="https://img.shields.io/github/license/trilitech/tezos-unity-sdk?color=blueviolet" /></a>
  </p>
</div>

The Tezos Unity SDK invites developers to discover the future of Web3 gaming with a complete kit that empowers game
developers with the ability to:

- Connect to a Tezos wallet
- Utilize data on the blockchain
- Call smart contracts
- True ownership of in-game assets

The Tezos SDK supports Desktop, Android, iOS, and browsers. Beyond allowing game developers to interact with the Tezos
blockchain, this SDK is a helpful resource for developing any Tezos decentralized application (dApp).

### Install from a Git URL

You can install the UPM package via directly Git URL. To load a package from a Git URL:

* Open [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui.html) window.
* Click the add **+** button in the status bar.
* The options for adding packages appear.
* Select Add package from git URL from the add menu. A text box and an Add button appear.
* Enter the `https://github.com/trilitech/tezos-unity-sdk.git` Git URL in the text box and click Add.
* You may also install a specific package version by using the URL with the specified version.
    * `https://github.com/trilitech/tezos-unity-sdk.git#X.Y.Z`
    * Please note that the version `X.Y.Z` stated here is to be replaced with the version you would like to get.
    * You can find all the available releases [here](https://github.com/trilitech/tezos-unity-sdk/releases).
    * The latest available release version
      is [![Last Release](https://img.shields.io/github/v/release/trilitech/tezos-unity-sdk)](https://github.com/trilitech/tezos-unity-sdk/releases/latest)

For more information about what protocols Unity supports, see [Git URLs](https://docs.unity3d.com/Manual/upm-git.html).

### Install from NPM

* Navigate to the `Packages` directory of your project.
* Adjust the [project manifest file](https://docs.unity3d.com/Manual/upm-manifestPrj.html) `manifest.json` in a text
  editor.
* Ensure `https://registry.npmjs.org/` is part of `scopedRegistries`.
    * Ensure `com.trilitech` is part of `scopes`.
    * Add `com.trilitech.tezos-unity-sdk` to the `dependencies`, stating the latest version.

A minimal example ends up looking like this. Please note that the version `X.Y.Z` stated here is to be replaced
with [the latest released version](https://www.npmjs.com/package/com.trilitech.tezos-unity-sdk), which is
currently [![NPM Package](https://img.shields.io/npm/v/com.trilitech.tezos-unity-sdk?color=blue)](https://www.npmjs.com/package/com.trilitech.tezos-unity-sdk).

```json
{
  "scopedRegistries": [
    {
      "name": "npmjs",
      "url": "https://registry.npmjs.org/",
      "scopes": [
        "com.trilitech"
      ]
    }
  ],
  "dependencies": {
    "com.trilitech.tezos-unity-sdk": "X.Y.Z",
    ...
  }
}
```

* Switch back to the Unity Editor and wait for it to finish importing the added package.

### WebGL Support

* Open Unity Editor.
* Navigate to `Packages` and find the `Tezos Unity SDK`.
* Go to `WebGLFrontend/output` and copy the `StreamingAssets` and `WebGLTemplates` folders.
* Navigate to the `Assets` folder of your project and paste copied folders.
* This action will create WebGL template folders for your project. Each template is a subfolder within the
  `WebGLTemplates` folder. Each template subfolder contains an `index.html` file along with any other resources the page
  needs, such as images or stylesheets. You can choose the appropriate template to use in the WebGL build in
  `Project settings/Player/Web tab/Resolution and Presentation`
* By default, unfortunately, Web builds didn't support copy-and-paste operations. To handle them properly, install with
  double-clicking `WebGLFrontend/output/WebGLCopyAndPaste.unitypackage`, this action will create `WebGLCopyAndPaste`
  alongside with `StreamingAssets` and `WebGLTemplates` folders inside your project Assets directory.


### Mobiles Support
If you are using SDK on mobile platforms (IOS or Android) you need to build your project with Disabled Managed Stripping
Level, otherwise you may encounter [assembly errors](https://github.com/trilitech/tezos-unity-sdk/issues/90).

To do this:
* Open Project settings.
* Navigate to Player tab.
* Chose platform-specific settings (Android or IOS).
* Make sure that `Managed Stripping Level` is disabled. (It's in optimization section)

### 📝 Read the [documentation.](https://opentezos.com/gaming/unity-sdk/)
