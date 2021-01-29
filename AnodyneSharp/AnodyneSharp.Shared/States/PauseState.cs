using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using AnodyneSharp.States.MenuSubstates;
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

        private PauseStateState _lastAllowedState
        {
            get
            {
                return InventoryManager.UnlockedSecretz ? PauseStateState.Secretz : PauseStateState.Achievements;
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

        private Substate _substate;

        public PauseState()
        {
            _bg = ResourceManager.GetTexture("menu_bg", true);
            _selector = new PauseMenuSelector(new Vector2(0, 30));

            _lastState = _state;

            SetLabels();
            StateChanged();

            UpdateEntities = false;
        }

        public override void Update()
        {
            if (GlobalState.CUR_HEALTH == 0)
            {
                Exit = true;
                return;
            }

            if (GlobalState.RefreshLabels)
            {
                GlobalState.RefreshLabels = false;
                SetLabels();
            }

            _playtimeLabel.SetText((DateTime.Now - GlobalState.START_TIME).ToString(@"hh\:mm\:ss"));

            if (KeyInput.ControllerModeChanged)
            {
                _inputLabel.SetText($"{DialogueManager.GetDialogue("misc", "any", "secrets", 13)} {DialogueManager.GetDialogue("misc", "any", "secrets", 14)}");
            }

            _selector.Update();
            _selector.PostUpdate();

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Pause) || (!_inSubstate && KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel)))
            {
                SoundManager.PlayPitchedSoundEffect("pause_sound", -0.1f);
                Exit = true;
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
                    if(GlobalState.WARP) //exit pause state if substate triggered a warp
                    {
                        Exit = true;
                        return;
                    }

                    if (_substate is SaveSubstate s && s.ReturnToTitle)
                    {
                        ChangeStateEvent(AnodyneGame.GameState.TitleScreen);
                    }
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

            _substate = _state switch
            {
                PauseStateState.Map => new MapSubstate(),
                PauseStateState.Equip => new EquipSubstate(),
                PauseStateState.Cards => new CardSubstate(),
                PauseStateState.Save => new SaveSubstate(),
                PauseStateState.Settings => new ConfigSubstate(),
                PauseStateState.Achievements => new AchievementsSubstate(),
                PauseStateState.Secretz => new SecretSubstate(),
                PauseStateState.Cheatz => new CheatzSubstate(),
                _ => new Substate(),
            };
        }

        private void BrowseInput()
        {
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) || KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
            {
                SoundManager.PlaySoundEffect("menu_select");
                _inSubstate = true;
                _selector.Play("inactive");
                _substate.GetControl();
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
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
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
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

        private void SetLabels()
        {
            float x = 10f;
            float startY = GameConstants.HEADER_HEIGHT - GameConstants.LineOffset + 11 + (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0) ;
            float yStep = (GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset ) * 2;

            _mapLabel = new UILabel(new Vector2(x, startY), true);
            _itemsLabel = new UILabel(new Vector2(x, startY + yStep), true);
            _cardsLabel = new UILabel(new Vector2(x, startY + yStep * 2), true);
            _saveLabel = new UILabel(new Vector2(x, startY + yStep * 3), true);
            _configLabel = new UILabel(new Vector2(x, startY + yStep * 4), true);
            _achievementsLabel = new UILabel(new Vector2(x, startY + yStep * 5), true);
            _secretsLabel = new UILabel(new Vector2(x, startY + yStep * 6), true);


            _playtimeLabel = new UILabel(new Vector2(1, 154), true);

            Vector2 inputPos = Vector2.Zero;
            if (GlobalState.CurrentLanguage == Language.ZH_CN)
            {
                inputPos = new Vector2(2, 168 - GameConstants.LineOffset +1);
            }
            else
            {
                inputPos = new Vector2(2, 168);
                if (GlobalState.CurrentLanguage == Language.KR)
                {
                    inputPos.Y -= 1;
                }
            }
            _inputLabel = new UILabel(inputPos, false, new Color(143, 153, 176, 255));

            _mapLabel.Initialize();
            _itemsLabel.Initialize();
            _cardsLabel.Initialize();
            _saveLabel.Initialize();
            _configLabel.Initialize();
            _achievementsLabel.Initialize();
            _secretsLabel.Initialize();

            _playtimeLabel.Initialize(true);
            _inputLabel.Initialize();

            _mapLabel.SetText(DialogueManager.GetDialogue("misc", "any", "map", 0));
            _itemsLabel.SetText(DialogueManager.GetDialogue("misc", "any", "items", 0));
            _cardsLabel.SetText(DialogueManager.GetDialogue("misc", "any", "cards", 0));
            _saveLabel.SetText(DialogueManager.GetDialogue("misc", "any", "save", 0));
            _configLabel.SetText(DialogueManager.GetDialogue("misc", "any", "config", 0));
            _achievementsLabel.SetText("Feats");
            _secretsLabel.SetText("???");

            _playtimeLabel.SetText("00:00:00");
            _inputLabel.SetText($"{DialogueManager.GetDialogue("misc", "any", "secrets", 13)} {DialogueManager.GetDialogue("misc", "any", "secrets", 14)}");

            _secretsLabel.IsVisible = InventoryManager.UnlockedSecretz;
        }
    }
}
