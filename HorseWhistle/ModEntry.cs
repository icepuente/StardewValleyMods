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
        private TileData[] Tiles;
        private bool GridActive;
        private ISoundBank CustomSoundBank;
        private WaveBank CustomWaveBank;
        private bool HasAudio;
        private ModConfigModel Config;


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfigModel>();

            if (Constants.TargetPlatform == GamePlatform.Windows)
            {
                try
                {
                    this.CustomSoundBank = new SoundBankWrapper(new SoundBank(Game1.audioEngine, Path.Combine(helper.DirectoryPath, "assets", "CustomSoundBank.xsb")));
                    this.CustomWaveBank = new WaveBank(Game1.audioEngine, Path.Combine(helper.DirectoryPath, "assets", "CustomWaveBank.xwb"));
                    this.HasAudio = true;
                }
                catch (ArgumentException ex)
                {
                    this.CustomSoundBank = null;
                    this.CustomWaveBank = null;
                    this.HasAudio = false;

                    this.Monitor.Log("Couldn't load audio, so the whistle sound won't play.");
                    this.Monitor.Log(ex.ToString(), LogLevel.Trace);
                }
            }

            // add all event listener methods
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            if (this.Config.EnableGrid)
            {
                GameEvents.SecondUpdateTick += (sender, e) => this.UpdateGrid();
                GraphicsEvents.OnPostRenderEvent += (sender, e) => this.DrawGrid(Game1.spriteBatch);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (e.Button == this.Config.TeleportHorseKey)
            {
                Horse horse = this.FindHorse();
                if (horse != null)
                {
                    this.PlayHorseWhistle();
                    Game1.warpCharacter(horse, Game1.currentLocation, Game1.player.getTileLocation());
                }
            }
            else if (this.Config.EnableGrid && e.Button == this.Config.EnableGridKey)
                this.GridActive = !this.GridActive;
        }

        /// <summary>Play the horse whistle sound.</summary>
        private void PlayHorseWhistle()
        {
            if (!this.HasAudio)
                return;

            ISoundBank originalSoundBank = Game1.soundBank;
            WaveBank originalWaveBank = Game1.waveBank;
            try
            {
                Game1.soundBank = this.CustomSoundBank;
                Game1.waveBank = this.CustomWaveBank;
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

        /// <summary>Find the current player's horse.</summary>
        private Horse FindHorse()
        {
            foreach (Stable stable in this.GetStables())
            {
                if (Context.IsMultiplayer && stable.owner.Value != Game1.player.UniqueMultiplayerID)
                    continue;

                Horse horse = Utility.findHorse(stable.HorseId);
                if (horse == null || horse.rider != null)
                    continue;

                return horse;
            }

            return null;
        }

        /// <summary>Get all stables in the game.</summary>
        private IEnumerable<Stable> GetStables()
        {
            foreach (BuildableGameLocation location in Game1.locations.OfType<BuildableGameLocation>())
            {
                foreach (Stable stable in location.buildings.OfType<Stable>())
                {
                    if (stable.GetType().FullName?.Contains("TractorMod") == true)
                        continue; // ignore tractor
                    yield return stable;
                }
            }
        }

        private void UpdateGrid()
        {
            if (!this.GridActive || !Context.IsPlayerFree || Game1.currentLocation == null)
            {
                this.Tiles = null;
                return;
            }

            // get updated tiles
            GameLocation location = Game1.currentLocation;
            this.Tiles = CommonMethods
                .GetVisibleTiles(location, Game1.viewport)
                .Where(tile => location.isTileLocationTotallyClearAndPlaceableIgnoreFloors(tile))
                .Select(tile => new TileData(tile, Color.Red))
                .ToArray();
        }

        private void DrawGrid(SpriteBatch spriteBatch)
        {
            if (!this.GridActive || !Context.IsPlayerFree || Tiles == null || Tiles.Length == 0)
                return;

            // draw tile overlay
            int tileSize = Game1.tileSize;
            foreach (TileData tile in Tiles.ToArray())
            {
                Vector2 position = tile.TilePosition * tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);
                RectangleSprite.DrawRectangle(spriteBatch, new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize), tile.Color * .3f, 6);
            }
        }
    }
}
