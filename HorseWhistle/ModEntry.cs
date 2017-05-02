using HorseWhistle.Common;
using HorseWhistle.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using System.IO;

namespace HorseWhistle
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        private static IModHelper helper;
        private static GameLocation[] Locations;
        private static TileData[] Tiles;
        private static bool IsLoaded => Game1.hasLoadedGame && ModEntry.Locations != null;
        private static bool GridActive = false;
        private static SoundBank OriginalSoundBank;
        private static WaveBank OriginalWaveBank;
        private static SoundBank CustomSoundBank;
        private static WaveBank CustomWaveBank;

        internal static ModConfigModel Config;

        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            ModEntry.helper = helper;
            Config = helper.ReadConfig<ModConfigModel>();

            CustomSoundBank = new SoundBank(Game1.audioEngine, Path.Combine(helper.DirectoryPath, "assets", "CustomSoundBank.xsb"));
            CustomWaveBank = new WaveBank(Game1.audioEngine, Path.Combine(helper.DirectoryPath, "assets", "CustomWaveBank.xwb"));

            // add all event listener methods
            ControlEvents.KeyPressed += ReceiveKeyPress;
            LocationEvents.LocationsChanged += LocationChangedEvent;
            GameEvents.SecondUpdateTick += (sender, e) => ReceiveUpdateTick();
            GraphicsEvents.OnPostRenderEvent += OnPostRenderEvent;
        }

        /// <summary>Update the mod's config.json file from the current <see cref="Config"/>.</summary>
        internal static void SaveConfig()
        {
            helper.WriteConfig(Config);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ReceiveKeyPress(object sender, EventArgsKeyPressed e)
        {
            Monitor.Log($"Player pressed {e.KeyPressed}.");
            if (!IsLoaded)
                return;

            if (e.KeyPressed.ToString() == Config.EnableGridKey)
            {
                GridActive = !GridActive;
            }
            if (e.KeyPressed.ToString() == Config.TeleportHorseKey)
            {
                NPC horse = Utility.findHorse();
                if (horse != null && Game1.currentLocation.IsOutdoors)
                {
                    if (OriginalSoundBank != null && OriginalWaveBank != null)
                    {
                        PlayHorseWhistle();
                    }                
                    Game1.warpCharacter(horse, Game1.currentLocation.Name, Game1.player.getLeftMostTileX(), true, true);
                }
            }
        }

        private void PlayHorseWhistle()
        {
            Game1.soundBank = CustomSoundBank;
            Game1.waveBank = CustomWaveBank;
            Game1.audioEngine.Update();
            Game1.playSound("horseWhistle");
            Game1.soundBank = OriginalSoundBank;
            Game1.waveBank = OriginalWaveBank;
            Game1.audioEngine.Update();
        }

        // <summary>The method called when the game finishes drawing components to the screen.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnPostRenderEvent(object sender, EventArgs e)
        {
            Draw(Game1.spriteBatch);
        }

        private void AfterLoadSaveEvent(object sender, EventArgs eventArgs)
        {
            Locations = CommonMethods.GetAllLocations().ToArray();
        }

        private void LocationChangedEvent(object sender, EventArgs eventArgs)
        {
            Locations = CommonMethods.GetAllLocations().ToArray();
        }

        private void ReceiveUpdateTick()
        {
            if (Game1.currentLocation == null)
            {
                Tiles = new TileData[0];
                return;
            }

            if (OriginalSoundBank == null && Game1.soundBank != null)
                OriginalSoundBank = Game1.soundBank;
            if (OriginalWaveBank == null && Game1.waveBank != null)
                OriginalWaveBank = Game1.waveBank;

            // get updated tiles
            GameLocation location = Game1.currentLocation;
            Tiles = Update(location, CommonMethods.GetVisibleTiles(location, Game1.viewport)).ToArray();
        }

        private void Draw(SpriteBatch spriteBatch)
        {
            if (Tiles == null || Tiles.Length == 0 || !GridActive)
                return;

            // draw tile overlay
            int tileSize = Game1.tileSize;
            foreach (TileData tile in Tiles.ToArray())
            {
                Vector2 position = tile.TilePosition * tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);
                RectangleSprite.DrawRectangle(spriteBatch, new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y, tileSize, tileSize), tile.Color * .3f, 6);
            }
        }

        private IEnumerable<TileData> Update(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                bool isPassable = location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport);

                if (location.isTileLocationTotallyClearAndPlaceableIgnoreFloors(tile))
                {
                    yield return new TileData(tile, Color.Red);
                } 
            }
        }
    }
}
