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

        private UIEntity subtitle;
        private UIEntity subtitleOverlay;

        private IState _state;

        private string[] credits;

        private UILabel[] creditsLabels;

        private List<(int, int)> notVisibleYet;

        private bool _secondNames;

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
            credits = new string[]
            {
                DialogueManager.GetDialogue("misc", "any", "title", 2),
                DialogueManager.GetDialogue("misc", "any", "title", 3),
                DialogueManager.GetDialogue("misc", "any", "title", 4)
            };

            creditsLabels = Array.Empty<UILabel>();

            _state = new StateMachineBuilder()
                .State("IntroFade")
                    .Enter((state) =>
                    {
                        GlobalState.flash.Flash(2f, Color.Black);
                        GlobalState.TitleScreenFinish.Darkness = ResourceManager.GetTexture("title_overlay1");
                        GlobalState.TitleScreenFinish.ForceAlpha(1);
                    })
                    .Condition(() => AnyKeyPressed, (s) => _state.ChangeState("PressStart"))
                    .Condition(() => !GlobalState.flash.Active(), (s) => _state.ChangeState("CreditsWrite"))
                .End()
                .State<TextDrawTimer>("CreditsWrite")
                    .Enter((state) =>
                    {
                        int lineH = GameConstants.FONT_LINE_HEIGHT + 2;
                        int center = GameConstants.SCREEN_WIDTH_IN_PIXELS / 2;
                        int charWidth = FontManager.GetCharacterWidth();
                        int charWidthEng = FontManager.GetCharacterWidth(true);

                        Color color = new Color(68, 109, 113);

                        creditsLabels = new UILabel[]
                        {
                            new UILabel(new Vector2(center - (credits[0].Length * charWidth)/2, 88 -lineH-4), false, new string(' ', credits[0].Length), color),
                            new UILabel(new Vector2(center - (credits[1].Length * charWidthEng)/2, 88), false, new string(' ', credits[1].Length), color, forceEnglish:true),
                            new UILabel(new Vector2(center - (credits[2].Length * charWidthEng)/2, 88 + lineH), false, new string(' ', credits[2].Length), color, forceEnglish:true)
                        };

                        GlobalState.TitleScreenFinish.Labels = creditsLabels.ToList();

                        notVisibleYet = Enumerable.Range(0, credits.Length).SelectMany((i) => Enumerable.Range(0, credits[i].Length).Select((j) => (i, j))).ToList();
                    })
                    .Event("DrawText", (state) =>
                    {
                        int index = GlobalState.RNG.Next(0, notVisibleYet.Count);

                        (int label, int s_index) = notVisibleYet[index];

                        UILabel l = creditsLabels[label];

                        char[] text = l.Text.ToCharArray();
                        text[s_index] = credits[label][s_index];

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
                        foreach (var label in creditsLabels)
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
                        foreach (var label in creditsLabels)
                        {
                            float o = label.Opacity;

                            MathUtilities.MoveTo(ref o, 0f, 2f);

                            label.Opacity = o;
                        }
                    })
                    .Condition(() => AnyKeyPressed, (s) => _state.ChangeState("PressStart"))
                    .Event("EndFade", (state) =>
                    {
                        if (_secondNames)
                        {
                            _state.ChangeState("ScollUp");
                        }
                        else
                        {
                            _secondNames = true;

                            credits[0] = DialogueManager.GetDialogue("misc", "any", "title", 5);
                            credits[1] = DialogueManager.GetDialogue("misc", "any", "title", 6);
                            credits[2] = DialogueManager.GetDialogue("misc", "any", "title", 7);

                            _state.ChangeState("CreditsWrite");
                        }

                    })
                .End()
                .State("ScollUp")
                    .Enter((state) =>
                    {
                        nexusImage.velocity.Y = -20;
                    })
                    .Condition(() => AnyKeyPressed || nexusImage.Position.Y + nexusImage.height <= 180, (s) => _state.ChangeState("PressStart"))
                .End()
                .State("PressStart")
                    .Enter((s) =>
                    {
                        GlobalState.flash.Flash(1.5f, Color.White, onFull: () => _state.ChangeState("DisplayTitle"));
                    })
                .End()
                .State<PressEnterTimer>("DisplayTitle")
                    .Enter((state) =>
                    {
                        foreach (var label in creditsLabels)
                        {
                            label.IsVisible = false;
                        }



                        GlobalState.TitleScreenFinish.Darkness = ResourceManager.GetTexture("title_overlay2");

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

                        subtitle.visible = true;
                        subtitleOverlay.visible = true;
                    })
                    .Update((state, t) =>
                    {
                        MathUtilities.MoveTo(ref titleOverlay.opacity, 0, 0.4f);
                        MathUtilities.MoveTo(ref subtitleOverlay.opacity, 0, 0.4f);
                    })
                    .Event("BlinkEnter", (state) => pressEnter.visible = !pressEnter.visible)
                    .Condition(() => AnyKeyPressed, (s) => _state.ChangeState("Pixelate"))
                .End()
                .State("Pixelate")
                    .Update((state, t) =>
                    {
                        GlobalState.pixelation.AddPixelation(15);
                        GlobalState.black_overlay.ChangeAlpha(0.36f);
                    })
                    .Condition(() => GlobalState.pixelation.Pixelation >= 15f, (state) => _state.ChangeState("FadeOut"))
                .End()
                .State("FadeOut")
                    .Update((state, t) =>
                    {
                        GlobalState.pixelation.AddPixelation(15);
                        GlobalState.black_overlay.ChangeAlpha(0.72f);
                    })
                    .Condition(() => GlobalState.black_overlay.alpha >= 1f, (state) =>
                    {
                        GlobalState.pixelation.SetPixelation(0);
                        GlobalState.black_overlay.alpha = 0;


                        GlobalState.flash.ForceAlpha(0);
                        GlobalState.TitleScreenFinish.ForceAlpha(0);

                        GlobalState.TitleScreenFinish.Entities.Clear();
                        GlobalState.TitleScreenFinish.Labels.Clear();

                        ChangeStateEvent(AnodyneGame.GameState.MainMenu);
                    })
                .End()
                .Build();

            _state.ChangeState("IntroFade");
        }

        public override void Create()
        {
            base.Create();

            _background = new ScrollingTex("title_bg", new Vector2(0, -30), DrawOrder.BACKGROUND);

            nexusImage = new UIEntity(new Vector2(0, 180), "door", GameConstants.SCREEN_WIDTH_IN_PIXELS, 116, DrawOrder.UI_OBJECTS);

            doorGlow = new UIEntity(Vector2.Zero, "door_glow", 64, 32, DrawOrder.MAP_BG2)
            {
                visible = false,
                LayerParent = nexusImage,
                LayerOffset = 1
            };

            doorSpin1 = new UIEntity(Vector2.Zero, "door_spinglow1", 64, 64, DrawOrder.MAP_FG)
            {
                visible = false,
                LayerParent = nexusImage,
                LayerOffset = 2
            };

            doorSpin2 = new UIEntity(Vector2.Zero, "door_spinglow2", 64, 64, DrawOrder.MAP_FG)
            {
                visible = false,
                LayerParent = nexusImage,
                LayerOffset = 2
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


            subtitle = new UIEntity(new Vector2(45, 47), "title_remake", 71, 11, DrawOrder.MENUTEXT)
            {
                visible = false
            };

            subtitleOverlay = new UIEntity(subtitle.Position, "title_remake_white", 71, 11, DrawOrder.TEXTBOX)
            {
                visible = false
            };

            //pressEnter.Draw();

            GlobalState.TitleScreenFinish.Entities.Add(pressEnter);

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

            subtitle.Draw();
            subtitleOverlay.Draw();

            //The UI labels get drawn in the TitleScreen overlay
        }
    }
}
