using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.States
{
    public class CreditsState : State
    {

        private const int maxLabels = 28;

        private List<UILabel> _labels;
        private List<UIEntity> _entities;

        private UILabel _endLabel;

        private UIEntity _dimOverlay;
        private UIEntity _bg;
        private Screenie _screenie;
        private SavePopup _savePopup;

        private IEnumerator _stateLogic;
        private bool _stopScroll;


        public CreditsState()
        {
            _labels = new List<UILabel>();
            _entities = new List<UIEntity>();

            int bgHeight = 480;

            _dimOverlay = new UIEntity(Vector2.Zero, "dim_overlay", 160, 180, DrawOrder.PAUSE_SELECTOR)
            {
                opacity = 0.2f
            };

            _bg = new UIEntity(new Vector2(0, -bgHeight + 180), "go", 160, bgHeight, new RefLayer(_dimOverlay.layer_def, -1));

            _screenie = new Screenie();

            int y = 0;

            for (int i = 0; i < maxLabels; i++)
            {
                if (i != 0 && i % 2 == 0)
                {
                    y += 195;
                }
                else
                {
                    y += 180;
                }


                string text = DialogueManager.GetDialogue("misc", "any", "ending", i);

                _labels.Add(new UILabel(new Vector2(0, y), true, text, layer: DrawOrder.TEXT, centerText: true));
            }

            _endLabel = new UILabel(new Vector2(0), false, DialogueManager.GetDialogue("misc", "any", "ending", 25 + 4), layer: DrawOrder.TEXT, centerText: true)
            {
                IsVisible = false
            };

            _savePopup = new SavePopup(this);

            CreateEntities();

            _stateLogic = StateLogic();
        }

        public override void Update()
        {
            base.Update();

            if (!_stopScroll)
            {
                if (KeyInput.IsRebindableKeyPressed(KeyFunctions.Accept))
                {
                    if (GlobalState.FUCK_IT_MODE_ON)
                    {
                        GameTimes.TimeScale = 80;
                    }
                    else
                    {
                        GameTimes.TimeScale = 8;
                    }
                }
                else
                {
                    GameTimes.TimeScale = 1;
                }

                foreach (var entity in _entities)
                {
                    entity.PostUpdate();
                }

                float speed = 15 * GameTimes.DeltaTime;

                for (int i = 0; i < _labels.Count; i++)
                {
                    var label = _labels[i];
                    label.Position += new Vector2(0, -speed);

                    if (i == _labels.Count - 1 && label.Position.Y <= 0)
                    {
                        label.Position = new Vector2(0);
                        _stopScroll = true;
                    }
                }
            }
            else
            {
                GameTimes.TimeScale = 1;
            }

            _stateLogic.MoveNext();

            _screenie.Update();

            _savePopup.Update();
        }

        public override void DrawUI()
        {
            base.DrawUI();

            _dimOverlay.Draw();
            _bg.Draw();

            _screenie.Draw();

            _savePopup.Draw();

            foreach (var label in _labels)
            {
                label.Draw();
            }

            foreach (var entity in _entities)
            {
                entity.Draw();
            }

            _endLabel.Draw();
        }

        private IEnumerator StateLogic()
        {
            SoundManager.PlaySong("ending");

            while (!MathUtilities.MoveTo(ref _bg.Position.Y, 0, 15))
            {
                yield return null;
            }

            int index = 6;

            while (index < maxLabels)
            {
                ProgressScreenie(ref index);
                yield return null;
            }

            while (!MathUtilities.MoveTo(ref _dimOverlay.opacity, 0, 0.06f))
            {
                yield return null;
            }

            while (_stopScroll)
            {
                yield return null;
            }

            while (!KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
            {
                yield return null;
            }

            _screenie.IsVisible = false;
            _bg.visible = false;

            _labels.Last().IsVisible = false;
            _endLabel.IsVisible = true;

            _savePopup.IsVisible = true;

            foreach (var entity in _entities)
            {
                entity.visible = false;
            }

            yield break;
        }

        private void ProgressScreenie(ref int index)
        {
            while (_labels[index].Position.Y > 180)
            {
                return;
            }

            index++;

            _screenie.ProgressFrame();
        }

        private void CreateEntities()
        {
            int i = 16;

            Vector2 lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(4, 26) + new Vector2(4, 10), "slime", new Point(16), 5, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(130, 46) + new Vector2(4, 20), "annoyer", new Point(16), 8, false, false, 0, 1, 2, 3, 4, 5);
            CreateEntity(lPos + new Vector2(5, 66) + new Vector2(4, 30), "pew_laser", new Point(16), 0, false, false, 0);

            CreateEntity(lPos + new Vector2(130, 92) + new Vector2(4, 30), "shieldy", new Point(16), 5, false, false, 1, 2, 1, 0, 1, 2, 1, 0, 16, 17, 18);
            CreateEntity(lPos + new Vector2(5, 84) + new Vector2(4, 30), "pew_laser_bullet", new Point(16, 8), 8, false, false, 0, 1);

            CreateEntity(lPos + new Vector2(11, 110) + new Vector2(4, 30), "sun_guy", new Point(16, 24), 3, false, false, 0, 1, 2, 3, 4);
            CreateEntity(lPos + new Vector2(125, 120) + new Vector2(4, 30), "light_orb", new Point(16), 6, false, false, 0, 1, 2, 3, 4, 3, 2, 1);
            CreateEntity(lPos + new Vector2(8, 144) + new Vector2(4, 30), "sun_guy_wave", new Point(128, 8), 8, false, false, 3, 4, 5);

            lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(35, -4) + new Vector2(4, 10), "f_mover", new Point(16), 4, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(108, 30) + new Vector2(10, 0), "on_off_shooter", new Point(16), 2, false, false, 0, 1, 2, 2, 1, 0);
            CreateEntity(lPos + new Vector2(4, 20) + new Vector2(10, 40), "f_four_shooter", new Point(16), 3, false, false, 0, 1, 2, 2, 1, 0);
            CreateEntity(lPos + new Vector2(115, 68) + new Vector2(4, 12), "f_slasher", new Point(24), 3, false, false, 0, 1, 0, 1, 0, 1);
            CreateEntity(lPos + new Vector2(12, 110) + new Vector2(4, 2), "red_boss", new Point(32), 3, false, false, 0, 0, 1, 0, 0, 2);
            CreateEntity(lPos + new Vector2(4, 138) + new Vector2(4, 2), "red_boss_ripple", new Point(48, 8), 12, false, false, 0, 1);

            lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(22, -3) + new Vector2(4, 4), "dog", new Point(16), 4, false, false, 2, 3, 2, 3, 4, 5, 4, 5, 6, 7, 6, 7, 2, 3);
            CreateEntity(lPos + new Vector2(118, 20) + new Vector2(4, 10), "frog", new Point(16), 2, false, false, 0, 1, 0, 1, 3, 3);

            CreateEntity(lPos + new Vector2(20, 42) + new Vector2(4, 12), "f_rotator", new Point(16), 10, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(120, 68) + new Vector2(4, 16), "person", new Point(16), 5, false, false, 0, 1, 0, 1, 2, 3, 2, 3, 4, 5, 4, 5, 2, 3, 2, 3);

            CreateEntity(lPos + new Vector2(0, 120) + new Vector2(0, 10), "wallboss_wall", new Point(160, 32), 4, false, false, 0, 1, 0, 1, 0, 1);
            CreateEntity(lPos + new Vector2(48, 120) + new Vector2(0, 10), "f_wallboss_face", new Point(64, 32), 3, false, true, 0, 0, 1, 0, 0, 2);

            CreateEntity(lPos + new Vector2(8, 150) + new Vector2(4, 10), "f_wallboss_l_hand", new Point(32), 1, false, true, 0, 1, 2, 3);
            CreateEntity(lPos + new Vector2(118, 150) + new Vector2(4, 10), "f_wallboss_l_hand", new Point(32), 1, true, true, 0, 1, 2, 3);

            lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(16, -2) + new Vector2(16, 0), "rat", new Point(16), 5, false, false, 0, 1);

            CreateEntity(lPos + new Vector2(122, 20) + new Vector2(4, 10), "gas_guy", new Point(16, 24), 4, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(137, 34) + new Vector2(4, 10), "gas_guy_cloud", new Point(24), 3, false, false, 0, 1);

            CreateEntity(lPos + new Vector2(5, 46) + new Vector2(4, 10), "silverfish", new Point(16), 5, false, false, 4, 5);
            CreateEntity(lPos + new Vector2(137, 66) + new Vector2(4, 26), "dash_trap", new Point(16), 12, false, false, 4, 5);

            CreateEntity(lPos + new Vector2(5, 78) + new Vector2(4, 24), "spike_roller_horizontal", new Point(128, 16), 5, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(70, 120) + new Vector2(-4, 30), "splitboss", new Point(24, 32), 5, false, false, 0, 1, 2, 1);

            lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(10, -12) + new Vector2(4, 10), "dustmaid", new Point(16, 24), 7, false, false, 0, 0, 0, 1, 2, 1, 2, 3, 4, 3, 4);
            CreateEntity(lPos + new Vector2(120, 20) + new Vector2(4, 10), "burst_plant", new Point(16), 8, false, false, 0, 0, 1, 0, 1, 3, 3, 3, 3, 0);
            CreateEntity(lPos + new Vector2(5, 70) + new Vector2(4, 10), "eye_boss_water", new Point(24), 6, false, false, 0, 1, 2, 3, 2, 1);
            CreateEntity(lPos + new Vector2(120, 70) + new Vector2(4, 10), "eye_boss_water", new Point(24), 6, false, false, 4, 5, 4, 5, 6, 7, 6);

            lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(10, -5) + new Vector2(4, -10), "lion", new Point(32), 4, false, false, 10, 11, 10, 11, 10, 11, 12, 12);

            CreateEntity(lPos + new Vector2(140, 20) + new Vector2(0, -30), "contort_big", new Point(16, 32), 9, false, false, 0, 1, 2, 1);
            CreateEntity(lPos + new Vector2(140, 50) + new Vector2(0, -30), "contort_small", new Point(16), 9, false, false, 0, 1);

            CreateEntity(lPos + new Vector2(118, 50) + new Vector2(0, -30), "contort_small", new Point(16), 9, false, false, 2, 3);
            CreateEntity(lPos + new Vector2(126, 65) + new Vector2(0, -30), "contort_small", new Point(16), 9, false, false, 4, 5);

            CreateEntity(lPos + new Vector2(5, 54 + 32 - 4 - 12) + new Vector2(4, 10), "fire_pillar_base", new Point(16), 8, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(5, 54) + new Vector2(4, 10), "fire_pillar", new Point(16, 32), 9, false, true, 0, 0, 0, 0, 1, 2, 3, 4, 3, 4, 5, 6, 0);

            CreateEntity(lPos + new Vector2(65, 94) + new Vector2(8, 28), "arthur_javiera", new Point(16, 32), 8, false, false, 0, 1);

            lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(14, -5) + new Vector2(4, 10), "follower_bro", new Point(16, 24), 4, false, false, 1, 2, 1, 0);

            CreateEntity(lPos + new Vector2(120, 20) + new Vector2(4, 10), "sadman", new Point(16), 2, false, false, 0, 1);

            CreateEntity(lPos + new Vector2(6, 52) + new Vector2(4, 10), "beach_npcs", new Point(16), 3, false, false, 10, 11);

            CreateEntity(lPos + new Vector2(130, 50) + new Vector2(-6, 10), "redwalker", new Point(32, 48), 6, false, false, 0, 1, 2, 3, 4);

            CreateEntity(lPos + new Vector2(6, 80) + new Vector2(4, 28), "beach_npcs", new Point(16), 2, false, false, 0);

            lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(129, 0) + new Vector2(4, 0), "forest_npcs", new Point(16), 4, false, false, 30, 31);
            CreateEntity(lPos + new Vector2(2, 15) + new Vector2(4, 10), "fields_npcs", new Point(16), 4, false, false, 10, 11);

            CreateEntity(lPos + new Vector2(2, 45) + new Vector2(4, 10), "fields_npcs", new Point(16), 4, false, false, 50, 51);
            CreateEntity(lPos + new Vector2(140, 49) + new Vector2(4, 28), "fields_npcs", new Point(16), 4, false, false, 0, 1);

            CreateEntity(lPos + new Vector2(2, 80) + new Vector2(4, 2), "fields_npcs", new Point(32), 18, false, false, 15, 15, 15, 15, 15, 15, 15, 15, 16, 17, 17, 18, 18);
            CreateEntity(lPos + new Vector2(124, 91) + new Vector2(4, 26), "fields_npcs", new Point(16), 4, false, false, 20, 21);


            lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(4, 0) + new Vector2(4, -4), "forest_npcs", new Point(16), 4, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(130, 0) + new Vector2(4, 18), "forest_npcs", new Point(16), 4, false, false, 10, 10, 11, 10, 10, 12);

            CreateEntity(lPos + new Vector2(4, 20) + new Vector2(4, 16), "forest_npcs", new Point(16), 4, false, false, 20, 21, 20, 22);
            CreateEntity(lPos + new Vector2(130, 20) + new Vector2(4, 38), "forest_npcs", new Point(16), 4, false, false, 30, 31);

            CreateEntity(lPos + new Vector2(4, 40) + new Vector2(4, 36), "cliffs_npcs", new Point(16), 4, false, false, 1, 3, 1, 5, 1, 1, 1, 0, 2, 0, 4, 0, 0, 1, 1);

            CreateEntity(lPos + new Vector2(3, 83) + new Vector2(4, 20), "suburb_walkers", new Point(16), 4, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(140, 80) + new Vector2(0, 20), "suburb_walkers", new Point(16), 4, false, false, 9, 10);

            CreateEntity(lPos + new Vector2(3, 103) + new Vector2(4, 20), "suburb_walkers", new Point(16), 4, false, false, 18, 19);
            CreateEntity(lPos + new Vector2(140, 103) + new Vector2(0, 20), "suburb_walkers", new Point(16), 4, false, false, 27, 28);

            CreateEntity(lPos + new Vector2(3, 123) + new Vector2(4, 20), "suburb_walkers", new Point(16), 4, false, false, 36, 37);
            CreateEntity(lPos + new Vector2(140, 123) + new Vector2(0, 20), "suburb_walkers", new Point(16), 4, false, false, 45, 46);

            lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(12, 0) + new Vector2(4, -8), "chaser", new Point(16, 32), 4, false, false, 8, 9);

            CreateEntity(lPos + new Vector2(3, 106) + new Vector2(4, 10), "space_npcs", new Point(32), 4, true, false, 10, 11);
            CreateEntity(lPos + new Vector2(120, 106) + new Vector2(0, 10), "space_npcs", new Point(32), 4, true, false, 12, 13);

            CreateEntity(lPos + new Vector2(20, 40) + new Vector2(4, 16), "space_npcs", new Point(16), 4, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(120, 40) + new Vector2(4, 16), "space_npcs", new Point(16), 4, false, false, 10, 11);

            CreateEntity(lPos + new Vector2(20, 70) + new Vector2(4, 16), "space_npcs", new Point(16), 4, false, false, 20, 21);
            CreateEntity(lPos + new Vector2(120, 70) + new Vector2(4, 16), "space_npcs", new Point(16), 4, false, false, 22, 23);

            lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(39, -5) + new Vector2(4, 2), "young_player", new Point(16), 6, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(110, 17) + new Vector2(4, 12), "mitra", new Point(16), 6, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(20, 44) + new Vector2(4, 12), "sage", new Point(16), 6, false, false, 0, 1);
            CreateEntity(lPos + new Vector2(125, 67) + new Vector2(4, 22), "briar", new Point(16), 6, false, false, 0, 1);

            lPos = _labels[i++].Position;

            CreateEntity(lPos + new Vector2(6, 28) + new Vector2(4, 10), "dev_npcs", new Point(16), 0, false, false, 0);
            CreateEntity(lPos + new Vector2(140, 28) + new Vector2(0, 10), "dev_npcs", new Point(16), 0, false, false, 10);
        }

        private void CreateEntity(Vector2 pos, string texture, Point size, int framerate, bool flipped, bool inFront, params int[] frames)
        {
            UIEntity e = new(pos, texture, size.X, size.Y, inFront ? DrawOrder.SUBMENU_SLIDER : DrawOrder.TEXT, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            e.AddAnimation("a", frames, framerate);
            e.Play("a");

            e.velocity.Y = -15;

            _entities.Add(e);
        }

        private class Screenie
        {
            public bool IsVisible { get; set; }

            private UIEntity _overlay;

            private UIEntity _entity1;
            private UIEntity _entity2;

            private int _frame;
            private bool _done;

            public Screenie()
                : base()
            {
                _overlay = new UIEntity(Vector2.Zero, "dim_overlay", 160, 180, DrawOrder.CREDITS_OVERLAY)
                {
                    opacity = 0f
                };

                _entity1 = new UIEntity(new Vector2(0, 10), "screenies", 160, 160, DrawOrder.HEADER)
                {
                    opacity = 0f
                };

                _entity2 = new UIEntity(new Vector2(0, 10), "screenies", 160, 160, DrawOrder.TEXTBOX)
                {
                    opacity = 0f
                };

                _frame = -1;
                _done = false;

                IsVisible = true;
            }

            public void Update()
            {
                if (_done)
                {
                    return;
                }

                if (_frame == 0)
                {
                    if (MathUtilities.MoveTo(ref _entity1.opacity, 1, 0.54f) ||
                        MathUtilities.MoveTo(ref _overlay.opacity, 1, 0.54f))
                    {
                        _entity1.opacity = 1;
                        _overlay.opacity = 1;

                        _done = true;
                    }
                }
                else
                {
                    if (MathUtilities.MoveTo(ref _entity2.opacity, 0, 0.54f))
                    {
                        _done = true;
                    }
                }
            }

            public void Draw()
            {
                if (IsVisible)
                {
                    _overlay.Draw();
                    _entity1.Draw();
                    _entity2.Draw();
                }
            }

            public int ProgressFrame()
            {
                _frame++;

                _done = false;

                if (_frame == 0)
                {
                    _entity1.SetFrame(_frame);

                    _entity1.opacity = 0f;
                }
                else
                {
                    _entity1.SetFrame(_frame);
                    _entity2.SetFrame(_frame - 1);

                    _entity1.opacity = 1f;
                    _entity2.opacity = 1f;
                }

                return _frame;
            }
        }

        private class SavePopup
        {
            public bool IsVisible { get; set; }

            private UIEntity _background;
            private UILabel _prompt;
            private UILabel _yes;
            private UILabel _no;
            private MenuSelector _selector;

            private Vector2 _pos1;
            private Vector2 _pos2;

            private CreditsState _parent;

            private bool _inputDelay;

            public SavePopup(CreditsState parent)
            {
                int bgW = 80;
                int bgH = 29;

                Vector2 topLeft = new Vector2((160 - bgW) / 2, 20 + (160 - bgH) / 2);
                _background = new UIEntity(topLeft, "checkpoint_save_box", bgW, bgH, DrawOrder.TEXTBOX);

                _prompt = new UILabel(topLeft + new Vector2(5, 0), true, DialogueManager.GetDialogue("misc", "any", "checkpoint", 0), layer:DrawOrder.TEXT);
                _yes = new UILabel(topLeft + new Vector2(5 +14, 8), true, DialogueManager.GetDialogue("misc", "any", "checkpoint", 1), layer: DrawOrder.TEXT);
                _no = new UILabel(topLeft + new Vector2(5 + 14, 16), true, DialogueManager.GetDialogue("misc", "any", "checkpoint", 2), layer: DrawOrder.TEXT);

                _selector = new MenuSelector();
                _selector.Play("enabledRight");

                _pos1 = topLeft + new Vector2(8, 10);
                _pos2 = topLeft + new Vector2(8, 18);

                _selector.Position = _pos1;

                IsVisible = false;
                _inputDelay = true;

                _parent = parent;
            }

            public void Update()
            {
                if (!IsVisible)
                {
                    return;
                }

                _selector.Update();
                _selector.PostUpdate();

                if (_inputDelay)
                {
                    _inputDelay = false;
                    return;
                }

                if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up) &&
                    _selector.Position == _pos2)
                {
                    _selector.Position = _pos1;
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down) &&
                    _selector.Position == _pos1)
                {
                    _selector.Position = _pos2;
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                {
                    if (_selector.Position == _pos1)
                    {
                        GlobalState.events.SetEvent("SeenCredits", 1);
                        GlobalState.SaveGame();
                    }

                    SoundManager.PlaySoundEffect("menu_select");

                    _parent.ChangeStateEvent(AnodyneGame.GameState.TitleScreen);
                }
            }

            public void Draw()
            {
                if (IsVisible)
                {
                    _background.Draw();
                    _prompt.Draw();
                    _yes.Draw();
                    _no.Draw();
                    _selector.Draw();
                }
            }
        }
    }
}
