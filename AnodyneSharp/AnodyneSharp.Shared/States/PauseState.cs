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
            Achievements,
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
                return InventoryState.UnlockedSecretz ? PauseStateState.Secretz : PauseStateState.Achievements;
            }
        }

        private Texture2D _bg;

        private UILabel _mapLabel;
        private UILabel _itemsLabel;
        private UILabel _cardsLabel;
        private UILabel _saveLabel;
        private UILabel _configLabel;
        private UILabel _achievementsLabel;
        private UILabel _secretsLabel;

        private UILabel _playtimeLabel;
        private UILabel _inputLabel;

        private PauseMenuSelector _selector;
        private PauseStateState _lastState;
        private int cheat_counter;

        private bool _inSubstate;

        private PauseSubstate _substate;

        public PauseState()
        {
            _bg = ResourceManager.GetTexture("menu_bg", true);
            _selector = new PauseMenuSelector(new Vector2(0, 30));

            _lastState = _state;

            CreateLabels();
            StateChanged();
        }

        public override void Update()
        {
            _playtimeLabel.SetText((DateTime.Now - GlobalState.START_TIME).ToString(@"hh\:mm\:ss"));

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
            _achievementsLabel.Draw();
            _secretsLabel.Draw();

            _playtimeLabel.Draw();
            _inputLabel.Draw();

            _substate.DrawUI();
        }

        private void StateChanged()
        {
            _lastState = _state;
            _selector.Position = new Vector2(0, 30 + (int)_state * 16);

            switch (_state)
            {
                case PauseStateState.Map:
                    _substate = new MapSubstate();
                    break;
                case PauseStateState.Equip:
                    _substate = new EquipSubstate();
                    break;
                case PauseStateState.Cards:
                    _substate = new CardSubstate();
                    break;
                case PauseStateState.Save:
                    _substate = new SaveSubstate();
                    break;
                case PauseStateState.Settings:
                    _substate = new ConfigSubstate();
                    break;
                case PauseStateState.Achievements:
                    _substate = new AchievementsSubstate();
                    break;
                case PauseStateState.Secretz:
                    _substate = new SecretSubstate();
                    break;
                case PauseStateState.Cheatz:
                    _substate = new CheatzSubstate();
                    break;
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
                if (_state == PauseStateState.Cheatz)
                {
                    SoundManager.PlaySoundEffect("menu_move");
                    _state = _lastAllowedState;
                    return;
                }

                if (_state == PauseStateState.Map)
                {
                    return;
                }

                SoundManager.PlaySoundEffect("menu_move");
                _state--;

                if (_state == PauseStateState.Achievements && InventoryState.AchievementsDisabled)
                {
                    _state--;
                }

            }
            else if (KeyInput.CanPressKey(Keys.Down))
            {
                if (_state == PauseStateState.Cheatz)
                {
                    return;
                }

                if (_state == _lastAllowedState)
                {
                    SoundManager.PlaySoundEffect("menu_move");
                    cheat_counter++;

                    if (cheat_counter == CheatCounterMax)
                    {
                        cheat_counter = 0;
                        _state = PauseStateState.Cheatz;
                    }

                    return;
                }

                SoundManager.PlaySoundEffect("menu_move");
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
            _achievementsLabel = new UILabel(new Vector2(x, startY + yStep * 5), true);
            _secretsLabel = new UILabel(new Vector2(x, startY + yStep * 6), true);

            _playtimeLabel = new UILabel(new Vector2(1, 154), true);
            _inputLabel = new UILabel(new Vector2(9, 168), new Color(143, 153, 176, 255), false);

            _mapLabel.Initialize();
            _itemsLabel.Initialize();
            _cardsLabel.Initialize();
            _saveLabel.Initialize();
            _configLabel.Initialize();
            _achievementsLabel.Initialize();
            _secretsLabel.Initialize();

            _playtimeLabel.Initialize();
            _inputLabel.Initialize();

            //TODO: Localization
            _mapLabel.SetText("Map");
            _itemsLabel.SetText("Items");
            _cardsLabel.SetText("Cards");
            _saveLabel.SetText("Save");
            _configLabel.SetText("Config");
            _achievementsLabel.SetText("Feats");
            _secretsLabel.SetText("???");

            _playtimeLabel.SetText("00:00:00");
            _inputLabel.SetText(" :Select  :Back");

            _secretsLabel.IsVisible = InventoryState.UnlockedSecretz;
        }
    }
}
