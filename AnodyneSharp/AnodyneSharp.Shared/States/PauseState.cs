using AnodyneSharp.Drawing;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using AnodyneSharp.States.PauseSubstates;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
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
        private enum PauseStateState
        {
            Map,
            Equip,
            Cards,
            Save,
            Settings,
            Secretz,
            Cheatz
        }

        private const int CheatCounterMax =
#if DEBUG
            2;
#else
            20;
#endif

        private static PauseStateState _state;

        public bool Exited { get; private set; }

        private PauseStateState _lastAllowedState
        {
            get
            {
                return InventoryState.UnlockedSecretz ? PauseStateState.Secretz : PauseStateState.Settings;
            }
        }

        private Texture2D _bg;

        private UILabel _mapLabel;
        private UILabel _itemsLabel;
        private UILabel _cardsLabel;
        private UILabel _saveLabel;
        private UILabel _configLabel;
        private UILabel _secretsLabel;

        private PauseMenuSelector _selector;
        private PauseStateState _lastState;
        private int cheat_counter;

        private bool _inSubstate;

        private PauseSubstate _substate;

        public PauseState()
        {
            _bg = ResourceManager.GetTexture("menu_bg");
            _selector = new PauseMenuSelector(new Vector2(0, 30));

            _lastState = _state;

            CreateLabels();
            StateChanged();
        }

        public override void Update()
        {
            _selector.Update();
            _selector.PostUpdate();

            if (KeyInput.CanPressKey(Keys.Enter))
            {
                Exited = true;
            }
            else if (!_inSubstate)
            {
                BrowseInput();
            }
            else
            {
                _substate.HandleInput();

                if (_substate.Exit)
                {
                    _inSubstate = false;
                    _substate.Exit = false;
                    _selector.Play("flash");
                }
            }


            _substate.Update();


            if (_lastState != _state)
            {
                StateChanged();
            }

        }

        public override void DrawUI()
        {
            base.DrawUI();

            SpriteDrawer.DrawGuiSprite(_bg, new Vector2(0, GameConstants.HEADER_HEIGHT), Z: DrawingUtilities.GetDrawingZ(DrawOrder.PAUSE_BG));
            _selector.Draw();

            _mapLabel.Draw();
            _itemsLabel.Draw();
            _cardsLabel.Draw();
            _saveLabel.Draw();
            _configLabel.Draw();
            _secretsLabel.Draw();

            _substate.DrawUI();
        }

        private void StateChanged()
        {
            _lastState = _state;
            _selector.Position = new Vector2(0, 30 + (int)_state * 16);

            switch (_state)
            {
                //case PauseStateState.Map:
                //    break;
                case PauseStateState.Equip:
                    _substate = new EquipSubstate();
                    break;
                //case PauseStateState.Cards:
                //    break;
                //case PauseStateState.Save:
                //    break;
                //case PauseStateState.Settings:
                //    break;
                //case PauseStateState.Secretz:
                //    break;
                //case PauseStateState.Cheatz:
                //    break;
                default:
                    _substate = new PauseSubstate();
                    break;
            }
        }

        private void BrowseInput()
        {
            if (KeyInput.CanPressKey(Keys.C) || KeyInput.CanPressKey(Keys.Right))
            {
                SoundManager.PlaySoundEffect("menu_select");
                _inSubstate = true;
                _selector.Play("inactive");
                _substate.GetControl();
            }
            else if (KeyInput.CanPressKey(Keys.Up))
            {
                SoundManager.PlaySoundEffect("menu_move");

                if (_state == PauseStateState.Cheatz)
                {
                    _state = _lastAllowedState;
                    return;
                }

                if (_state == PauseStateState.Map)
                {
                    return;
                }

                _state--;

            }
            else if (KeyInput.CanPressKey(Keys.Down))
            {
                SoundManager.PlaySoundEffect("menu_move");

                if (_state == PauseStateState.Cheatz)
                {
                    return;
                }

                if (_state == _lastAllowedState)
                {
                    cheat_counter++;

                    if (cheat_counter == CheatCounterMax)
                    {
                        cheat_counter = 0;
                        _state = PauseStateState.Cheatz;

                    }

                    return;
                }

                _state++;
            }

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

            _secretsLabel.IsVisible = InventoryState.UnlockedSecretz;
        }
    }
}
