﻿using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Entities;
using AnodyneSharp.Input;
using AnodyneSharp.Logging;
using AnodyneSharp.MapData;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.MenuSubstates
{
    public class MapSubstate : Substate
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

        private Spritesheet _mapSheet;

        private float _playerIndicatorTimer = 0f;
        private const float _playerBlink = 0.4f;

        public MapSubstate()
        {
            float x = 73;
            float y = 144 - GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);

            _noMapLabel = new UILabel(new Vector2(x + 14, GameConstants.HEADER_HEIGHT + 11), true, DialogueManager.GetDialogue("misc", "any", "map", 3));

            _returnLabel = new UILabel(new Vector2(x, y - GameConstants.FONT_LINE_HEIGHT * 2), true, DialogueManager.GetDialogue("misc", "any", "map", GlobalState.IsDungeon ? 5 : 4))
            {
                IsVisible = GlobalState.ReturnTarget != null
            };

            y += 4;

            _yesLabel = new UILabel(new Vector2(x, y), true, DialogueManager.GetDialogue("misc", "any", "checkpoint", 1));
            _noLabel = new UILabel(new Vector2(x + 56, y), true, DialogueManager.GetDialogue("misc", "any", "checkpoint", 2));

            _mapSheet = new Spritesheet(ResourceManager.GetTexture("minimap_tiles",true,true),7,7);
        }

        public override void GetControl()
        {
            if(GlobalState.ReturnTarget == null)
            {
                ExitSubState();
                return;
            }
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

            _playerIndicatorTimer += GameTimes.DeltaTime;
            if(_playerIndicatorTimer > 2*_playerBlink)
            {
                _playerIndicatorTimer -= 2 * _playerBlink;
            }

            base.Update();
        }

        public override void HandleInput()
        {
            switch (_state)
            {
                case MapState.ReturnToNexusLabel:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
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
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                    {
                        _state = MapState.ReturnToNexusLabel;
                        SoundManager.PlaySoundEffect("menu_select");
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
                    {
                        _state = MapState.ReturnToNexusLabel;
                        SoundManager.PlaySoundEffect("menu_select");
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Left))
                    {
                        _state = MapState.YesLabel;
                        SoundManager.PlaySoundEffect("menu_move");
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
                    {
                        SoundManager.PlaySoundEffect("menu_move");
                    }
                    break;
                case MapState.YesLabel:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                    {
                        SoundManager.PlaySoundEffect("menu_select");
                        GlobalState.ReturnTarget.Warp(new Vector2(10, 34));
                        ExitSubState();
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
                    {
                        _state = MapState.ReturnToNexusLabel;
                        SoundManager.PlaySoundEffect("menu_select");
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Left))
                    {
                        SoundManager.PlaySoundEffect("menu_move");
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
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

            _returnLabel.Draw();

            if (_state != MapState.ReturnToNexusLabel)
            {
                _yesLabel.Draw();
                _noLabel.Draw();
            }

            Minimap minimap = GlobalState.CurrentMinimap;
            if(minimap.tiles.Width == 0)
                _noMapLabel.Draw();

            //explicit int math to not have to deal with subpixels
            int x = 110 - minimap.tiles.Width * _mapSheet.Width / 2;
            int y = 70 - minimap.tiles.Height * _mapSheet.Height / 2;
            minimap.Draw(_mapSheet,new Vector2(x,y),DrawPlayerIndicator:_playerIndicatorTimer < _playerBlink);

            selector.Draw();
        }

        private void SetSelectorPos()
        {
            switch (_state)
            {
                case MapState.ReturnToNexusLabel:
                    selector.Position = _returnLabel.Position;
                    break;
                case MapState.NoLabel:
                    selector.Position = _noLabel.Position;
                    break;
                case MapState.YesLabel:
                    selector.Position = _yesLabel.Position;
                    break;
            }

            selector.Position -= new Vector2(selector.sprite.Width, -2 - CursorOffset);
        }
    }
}
