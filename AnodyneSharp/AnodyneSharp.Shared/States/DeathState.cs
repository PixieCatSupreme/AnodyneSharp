using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
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

namespace AnodyneSharp.States
{
    public class DeathState : State
    {
        private enum CurrentState
        {
            FadeIn,
            SelectOption,
            FadeOut
        }

        PlayerDieDummy _dieDummy;
        DeathFadeIn _deathFade;

        UILabel _continueLabel;
        UILabel _yesLabel;
        UILabel _noLabel;

        MenuSelector _selector;

        bool gotControl;
        bool yesSelected;

        CurrentState state;

        Player _player;

        public DeathState(Player player)
        {
            GlobalState.DeathCount++;

            _player = player;

            player.Reset();

            GlobalState.wave.Deactivate();

            _dieDummy = new PlayerDieDummy(player.Position);

            _deathFade = new DeathFadeIn(GlobalState.IsCell ? Color.Black : Color.White);

            float x = 50;
            float y = 60 /*- GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0)*/;
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset;

            Color textColor = GlobalState.IsCell ? Color.White : Color.Black;

            //TODO Not localized, see if it's on purpose
            _continueLabel = new UILabel(new Vector2(x, y), false, "Continue?", textColor, DrawOrder.DEATH_TEXT);
            _yesLabel = new UILabel(new Vector2(x + 19, y + yStep), false, "Yes", textColor, DrawOrder.DEATH_TEXT);
            _noLabel = new UILabel(new Vector2(x + 17, y + yStep * 2), false, "No..", textColor, DrawOrder.DEATH_TEXT);


            _selector = new MenuSelector(DrawOrder.DEATH_TEXT);

            yesSelected = true;

            UpdateEntities = false;

            state = CurrentState.FadeIn;

            _player.dontMove = true;
            _player.exists = false;
            GlobalState.disable_menu = true;
        }

        public override void Update()
        {
            _dieDummy.Update();
            _dieDummy.PostUpdate();

            if (!gotControl && _dieDummy.AnimFinished)
            {
                SoundManager.PlaySoundEffect("player_hit_1");
                SoundManager.PlaySong("gameover");

                GetControl();
            }

            _deathFade.Update();

            _selector.Update();
            _selector.PostUpdate();

            if (state == CurrentState.FadeOut)
            {
                GlobalState.black_overlay.ChangeAlpha(0.6f);

                if (GlobalState.black_overlay.alpha == 1)
                {
                    GlobalState.WARP = true;
                    Exit = true;
                }
            }
            else if (state == CurrentState.SelectOption)
            {
                if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                {
                    _selector.Position = new Vector2(53, _yesLabel.Position.Y + 2);
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

                    GlobalState.CUR_HEALTH = GlobalState.MAX_HEALTH;

                    state = CurrentState.FadeOut;

                    if (yesSelected)
                    {
                        (GlobalState.NEXT_MAP_NAME, GlobalState.PLAYER_WARP_TARGET) = (GlobalState.checkpoint.map,GlobalState.checkpoint.Position);

                        _player.dontMove = false;
                        _player.exists = true;
                        GlobalState.disable_menu = false;
                    }
                    else
                    {
                        GlobalState.NEXT_MAP_NAME = "DRAWER";
                        GlobalState.PLAYER_WARP_TARGET = new Vector2(368, 224);


                        _player.ANIM_STATE = PlayerAnimState.as_slumped;

                        GlobalState.inventory.EquippedBroom =  BroomType.NONE;

                        GlobalState.InDeathRoom = true;
                    }
                }
            }
        }

        public override void DrawUI()
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

            state = CurrentState.SelectOption;
        }
    }
}
