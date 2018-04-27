using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HorseWhistle.Common;
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

            try
            {
                CustomSoundBank = new SoundBankWrapper(new SoundBank(Game1.audioEngine, Path.Combine(helper.DirectoryPath, "assets", "CustomSoundBank.xsb")));
                CustomWaveBank = new WaveBank(Game1.audioEngine, Path.Combine(helper.DirectoryPath, "assets", "CustomWaveBank.xwb"));
                HasAudio = true;
            }
            catch (ArgumentException ex)
            {
                this.Monitor.Log("Couldn't load audio (this is normal on Linux/Mac). The mod will work fine without audio.");
                this.Monitor.Log(ex.ToString(), LogLevel.Trace);
            }

            // add all event listener methods
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            GameEvents.SecondUpdateTick += this.GameEvents_SecondUpdateTick;
            GraphicsEvents.OnPostRenderEvent += this.GraphicsEvents_OnPostRenderEvent;
        }

        /// <summary>Update the mod's config.json file from the current <see cref="Config"/>.</summary>
        internal void SaveConfig()
        {
            Helper.WriteConfig(Config);
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

            if (e.Button == Config.EnableGridKey)
                GridActive = !GridActive;
            else if (e.Button == Config.TeleportHorseKey)
            {
                Horse horse = this.FindHorse();
                if (horse != null)
                {
                    this.PlayHorseWhistle();
                    Game1.warpCharacter(horse, Game1.currentLocation, Game1.player.getTileLocation());
                }
            }
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

        // <summary>The method called when the game finishes drawing components to the screen.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void GraphicsEvents_OnPostRenderEvent(object sender, EventArgs e)
        {
            Draw(Game1.spriteBatch);
        }

        private void GameEvents_SecondUpdateTick(object sender, EventArgs e)
        {
            if (Game1.currentLocation == null)
            {
                Tiles = new TileData[0];
                return;
            }

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
                if (location.isTileLocationTotallyClearAndPlaceableIgnoreFloors(tile))
                    yield return new TileData(tile, Color.Red);
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
    }
}
