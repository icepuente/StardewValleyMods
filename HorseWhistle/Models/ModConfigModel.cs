using StardewModdingAPI;

namespace HorseWhistle.Models
{
    class ModConfigModel
    {
        public bool EnableGrid { get; set; } = true;
        public SButton EnableGridKey { get; set; } = SButton.G;
        public SButton TeleportHorseKey { get; set; } = SButton.V;
    }
}
