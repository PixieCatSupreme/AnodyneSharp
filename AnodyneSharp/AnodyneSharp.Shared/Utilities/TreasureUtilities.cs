using static AnodyneSharp.Entities.Gadget.TreasureChest;

namespace AnodyneSharp.Utilities
{
    public static class TreasureUtilities
    {
        public static TreasureType GetTreasureType(int frame)
        {
            TreasureType treasureType;

            switch (frame)
            {
                case 0:
                    treasureType = TreasureType.BROOM;
                    break;
                case 4:
                    treasureType = TreasureType.WIDE;
                    break;
                case 5:
                    treasureType = TreasureType.LONG;
                    break;
                case 6:
                    treasureType = TreasureType.SWAP;
                    break;
                case 1:
                    treasureType = TreasureType.KEY;
                    break;
                case 2:
                    treasureType = TreasureType.GROWTH;
                    break;
                case 21:
                    treasureType = TreasureType.ARCHIPELAGO;
                    break;
                default:
                    if (frame >= 7 && frame <= 20)
                    {
                        treasureType = TreasureType.SECRET;
                    }
                    else
                    {
                        treasureType = TreasureType.NONE;
                    }
                    break;
            }

            return treasureType;
        }
    }
}
