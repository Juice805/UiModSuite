using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace UiModSuite.Utilities
{
	public static class SourceRects
	{
		public static readonly Rectangle springIcon = new Rectangle(406, 441, 12, 8);
		public static readonly Rectangle summerIcon = new Rectangle(406, 449, 12, 8);
		public static readonly Rectangle fallIcon = new Rectangle(406, 457, 12, 8);
		public static readonly Rectangle winterIcon = new Rectangle(406, 465, 12, 8);
		public static readonly Rectangle sunnyIcon = new Rectangle(452, 333, 13, 13);
		public static readonly Rectangle nightIcon = new Rectangle(465, 344, 13, 13);
		public static readonly Rectangle rainIcon = new Rectangle(465, 333, 13, 13);

		public static readonly Rectangle fishIcon = new Rectangle(20, 428, 10, 10);
		public static readonly Rectangle cropIcon = new Rectangle(10, 428, 10, 10);
		public static readonly Rectangle bundleIcon = new Rectangle(331, 374, 15, 14);
		public static readonly Rectangle healingIcon = new Rectangle(140, 428, 10, 10);
		public static readonly Rectangle energyIcon = new Rectangle(0, 438, 10, 10);
		public static readonly Rectangle currencyIcon = Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16);


	}
}
