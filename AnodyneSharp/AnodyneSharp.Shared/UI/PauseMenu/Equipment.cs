using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu
{
    public class Equipment : UIEntity
    {
        public Vector2 LabelPos
        {
            get
            {
                return _label.Position;
            }
        }

        public bool equipped;

        private UILabel _label;
        private Texture2D _equipIcon;

        public Equipment(Vector2 pos, string textureName, string text)
            : base(pos, 18, 18, DrawOrder.EQUIPMENT_ICON)
        {
            _label = new UILabel(new Vector2(pos.X + frameWidth + 4, pos.Y + frameHeight /4 -1 - (GlobalState.CurrentLanguage == Dialogue.Language.ZHS ? GameConstants.LineOffset - 2 : 0)), true);
            _label.Initialize();
            _label.SetText(text);

            SetTexture(textureName);
            visible = text != "-";

            SetFrame(0);

            _equipIcon = ResourceManager.GetTexture("equipped_icon", true);
        }

        public override void Draw()
        {
            base.Draw();

            _label.Draw();

            if (equipped)
            {
                SpriteDrawer.DrawGuiSprite(_equipIcon, Position + new Vector2(12, -1), Z: DrawingUtilities.GetDrawingZ(DrawOrder.EQUIPPED_ICON));
            }

        }
    }
}
