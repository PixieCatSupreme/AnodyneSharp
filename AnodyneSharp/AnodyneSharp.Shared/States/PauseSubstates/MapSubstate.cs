using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.PauseSubstates
{
    public class MapSubstate : PauseSubstate
    {
        private enum MapState
        {
            ReturnToNexusLabel,
            NoLabel,
            YesLabel
        }

        private UILabel _noMapLabel;

        private UILabel _returnLabel;
        private UILabel _yesLabel;
        private UILabel _noLabel;

        private MapState _state;
        private MapState _lastState;

        public MapSubstate()
        {
            float x = 73;
            float y = 144;

            _noMapLabel = new UILabel(new Vector2(x + 14, GameConstants.HEADER_HEIGHT + 11), true);

            _returnLabel = new UILabel(new Vector2(x, y - GameConstants.FONT_LINE_HEIGHT * 2), true);

            y += 4;

            _yesLabel = new UILabel(new Vector2(x, y), true);
            _noLabel = new UILabel(new Vector2(x + 56, y), true);

            _noMapLabel.Initialize();
            _returnLabel.Initialize();
            _yesLabel.Initialize();
            _noLabel.Initialize();


            //TODO add localization
            _noMapLabel.SetText("No Map");
            _returnLabel.SetText("Return to\nNexus");
            _yesLabel.SetText("Yes");
            _noLabel.SetText("No");
        }

        public override void GetControl()
        {
            base.GetControl();
            _state = MapState.ReturnToNexusLabel;

            SetSelectorPos();
        }

        public override void Update()
        {
            if (_lastState != _state)
            {
                _lastState = _state;
                SetSelectorPos();
            }

            base.Update();
        }

        public override void HandleInput()
        {
            switch (_state)
            {
                case MapState.ReturnToNexusLabel:
                    if (KeyInput.JustPressedKey(Keys.C))
                    {
                        _state = MapState.NoLabel;
                        SoundManager.PlaySoundEffect("menu_select");
                    }
                    else
                    {
                        base.HandleInput();
                    }
                    break;
                case MapState.NoLabel:
                    if (KeyInput.JustPressedKey(Keys.C))
                    {
                        _state = MapState.ReturnToNexusLabel;
                        SoundManager.PlaySoundEffect("menu_select");
                    }
                    else if (KeyInput.JustPressedKey(Keys.X))
                    {
                        _state = MapState.ReturnToNexusLabel;
                        SoundManager.PlaySoundEffect("menu_select");
                    }
                    else if (KeyInput.JustPressedKey(Keys.Left))
                    {
                        _state = MapState.YesLabel;
                        SoundManager.PlaySoundEffect("menu_move");
                    }
                    else if (KeyInput.JustPressedKey(Keys.Right))
                    {
                        SoundManager.PlaySoundEffect("menu_move");
                    }
                    break;
                case MapState.YesLabel:
                    if (KeyInput.JustPressedKey(Keys.C))
                    {
                        SoundManager.PlaySoundEffect("menu_select");
                        //TODO return to nexus
                    }
                    else if (KeyInput.JustPressedKey(Keys.X))
                    {
                        _state = MapState.ReturnToNexusLabel;
                        SoundManager.PlaySoundEffect("menu_select");
                    }
                    else if (KeyInput.JustPressedKey(Keys.Left))
                    {
                        SoundManager.PlaySoundEffect("menu_move");
                    }
                    else if (KeyInput.JustPressedKey(Keys.Right))
                    {
                        _state = MapState.NoLabel;
                        SoundManager.PlaySoundEffect("menu_move");
                    }
                    break;
            }
        }

        public override void DrawUI()
        {
            base.DrawUI();

            _noMapLabel.Draw();
            _returnLabel.Draw();

            if (_state != MapState.ReturnToNexusLabel)
            {
                _yesLabel.Draw();
                _noLabel.Draw();
            }

            _selector.Draw();
        }

        private void SetSelectorPos()
        {
            switch (_state)
            {
                case MapState.ReturnToNexusLabel:
                    _selector.Position = _returnLabel.Position;
                    break;
                case MapState.NoLabel:
                    _selector.Position = _noLabel.Position;
                    break;
                case MapState.YesLabel:
                    _selector.Position = _yesLabel.Position;
                    break;
            }

            _selector.Position -= new Vector2(_selector.frameWidth, -2);
        }
    }
}
