using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using XRectangle = xTile.Dimensions.Rectangle;

namespace HorseWhistle
{
    internal static class CommonMethods
    {
        public static IEnumerable<Vector2> GetVisibleTiles(GameLocation currentLocation, XRectangle viewport)
        {
            int tileSize = Game1.tileSize;
            int left = viewport.X / tileSize;
            int top = viewport.Y / tileSize;
            int right = (int)Math.Ceiling((viewport.X + viewport.Width) / (decimal)tileSize);
            int bottom = (int)Math.Ceiling((viewport.Y + viewport.Height) / (decimal)tileSize);

            for (int x = left; x < right; x++)
            {
                for (int y = top; y < bottom; y++)
                {
                    if (currentLocation.isTileOnMap(x, y))
                        yield return new Vector2(x, y);
                }
            }
        }
    }
}
