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
            : base(pos, textureName, 18, 18, DrawOrder.EQUIPMENT_ICON)
        {
            _label = new UILabel(new Vector2(pos.X + sprite.Width + 4, pos.Y + sprite.Height /4 -1 - (GlobalState.CurrentLanguage == Dialogue.Language.ZH_CN ? GameConstants.LineOffset - 2 : 0)), true, text);

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
