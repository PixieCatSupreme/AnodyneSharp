using Microsoft.Xna.Framework;

namespace AnodyneSharp.UI.Text
{
    public class TextCharacter
    {
        public Rectangle? Crop { get; set; }

        public char? Character { get; private set; }
        public Vector2 Position;

        public TextCharacter(char? character, Vector2 position, Rectangle? crop)
        {
            Character = character;
            Position = position;
            Crop = crop;
        }

        public virtual Vector2 GetPosition()
        {
            return Position;
        }
    }
}
