using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HorseWhistle.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;

namespace HorseWhistle
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        private TileData[]? _tiles;
        private bool _gridActive;
        private ISoundBank? _customSoundBank;
        private WaveBank? _customWaveBank;
        private bool _hasAudio;
        private ModConfigModel _config = null!; // Initialized in Entry()


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            _config = helper.ReadConfig<ModConfigModel>();

            // set up sounds
            SetupAudio();

            // add event listeners
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Multiplayer.ModMessageReceived += ModMessageReceived;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += UpdateTicked;
            helper.Events.Display.Rendered += OnRendered;
        }

        /// <summary>Set up audio for the whistle sound.</summary>
        private void SetupAudio()
        {
            if (Constants.TargetPlatform == GamePlatform.Windows && _config.EnableWhistleAudio)
            {
                try
                {
                    // SDV 1.6+ uses MonoGame which requires relative paths from the game directory
                    string soundBankPath = Path.Combine("Mods", "HorseWhistle", "assets", "CustomSoundBank.xsb");
                    string waveBankPath = Path.Combine("Mods", "HorseWhistle", "assets", "CustomWaveBank.xwb");

                    _customSoundBank = new SoundBankWrapper(new SoundBank(Game1.audioEngine.Engine, soundBankPath));
                    _customWaveBank = new WaveBank(Game1.audioEngine.Engine, waveBankPath);
                    _hasAudio = true;
                }
                catch (Exception ex)
                {
                    _customSoundBank = null;
                    _customWaveBank = null;
                    _hasAudio = false;

                    Monitor.Log("Couldn't load audio, so the whistle sound won't play.");
                    Monitor.Log(ex.ToString());
                }
            }
        }

        /// <summary>Raised after the game is launched, right before the first update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // Get Generic Mod Config Menu API
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // Register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => _config = new ModConfigModel(),
                save: () => Helper.WriteConfig(_config)
            );

            // Add section title
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "General Settings"
            );

            // Add whistle keybind option
            configMenu.AddKeybind(
                mod: ModManifest,
                getValue: () => _config.TeleportHorseKey,
                setValue: value => _config.TeleportHorseKey = value,
                name: () => "Summon Horse Key",
                tooltip: () => "The key to press to summon your horse to your location."
            );

            // Add whistle audio option
            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.EnableWhistleAudio,
                setValue: value => _config.EnableWhistleAudio = value,
                name: () => "Enable Whistle Sound",
                tooltip: () => "Whether to play the whistle sound effect when summoning your horse. (Windows only)"
            );

            // Add debug section title
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Debug Options"
            );

            // Add grid option
            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.EnableGrid,
                setValue: value => _config.EnableGrid = value,
                name: () => "Enable Debug Grid",
                tooltip: () => "Enable the debug tile grid overlay feature."
            );

            // Add grid keybind option
            configMenu.AddKeybind(
                mod: ModManifest,
                getValue: () => _config.EnableGridKey,
                setValue: value => _config.EnableGridKey = value,
                name: () => "Toggle Grid Key",
                tooltip: () => "The key to press to toggle the debug tile grid overlay."
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree) return;

            if (e.Button == _config.TeleportHorseKey && !Game1.player.isRidingHorse() && !Game1.player.isAnimatingMount)
            {
                PlayHorseWhistle(); //play the whistle noise for the whistling player (even if the warp doesn't succeed)

                if (Context.IsMainPlayer)
                    WarpHorse();
                else //if the current player is a multiplayer farmhand
                    Helper.Multiplayer.SendMessage(true, "RequestHorse", new[] {ModManifest.UniqueID}); //request a horse from the host player
            }
            else if (_config.EnableGrid && e.Button == _config.EnableGridKey) _gridActive = !_gridActive;
        }

        /// <summary>Raised after the a mod message is received over the network.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (!Context.IsMainPlayer) return;

            //if a multiplayer farmhand sent a horse request, warp a horse to them
            if (e.Type == "RequestHorse")
            {
                Farmer? requester = Game1.GetPlayer(e.FromPlayerID);
                if (requester != null) WarpHorse(requester);
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void UpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(2)) UpdateGrid();
        }

        /// <summary>Raised after the game draws to the sprite patch in a draw tick, just before the final sprite batch is rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnRendered(object? sender, RenderedEventArgs e)
        {
            DrawGrid(e.SpriteBatch);
        }

        /// <summary>Play the horse whistle sound.</summary>
        private void PlayHorseWhistle()
        {
            if (!_hasAudio || !_config.EnableWhistleAudio) return;

            var originalSoundBank = Game1.soundBank;
            var originalWaveBank = Game1.waveBank;
            try
            {
                Game1.soundBank = _customSoundBank;
                Game1.waveBank = _customWaveBank;
                Game1.audioEngine.Update();
                Game1.playSound("horseWhistle");
            }
            finally
            {
                Game1.soundBank = originalSoundBank;
                Game1.waveBank = originalWaveBank;
                Game1.audioEngine.Update();
            }
        }

        /// <summary>Get all available locations.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            GameLocation[] mainLocations = (Context.IsMainPlayer ? Game1.locations : this.Helper.Multiplayer.GetActiveLocations()).ToArray();

            foreach (GameLocation location in mainLocations.Concat(MineShaft.activeMines))
            {
                yield return location;

                // In SDV 1.6+, all GameLocations have a buildings property (BuildableGameLocation was removed)
                foreach (Building building in location.buildings)
                {
                    if (building.indoors.Value != null)
                        yield return building.indoors.Value;
                }
            }
        }

        /// <summary>Find the current player's horse.</summary>
        private Horse? FindHorse()
        {
            foreach (GameLocation location in GetLocations())
            {
                foreach (Horse horse in location.characters.OfType<Horse>())
                {
                    if (horse.rider != null || horse.Name.StartsWith("tractor/")) continue;

                    return horse;
                }
            }

            return null;
        }

        /// <summary>Warps a horse to a player's location.</summary>
        /// <param name="player">The player to which the horse will warp. If null, this will default to the current player.</param>
        private void WarpHorse(Farmer? player = null)
        {
            //default to the current player
            if (player == null) player = Game1.player;

            //prevent warping to locations that might lose or delete the horse NPC
            if (player.currentLocation is MineShaft || !GetLocations().Contains(player.currentLocation)) return;

            //get a horse
            var horse = FindHorse();
            if (horse == null) return;

            //warp the horse to the target player
            Game1.warpCharacter(horse, player.currentLocation, player.Tile);
        }

        private void UpdateGrid()
        {
            if (!_config.EnableGrid || !_gridActive || !Context.IsPlayerFree || Game1.currentLocation == null)
            {
                _tiles = null;
                return;
            }

            // get updated tiles
            var location = Game1.currentLocation;
            _tiles = CommonMethods.GetVisibleTiles(location, Game1.viewport).Where(tile => location.isTilePassable(tile)).Select(tile => new TileData(tile, Color.Red)).ToArray();
        }

        private void DrawGrid(SpriteBatch spriteBatch)
        {
            if (!_config.EnableGrid || !_gridActive || !Context.IsPlayerFree || _tiles == null || _tiles.Length == 0) return;

            // draw tile overlay
            const int tileSize = Game1.tileSize;
            foreach (var tile in _tiles.ToArray())
            {
                var position = tile.TilePosition * tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);
                RectangleSprite.DrawRectangle(spriteBatch, new Rectangle((int) position.X, (int) position.Y, tileSize, tileSize), tile.Color * .3f, 6);
            }
        }
    }
}