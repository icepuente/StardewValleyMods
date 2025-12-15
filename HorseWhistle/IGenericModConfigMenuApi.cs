using System;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace HorseWhistle
{
    /// <summary>The API for Generic Mod Config Menu.</summary>
    public interface IGenericModConfigMenuApi
    {
        /// <summary>Register a mod whose config can be edited through the UI.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="reset">Reset the config to its default values.</param>
        /// <param name="save">Save the config to disk.</param>
        /// <param name="titleScreenOnly">Whether the options can only be edited from the title screen.</param>
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        /// <summary>Add a section title at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="text">The title text.</param>
        /// <param name="tooltip">The tooltip text, or null for none.</param>
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string>? tooltip = null);

        /// <summary>Add a boolean option at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value.</param>
        /// <param name="setValue">Set a new value.</param>
        /// <param name="name">The label text.</param>
        /// <param name="tooltip">The tooltip text, or null for none.</param>
        /// <param name="fieldId">The unique field ID, or null to auto-generate.</param>
        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);

        /// <summary>Add a keybind option at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value.</param>
        /// <param name="setValue">Set a new value.</param>
        /// <param name="name">The label text.</param>
        /// <param name="tooltip">The tooltip text, or null for none.</param>
        /// <param name="fieldId">The unique field ID, or null to auto-generate.</param>
        void AddKeybind(IManifest mod, Func<SButton> getValue, Action<SButton> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);

        /// <summary>Add a keybind list option at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value.</param>
        /// <param name="setValue">Set a new value.</param>
        /// <param name="name">The label text.</param>
        /// <param name="tooltip">The tooltip text, or null for none.</param>
        /// <param name="fieldId">The unique field ID, or null to auto-generate.</param>
        void AddKeybindList(IManifest mod, Func<KeybindList> getValue, Action<KeybindList> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);
    }
}
