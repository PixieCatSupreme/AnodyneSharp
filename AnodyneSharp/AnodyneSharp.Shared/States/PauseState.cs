using AnodyneSharp.Drawing;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States
{
    public class PauseState : State
    {
        public bool Exited { get; private set; }

        private Texture2D _bg;

        private UILabel _mapLabel;
        private UILabel _itemsLabel;
        private UILabel _cardsLabel;
        private UILabel _saveLabel;
        private UILabel _configLabel;
        private UILabel _secretsLabel;



        public PauseState()
        {
            _bg = ResourceManager.GetTexture("menu_bg");

            CreateLabels();
        }

        public override void Update()
        {
            if (KeyInput.CanPressKey(Keys.Enter))
            {
                Exited = true;
            }
        }

        public override void DrawUI()
        {
            base.DrawUI();

            SpriteDrawer.DrawGuiSprite(_bg, new Vector2(0, GameConstants.HEADER_HEIGHT), Z: DrawingUtilities.GetDrawingZ(DrawOrder.PAUSE_BG));

            _mapLabel.Draw();
            _itemsLabel.Draw();
            _cardsLabel.Draw();
            _saveLabel.Draw();
            _configLabel.Draw();
            _secretsLabel.Draw();
        }

        private void CreateLabels()
        {
            float x = 10f;
            float startY = GameConstants.HEADER_HEIGHT + 11;
            float yStep = GameConstants.FONT_LINE_HEIGHT * 2;

            _mapLabel = new UILabel(new Vector2(x, startY), true);
            _itemsLabel = new UILabel(new Vector2(x, startY + yStep), true);
            _cardsLabel = new UILabel(new Vector2(x, startY + yStep * 2), true);
            _saveLabel = new UILabel(new Vector2(x, startY + yStep * 3), true);
            _configLabel = new UILabel(new Vector2(x, startY + yStep * 4), true);
            _secretsLabel = new UILabel(new Vector2(x, startY + yStep * 5), true);

            _mapLabel.Initialize();
            _itemsLabel.Initialize();
            _cardsLabel.Initialize();
            _saveLabel.Initialize();
            _configLabel.Initialize();
            _secretsLabel.Initialize();


            //TODO: Localization
            _mapLabel.SetText("Map");
            _itemsLabel.SetText("Items");
            _cardsLabel.SetText("Cards");
            _saveLabel.SetText("Save");
            _configLabel.SetText("Config");
            _secretsLabel.SetText("???");
        }
    }
}
