## Unity iOS/Android plugin for the Greendeck SDK

1. Copy `GreendeckUnity.cs` and `SimpleJSON.cs` to your existing `Assets/Plugins/` directory in your Unity Project.

2. Open your Controller file attached to the main scene where you want to track events or show prices. start `using` the `Greendeck` and `SimpleJSON` namespaces.

3. For further instructions, take a look at our documentations <a href="http://www.greendeck.co/#/docs/guides/view/unity">here</a>.

### iOS Specific:

### Android Specific:
- Edit the `AndroidManifest.xml` file in `Assets/Plugins/Android` to add your Bundle Identifier (if applicable).

- Also, Please add in the permission for INTERNET in the manifest file.

