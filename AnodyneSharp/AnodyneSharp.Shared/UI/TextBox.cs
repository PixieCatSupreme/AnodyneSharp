using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Resources;
using AnodyneSharp.UI.Text;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI
{
    public class TextBox
    {
        private const int Width = 156;
        private const int Height = 44;

        public TextWriter Writer { get; set; }
        //Blinky

        private Texture2D _boxTexture;

        private Vector2 pos;

        public TextBox()
        {
            Set_box_position();

            _boxTexture = ResourceManager.GetTexture("dialogue_box");
        }

        public void  Update()
        {
            Writer.Update();
        }

        public void DrawUI()
        {
            Writer.Draw();
            SpriteDrawer.DrawGuiSprite(_boxTexture, pos, Z: DrawingUtilities.GetDrawingZ(DrawOrder.TEXTBOX));
        }

        private void Set_box_position(Touching direction = Touching.DOWN) 
		{
            float height = 44;


            if (direction ==  Touching.DOWN) {
                pos.Y = 180 - height;
                height -= 2;

            } else if (direction == Touching.UP) {
                pos.Y = 22;
			}

            pos.X = 2;

            Writer = new TextWriter
            {
                WriteArea = MathUtilities.CreateRectangle(pos.X + 4, pos.Y, Width, height)
            };


            //if (DH.isZH()) dialogue.y = dialogue_box.y + 2;

            //blinky_box.x = dialogue_box.x + dialogue_box.width - 10;
            //blinky_box.y = dialogue_box.y + dialogue_box.height - 10;
        }
    }
}
