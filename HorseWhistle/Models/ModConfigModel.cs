using StardewModdingAPI;

namespace HorseWhistle.Models
{
    internal class ModConfigModel
    {
        public bool EnableGrid { get; } = false;
        public bool EnableWhistleAudio { get; } = true;
        public SButton EnableGridKey { get; } = SButton.G;
        public SButton TeleportHorseKey { get; } = SButton.V;
    }
}