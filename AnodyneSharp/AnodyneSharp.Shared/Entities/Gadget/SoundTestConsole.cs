using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnodyneSharp.Drawing;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using AnodyneSharp.States.MenuSubstates;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity(xmlName: "Console", type: null, frames: 1), Collision(typeof(Player))]
    public class SoundTestConsole : Entity, Interactable
    {
        EntityPreset _preset;
        SoundTest menu;

        public SoundTestConsole(EntityPreset preset, Player p) 
            : base(preset.Position, "console", 16, 16, DrawOrder.ENTITIES)
        {
            menu = new(preset);

            _preset = preset;
            immovable = true;

            AddAnimation("spaz", CreateAnimFrameArray(0, 1, 2), 20);

            Play("spaz");
        }

        public override void Collided(Entity other)
        {
            Separate(other, this);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            menu.Exit = false;
            GlobalState.SetSubstate(menu);
            menu.GetControl();

            return false;
        }

        class SoundTest : ListSubstate
        {
            enum State
            {
                JukeBox,
                Music,
                SFX
            }

            private Vector2 position;
            private Texture2D background;
            private UILabel title;

            private EntityPreset parentPreset;

            private State jukeboxState;

            private string[] soundOptions;
            private Dictionary<string, SFXLimiter> sfx;

            private int selection;

            public SoundTest(EntityPreset e) : base(false)
            {
                jukeboxState = State.JukeBox;

                position = new(2, 20 + (160 - 29) / 2);
                background = ResourceManager.GetTexture("dialogue_box");

                sfx = ResourceManager.GetSFX();

                parentPreset = e;
                SetLabels();
            }
            protected override void SetLabels()
            {
                state = 0;

                switch (jukeboxState)
                {
                    case State.JukeBox:
                        playSfx = true;
                        JukeboxMenu();
                        break;
                    case State.Music:
                        MusicMenu();
                        break;
                    case State.SFX:
                        playSfx = false;
                        SfxMenu();
                        break;
                    default:
                        break;
                }
            }

            public override void Update()
            {
                switch (jukeboxState)
                {
                    case State.JukeBox:
                        base.HandleInput();
                        break;
                    case State.Music:
                    case State.SFX:
                        bool selectionChanged = false;

                        if (KeyInput.JustPressedRebindableKey(KeyFunctions.Left))
                        {
                            if (selection == 0)
                            {
                                selection = soundOptions.Length - 1;
                            }
                            else
                            {
                                selection--;
                            }

                            selectionChanged = true;
                        }
                        else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
                        {
                            if (selection == soundOptions.Length - 1)
                            {
                                selection = 0;
                            }
                            else
                            {
                                selection++;
                            }

                            selectionChanged = true;

                        }
                        else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                        {
                            Select();
                        }
                        else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
                        {
                            jukeboxState = State.JukeBox;
                            SetLabels();
                            selector.visible = true;
                        }

                        if (selectionChanged)
                        {
                            options[0].label.SetText(soundOptions[selection]);
                        }

                        break;
                    default:
                        break;
                }

                base.Update();
            }

            public override void DrawUI()
            {
                base.DrawUI();
                SpriteDrawer.DrawGuiSprite(background, position, Z: DrawingUtilities.GetDrawingZ(DrawOrder.PAUSE_BG));
                title.Draw();
            }

            private void JukeboxMenu()
            {
                float lineHeight = GameConstants.FONT_LINE_HEIGHT + 4;

                soundOptions = null;
                selection = -1;

                title = new UILabel(position + new Vector2(3, 1), true, "Jukebox!");

                options = new();
                options.Add((
                    new UILabel(position + new Vector2(16, lineHeight), true, "Muzak"),
                    new ActionOption(() => { jukeboxState = State.Music; SetLabels(); })));
                options.Add((
                    new UILabel(position + new Vector2(20, lineHeight * 2), true, "SFX"),
                    new ActionOption(() => { jukeboxState = State.SFX; SetLabels(); })));
            }

            private void MusicMenu()
            {
                float lineHeight = GameConstants.FONT_LINE_HEIGHT + 4;

                soundOptions = new string[] {
                    "Title",
                    "Blank", "Nexus", "Street",
                    "Overworld", "Bedroom", "Bedroom Boss",
                    "Mitra", "Fields",
                    "Beach", "Redsea", "Sacrificial",
                    "Forest", "Cliff",
                    "Redcave", "Redcave Boss",
                    "Crowd", "Crowd Boss",
                    "Windmill", "Suburb",
                    "Space", "Apartment", "Apartment Boss",
                    "Roof", "Hotel", "Hotel Boss",
                    "Cell", "Circus", "Circus Boss",
                    "Pre-Terminal", "Sagefight", "Terminal",
                    "Go", "Happy Init", "Happy", "Blue",
                    "Briar Fight", "Ending",
                    "Gameover", "Trailer"};


                selection = 0;

                title = new UILabel(position + new Vector2(3, 1), true, "MUSIC");

                UILabel label = new UILabel(position + new Vector2(3, lineHeight), true, soundOptions[0], forceEnglish: true);

                options = new();
                options.Add((
                    label,
                    new ActionOption(() => DoMusic(label.Text))));
            }

            private void SfxMenu()
            {
                float lineHeight = GameConstants.FONT_LINE_HEIGHT + 4;

                soundOptions = sfx.Keys.Select(s => s.Replace('_', ' ')).ToArray();

                selection = 0;

                title = new UILabel(position + new Vector2(3, 1), true, "SFX");

                UILabel label = new UILabel(position + new Vector2(3, lineHeight), true, soundOptions[0], forceEnglish: true);

                options = new();
                options.Add((
                    label,
                    new ActionOption(() => SoundManager.PlaySoundEffect(label.Text.Replace(' ', '_')))));
            }

            private void DoMusic(string musicId)
            {
                musicId = musicId.Replace(' ', '-').ToUpper();

                if (musicId == "HAPPY-INIT")
                {
                    musicId = "HAPPY_ALT";
                }
                else if (musicId == "PRE-TERMINAL")
                {
                    musicId = "TERMINAL_ALT";
                }
                else if (musicId == "ROOF")
                {
                    musicId = "HOTEL_ALT";
                }
                else if (musicId == "CROWD-BOSS")
                {
                    musicId = "CROWD_BOSS";
                }
                else if (musicId == "TRAILER")
                {
                    SoundManager.PlaySong("anodyne_TRAILER_1");
                    return;
                }

                SoundManager.PlaySong(musicId.ToLower());
            }
        }
    }
}
