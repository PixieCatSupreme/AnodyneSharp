using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Decorations;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Events
{
    public class DeathEvent
    {
        PlayerDieDummy _dieDummy;
        DeathFadeIn _deathFade;

        UILabel _continueLabel;
        UILabel _yesLabel;
        UILabel _noLabel;

        MenuSelector _selector;

        bool gotControl;
        bool yesSelected;

        public DeathEvent(Vector2 playerPos)
        {
            _dieDummy = new PlayerDieDummy(playerPos, GlobalState.IsCell ? Player.Cell_Player_Sprite : Player.Player_Sprite);

            _deathFade = new DeathFadeIn(GlobalState.IsCell ? Color.Black : Color.White);

            float x = 50;
            float y = 60 /*- GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0)*/;
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset;

            Color textColor = GlobalState.IsCell ? Color.White : Color.Black;

            _continueLabel = new UILabel(new Vector2(x, y), false, textColor, DrawOrder.DEATH_TEXT);
            _yesLabel = new UILabel(new Vector2(x +19, y + yStep), false, textColor, DrawOrder.DEATH_TEXT);
            _noLabel = new UILabel(new Vector2(x +17, y + yStep * 2), false, textColor, DrawOrder.DEATH_TEXT);

            _continueLabel.Initialize();
            _yesLabel.Initialize();
            _noLabel.Initialize();

            //TODO Not localized, see if it's on purpose
            _continueLabel.SetText("Continue?");
            _yesLabel.SetText("Yes");
            _noLabel.SetText("No..");

            _selector = new MenuSelector(DrawOrder.DEATH_TEXT);

            yesSelected = true;
        }

        public void Update()
        {
            _dieDummy.Update();
            _dieDummy.PostUpdate();

            if (!gotControl && _dieDummy.AnimDone)
            {
                SoundManager.PlaySoundEffect("player_hit_1");
                SoundManager.PlaySong("gameover");

                GetControl();
            }

            _deathFade.Update();

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
            {
                _selector.Position = new Vector2(53, _yesLabel.Position.Y +2);
                yesSelected = true;
                SoundManager.PlaySoundEffect("menu_move");
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
            {
                _selector.Position = new Vector2(53, _noLabel.Position.Y + 2);
                yesSelected = false;
                SoundManager.PlaySoundEffect("menu_move");
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
            {
                SoundManager.PlaySoundEffect("menu_select");

                if (yesSelected)
                {
                    //TODO load checkpoint
                }
                else
                {
                    //TODO load death room, keep menu disabled, remove items
                }
            }

            _selector.Update();
            _selector.PostUpdate();
        }

        public void DrawUI()
        {
            _dieDummy.Draw();
            _deathFade.Draw();

            if (gotControl)
            {
                _continueLabel.Draw();
                _yesLabel.Draw();
                _noLabel.Draw();

                _selector.Draw();
            }
        }

        public virtual void GetControl()
        {
            _selector.visible = true;
            _selector.Play("enabledRight");
            gotControl = true;

            _selector.Position = new Vector2(53, _yesLabel.Position.Y + 2);
        }
    }
}
