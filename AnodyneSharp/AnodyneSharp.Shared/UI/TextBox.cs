using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Input;
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
        public UILabel introPrompt;
        public bool BlinkyEnabled;


        public bool PauseWriting;

        private float blinky_box_timer;
        private bool _drawBlinky;


        private Texture2D _boxTexture;
        private Vector2 _blinkyPos;

        private Vector2 pos;

        private bool _usePrompt;
        private bool _isControllerMode;

        public TextBox(bool useMenuBox, bool usePrompt)
        {
            blinky_box_timer = blinky_box_timer_max;
            Set_box_position((!useMenuBox && GlobalState.DialogueTop) ? Touching.UP : Touching.DOWN);

            _boxTexture = ResourceManager.GetTexture(!useMenuBox ? "dialogue_box" : "menudialogue_box", true);
            blinky_box = ResourceManager.GetTexture("dialogue_blinky_box", true);

            _usePrompt = usePrompt;

            _isControllerMode = KeyInput.ControllerMode;
            BlinkyEnabled = true;
        }

        public void Update()
        {
            if (!PauseWriting)
            {
                Writer.Update();
            }

            if (BlinkyEnabled &&(PauseWriting || Writer.AtEndOfBox || Writer.AtEndOfText))
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

            if (_usePrompt && _isControllerMode != KeyInput.ControllerMode)
            {
                UpdatePrompt();
            }
        }

        public void DrawUI()
        {
            Writer.Draw();
            SpriteDrawer.DrawSprite(_boxTexture, pos, Z: DrawingUtilities.GetDrawingZ(DrawOrder.TEXTBOX));

            if (_drawBlinky)
            {
                if (_usePrompt)
                {
                    introPrompt.Draw();
                }
                else
                {
                    SpriteDrawer.DrawSprite(blinky_box, _blinkyPos, Z: DrawingUtilities.GetDrawingZ(DrawOrder.TEXT));
                }
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

            _blinkyPos = new Vector2(pos.X + Width - 10, pos.Y + Height - 10);


            introPrompt = new UILabel(new Vector2(pos.X + Width -4, pos.Y + Height - 14), false, "", new Color(254,33,33), DrawOrder.TEXT);

            UpdatePrompt();
        }

        private void UpdatePrompt()
        {
            introPrompt.SetText(DialogueManager.ReplaceKeys("[SOMEKEY-C]"));

            _isControllerMode = KeyInput.ControllerMode;

            if (!KeyInput.ControllerMode)
            {
                int width = introPrompt.Text.Length * Font.FontManager.GetCharacterWidth();

                introPrompt.Position = new Vector2(pos.X + Width - width -4, pos.Y + Height - 14);

            }
            else
            {
                introPrompt.Position = new Vector2(pos.X + Width - GameConstants.BUTTON_WIDTH -6, pos.Y + Height - GameConstants.BUTTON_HEIGHT - 1);
            }
        }
    }
}
