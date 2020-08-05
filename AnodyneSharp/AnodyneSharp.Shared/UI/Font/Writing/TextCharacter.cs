using Microsoft.Xna.Framework;

namespace AnodyneSharp.UI.Text
{
    public class TextCharacter
    {
        public Rectangle? Crop { get; set; }

        public char? Character { get; private set; }
        public float X;

        public TextCharacter(char? character, float x, Rectangle? crop)
        {
            Character = character;
            X = x;
            Crop = crop;
        }
    }
}
