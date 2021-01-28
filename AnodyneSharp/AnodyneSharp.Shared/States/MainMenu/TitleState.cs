using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.FSM;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.Font;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.States.MainMenu
{
    public class TitleState : State
    {
        private static bool AnyKeyPressed => KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) || KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel) || KeyInput.JustPressedRebindableKey(KeyFunctions.Pause);


        private ScrollingTex _background;

        private UIEntity nexusImage;
        private UIEntity doorGlow;
        private UIEntity doorSpin1;
        private UIEntity doorSpin2;

        private UIEntity title;
        private UIEntity titleOverlay;
        private UIEntity pressEnter;

        private IState _state;

        private string[] names;

        private UILabel[] nameLabels;

        private List<(int, int)> notVisibleYet;

        class TextDrawTimer : TimerState
        {
            public TextDrawTimer()
            {
                AddTimer(0.03f, "DrawText");
            }
        }

        class TextFadeTimer : TimerState
        {
            public TextFadeTimer()
            {
                AddTimer(1f, "EndFade");
            }
        }

        class PressEnterTimer : TimerState
        {
            public PressEnterTimer()
            {
                AddTimer(1, "BlinkEnter");
            }
        }

        public TitleState()
        {
            names = new string[]
            {
                DialogueManager.GetDialogue("misc", "any", "title", 2),
                DialogueManager.GetDialogue("misc", "any", "title", 3),
                DialogueManager.GetDialogue("misc", "any", "title", 4)
            };

            nameLabels = Array.Empty<UILabel>();

            _state = new StateMachineBuilder()
                .State("IntroFade")
                    .Enter((state) =>
                    {
                        GlobalState.flash.Flash(2f, Color.Black);
                        GlobalState.FullscreenDarkness.Darkness = ResourceManager.GetTexture("title_overlay1");
                        GlobalState.FullscreenDarkness.ForceAlpha(1);
                    })
                    .Condition(() => AnyKeyPressed, (s) => _state.ChangeState("PressStart"))
                    .Condition(() => !GlobalState.flash.Active(), (s) => _state.ChangeState("CreditsWrite"))
                .End()
                .State<TextDrawTimer>("CreditsWrite")
                    .Enter((state) =>
                    {
                        int lineH = GameConstants.FONT_LINE_HEIGHT + 2;
                        int center = GameConstants.SCREEN_WIDTH_IN_PIXELS / 2;
                        int charWidth = FontManager.GetCharacterWidth(true);

                        nameLabels = new UILabel[]
                        {
                            new UILabel(new Vector2(center - (names[0].Length * charWidth)/2, 88 -lineH-4), false, new Color(117, 141, 145)),
                            new UILabel(new Vector2(center - (names[1].Length * charWidth)/2, 88), false, new Color(117, 141, 145)),
                            new UILabel(new Vector2(center - (names[2].Length * charWidth)/2, 88 + lineH), false, new Color(117, 141, 145))
                        };

                        for (int i = 0; i < names.Length; i++)
                        {
                            UILabel label = nameLabels[i];
                            label.Initialize(true);
                            label.SetText(new string(' ', names[i].Length));
                        }

                        notVisibleYet = Enumerable.Range(0, names.Length).SelectMany((i) => Enumerable.Range(0, names[i].Length).Select((j) => (i, j))).ToList();
                    })
                    .Event("DrawText", (state) =>
                    {
                        int index = GlobalState.RNG.Next(0, notVisibleYet.Count);

                        (int label, int s_index) = notVisibleYet[index];

                        UILabel l = nameLabels[label];

                        char[] text = l.Text.ToCharArray();
                        text[s_index] = names[label][s_index];

                        l.SetText(new string(text));

                        notVisibleYet.RemoveAt(index);
                    })
                    .Condition(() => AnyKeyPressed, (s) => _state.ChangeState("PressStart"))
                    .Condition(() => notVisibleYet.Count == 0, (s) => _state.ChangeState("CreditsFade"))
                .End()
                .State("CreditsFade")
                    .Update((state, t) =>
                    {
                        bool endFade = false;
                        foreach (var label in nameLabels)
                        {
                            float o = label.Opacity;

                            if (MathUtilities.MoveTo(ref o, 0.3f, 0.6f))
                            {
                                endFade = true;
                            }

                            label.Opacity = o;
                        }

                        if (endFade)
                        {
                            _state.ChangeState("CreditsFadeEnd");
                        }
                    })
                    .Condition(() => AnyKeyPressed, (s) => _state.ChangeState("PressStart"))
                .End()
                .State<TextFadeTimer>("CreditsFadeEnd")
                    .Update((state, t) =>
                    {
                        foreach (var label in nameLabels)
                        {
                            float o = label.Opacity;

                            MathUtilities.MoveTo(ref o, 0f, 2f);

                            label.Opacity = o;
                        }
                    })
                    .Condition(() => AnyKeyPressed, (s) => _state.ChangeState("PressStart"))
                    .Event("EndFade", (state) => _state.ChangeState("ScollUp"))
                .End()
                .State("ScollUp")
                    .Enter((state) =>
                    {
                        nexusImage.velocity.Y = -20;
                    })
                    .Condition(() => AnyKeyPressed || nexusImage.Position.Y + nexusImage.height <= 180, (s) => _state.ChangeState("PressStart"))
                .End()
                .State<PressEnterTimer>("PressStart")
                    .Enter((state) =>
                    {
                        GlobalState.FullscreenDarkness.Darkness = ResourceManager.GetTexture("title_overlay2");

                        nexusImage.Position.Y = 180 - nexusImage.height;
                        nexusImage.velocity.Y = 0;

                        doorGlow.Position = new Vector2((160 - 64) / 2, nexusImage.Position.Y + 17);

                        Vector2 spinPos = new Vector2(doorGlow.Position.X, nexusImage.Position.Y);
                        doorSpin1.Position = spinPos;
                        doorSpin2.Position = spinPos;

                        doorSpin1.angularVelocity = MathHelper.ToRadians(90);
                        doorSpin2.angularVelocity = MathHelper.ToRadians(-90);

                        doorGlow.visible = true;
                        doorSpin1.visible = true;
                        doorSpin2.visible = true;

                        pressEnter.visible = true;
                        title.visible = true;
                        titleOverlay.visible = true;

                        GlobalState.flash.Flash(1.5f, Color.White);
                    })
                    .Update((state, t) =>
                    {
                        MathUtilities.MoveTo(ref titleOverlay.opacity, 0, 0.4f);
                    })
                    .Event("BlinkEnter", (state) => pressEnter.visible = !pressEnter.visible)
                    .Condition(() => AnyKeyPressed, (s) =>
                    {
                        GlobalState.FullscreenDarkness.ForceAlpha(0);
                        ChangeStateEvent(AnodyneGame.GameState.Game);
                    })
                .End()
                .Build();

            _state.ChangeState("IntroFade");
        }

        public override void Create()
        {
            base.Create();

            _background = new ScrollingTex("title_bg", new Vector2(0, -30), DrawOrder.BACKGROUND);

            nexusImage = new UIEntity(new Vector2(0, 180), "door", GameConstants.SCREEN_WIDTH_IN_PIXELS, 116, DrawOrder.MAP_BG);

            doorGlow = new UIEntity(Vector2.Zero, "door_glow", 64, 32, DrawOrder.MAP_BG2)
            {
                visible = false
            };

            doorSpin1 = new UIEntity(Vector2.Zero, "door_spinglow1", 64, 64, DrawOrder.MAP_FG)
            {
                visible = false
            };

            doorSpin2 = new UIEntity(Vector2.Zero, "door_spinglow2", 64, 64, DrawOrder.MAP_FG)
            {
                visible = false
            };

            pressEnter = new UIEntity(new Vector2((GameConstants.SCREEN_WIDTH_IN_PIXELS - 96) / 2, GameConstants.SCREEN_HEIGHT_IN_PIXELS), "press_enter", 96, 16, DrawOrder.MENUTEXT)
            {
                visible = false
            };

            title = new UIEntity(new Vector2(16), "title_text", 128, 48, DrawOrder.MENUTEXT)
            {
                visible = false
            };
            titleOverlay = new UIEntity(new Vector2(16), "title_text_white", 128, 48, DrawOrder.TEXTBOX)
            {
                visible = false
            };

            SoundManager.PlaySong("title");
        }

        public override void Update()
        {
            base.Update();

            _state.Update(GameTimes.DeltaTime);


            _background.Update();

            nexusImage.Update();
            doorGlow.Update();
            doorSpin1.Update();
            doorSpin2.Update();
            title.Update();
            titleOverlay.Update();
            pressEnter.Update();


            nexusImage.PostUpdate();
            doorGlow.PostUpdate();
            doorSpin1.PostUpdate();
            doorSpin2.PostUpdate();
            title.PostUpdate();
            titleOverlay.PostUpdate();
            pressEnter.PostUpdate();
        }

        public override void DrawUI()
        {
            base.DrawUI();

            _background.DrawUI();

            nexusImage.Draw();
            doorGlow.Draw();
            doorSpin1.Draw();
            doorSpin2.Draw();
            title.Draw();
            titleOverlay.Draw();
            pressEnter.Draw();

            foreach (var label in nameLabels)
            {
                label.Draw();
            }
        }
    }
}
