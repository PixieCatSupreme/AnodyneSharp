using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.UI.Text;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.UI
{
    public class TextBox
    {
        private const int Width = 156;
        private const int Height = 44;
        private const float blinky_box_timer_max = 0.4f;

        public TextWriter Writer { get; set; }
        public Texture2D blinky_box;

        public bool PauseWriting;

        private float blinky_box_timer;
        private bool _drawBlinky;


        private Texture2D _boxTexture;
        private Vector2 _blinkyPos;

        private Vector2 pos;

        public TextBox(bool useMenuBox)
        {
            blinky_box_timer = blinky_box_timer_max;
            Set_box_position((!useMenuBox && GlobalState.DialogueTop) ? Touching.UP : Touching.DOWN);

            _boxTexture = ResourceManager.GetTexture(!useMenuBox ? "dialogue_box" : "menudialogue_box",true);
            blinky_box = ResourceManager.GetTexture("dialogue_blinky_box", true);
        }

        public void Update()
        {
            if (!PauseWriting)
            {
                Writer.Update();
            }

            if (PauseWriting || Writer.AtEndOfBox || Writer.AtEndOfText)
            {
                blinky_box_timer -= GameTimes.DeltaTime;
                if (blinky_box_timer < 0)
                {
                    blinky_box_timer = blinky_box_timer_max;
                    _drawBlinky = !_drawBlinky;
                }
            }
            else
            {
                _drawBlinky = false;
            }
        }

        public void DrawUI()
        {
            Writer.Draw();
            SpriteDrawer.DrawGuiSprite(_boxTexture, pos, Z: DrawingUtilities.GetDrawingZ(DrawOrder.TEXTBOX));

            if (_drawBlinky)
            {
                SpriteDrawer.DrawGuiSprite(blinky_box, _blinkyPos, Z: DrawingUtilities.GetDrawingZ(DrawOrder.TEXT));
            }

        }

        private void Set_box_position(Touching direction = Touching.DOWN)
        {
            int height = 44;


            if (direction == Touching.DOWN)
            {
                pos.Y = GameConstants.SCREEN_HEIGHT_IN_PIXELS - height - 2;
                height -= 2;
            }
            else if (direction == Touching.UP)
            {
                pos.Y = 2;
            }

            pos.Y += GameConstants.HEADER_HEIGHT;

            pos.X = 2;

            Writer = new TextWriter((int)pos.X + 4, (int)pos.Y + 5, Width - 16, height - 8);


            //if (DH.isZH()) dialogue.y = dialogue_box.y + 2;

            _blinkyPos = new Vector2( pos.X + Width - 10, pos.Y + Height - 10);
        }
    }
}
