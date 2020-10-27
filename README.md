# GameEngine

GameEngine is a web API that serves games and challenges and also provides grading for the [Gameboard](https://github.com/cmu-sei/gameboard) platform.

## Dependencies

The GameEngine requires the .NET Core 2.2 framework.

## Getting Started

1. Install .Net Core SDK 2.2
2. Start the application using the following command: `dotnet run`
3. Browse to `http://localhost:5001/`

## Development `appsettings`

.NET Core supports multiple `appsettings` files that are read based on the environment. Currently only the `Development`environment is used. This means that you can make a file named `appsettings.Development.json` in the same directory as `appsettings.json` and any settings in `appsettings.Development.json` will override the settings given in
`appsettings.json`. Most importantly, it means you won't have any issues if the settings file changes and you want the latest code.

## Storage

All data used by the application is read from a collection of configuration files.

Many of the defalt paths where data is assumed to be can be found in the Options class located here: `\gameengine\src\GameEngine.Api\Options.cs`

By default a `_data` folder should be created under the `\gameengine\src\GameEngine.Api\` directory. This folder will contain additional folders such as `games`, `problems`, `_iso`, and `challenges`.
	
## Configuration Settings

The API has a few configuration options that can be set in the `appsettings.json` file located in the root of the GameEngine.Api application: `\gameengine\src\GameEngine.Api\`

ClientKeys is an array of overloaded api keys in the format `name#random;callbackurl`. For example:  `solo#f1229d4c9772eab8272012aff161ab73e43d;https://solo.yourappurl.us/api/engine/`.  

Engine represents the Models/Options.cs object.

TopoMojo is the Url and api-key for accessing it; you'd get both from the topomojo admin.

    "AllowedHosts": "*",
    "ClientKeys": ["solo#f1229d4c9772eab8272012aff161ab73e43d;https://solo.yourappurl.us/api/engine/"],
    "TopoMojo": {
      "Url": "https://yourtopomojourl.us/api/engine/",
      "Key": "dev#test",
      "MaxRetries": 0
    }

## Change Port

By default, GameEngine API listens on port 5001. In case you want to change this, open the file `Properties/launchSettings.json` and find the line:

`"applicationUrl": "http://localhost:5001",`
    
...and change the port in this line.
