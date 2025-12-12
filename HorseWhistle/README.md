# Horse Whistle

A [Stardew Valley](https://www.stardewvalley.net/) mod that lets you summon your horse to your current location with the press of a button.

## Features

- **Summon Your Horse**: Press `V` (default) to call your horse to any outdoor location
- **Whistle Sound Effect**: Plays an audible whistle sound when summoning (Windows only)
- **Multiplayer Support**: Works in multiplayer - farmhands can request the host's horse
- **Debug Grid**: Optional tile grid overlay to visualize walkable tiles (toggle with `G`)
- **Tractor Mod Compatible**: Automatically ignores tractors from the Tractor Mod

## Requirements

- [Stardew Valley](https://www.stardewvalley.net/) 1.6.0 or later
- [SMAPI](https://smapi.io/) 4.0.0 or later

## Installation

1. Install [SMAPI](https://smapi.io/)
2. Download the latest release from [Nexus Mods](https://www.nexusmods.com/stardewvalley/mods/1131)
3. Extract the `HorseWhistle` folder into your `Stardew Valley/Mods` directory
4. Launch the game through SMAPI

## Configuration

After running the game once with the mod installed, a `config.json` file will be created in the `HorseWhistle` mod folder. You can edit this file to customize the mod:

```json
{
  "EnableGrid": false,
  "EnableWhistleAudio": true,
  "EnableGridKey": "G",
  "TeleportHorseKey": "V"
}
```

### Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| `TeleportHorseKey` | `V` | The key to press to summon your horse |
| `EnableWhistleAudio` | `true` | Whether to play the whistle sound effect (Windows only) |
| `EnableGrid` | `false` | Enable the debug tile grid feature |
| `EnableGridKey` | `G` | The key to toggle the debug tile grid |

### Keybinds

You can use any [SMAPI button code](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings) for the key settings. Examples:
- Keyboard: `V`, `G`, `LeftShift`, `Space`
- Mouse: `MouseLeft`, `MouseRight`
- Controller: `ControllerA`, `ControllerB`

## How It Works

When you press the summon key:

1. The whistle sound plays (if enabled and on Windows)
2. The mod searches all accessible locations for an available horse
3. If found, the horse is warped to your current tile position

### Restrictions

The horse will **not** be summoned if:
- You are currently riding a horse
- You are in the mines or skull cavern
- You are in a location the mod cannot access
- No horse is available (already being ridden or doesn't exist)

## Multiplayer

In multiplayer, farmhands can use the whistle to request a horse from the host player. The host's game handles finding and warping an available horse to the farmhand's location.

## Compatibility

- **Tractor Mod**: Fully compatible - the mod ignores tractors when searching for horses
- **Other horse mods**: Should be compatible with most horse-related mods

## Troubleshooting

### No whistle sound
- The whistle sound only works on Windows
- Check that `EnableWhistleAudio` is set to `true` in config.json
- Check the SMAPI console for any audio loading errors

### Horse doesn't appear
- Make sure you own a horse (build a stable)
- Make sure the horse isn't already being ridden
- Make sure you're not in the mines or skull cavern
- Check the SMAPI console for any errors

## Building from Source

### Requirements
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- Stardew Valley installed

### Build
```bash
cd HorseWhistle
dotnet build
```

The mod will be automatically deployed to your Stardew Valley Mods folder.

## Links

- [Nexus Mods Page](https://www.nexusmods.com/stardewvalley/mods/1131)
- [Source Code](https://github.com/icepuente/StardewValleyMods)

## License

This mod is licensed under the [MIT License](../LICENSE).

## Credits

- **Author**: icepuente
- **Framework**: [SMAPI](https://smapi.io/) by Pathoschild
