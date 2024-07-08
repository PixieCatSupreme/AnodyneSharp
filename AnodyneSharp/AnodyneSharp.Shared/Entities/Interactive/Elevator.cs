using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using AnodyneSharp.States.MenuSubstates;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Interactive
{
    [NamedEntity]
    public class Elevator : Entity
    {
        //Slight abuse of update order with subentities getting updated after the parent one
        PlayerDetector openDetector;
        PlayerDetector menuDetector;

        ElevatorMenu menu;
        Entity elevator_frame;

        IState _state;

        public static AnimatedSpriteRenderer GetSprite() => new("elevator_doors", 32, 32,
            new Anim("default", new int[] { 0 }, 1),
            new Anim("open", new int[] { 0, 1, 2, 3 }, 12, false),
            new Anim("close", new int[] { 3, 2, 1, 0 }, 12, false)
            );

        public Elevator(EntityPreset preset, Player p) : base(preset.Position, GetSprite(), DrawOrder.ENTITIES)
        {
            menu = new(preset);
            elevator_frame = new(preset.Position, new StaticSpriteRenderer("elevator", 32, 32, preset.Frame), DrawOrder.ENTITIES);

            openDetector = new(new((Position + Vector2.UnitY * 32).ToPoint(), new(width, height)));
            menuDetector = new(new(Position.ToPoint(), new(32, 20)));

            _state = new StateMachineBuilder()
                .State("Close")
                    .Enter((s) => { if (Frame != 0) { Play("close"); SoundManager.PlaySoundEffect("elevator_close"); } })
                    .Condition(() => openDetector.Hit, (s) => _state.ChangeState("Open"))
                    .Condition(() => menuDetector.Hit, (s) => _state.ChangeState("Menu"))
                .End()
                .State("Open")
                    .Enter((s) => { if (CurAnimName != "open") { Play("open"); SoundManager.PlaySoundEffect("elevator_open"); } })
                    .Condition(() => !openDetector.Hit, (s) => _state.ChangeState("Close"))
                .End()
                .State("Menu")
                    .Enter((s) =>
                    {
                        menu.Exit = false;
                        GlobalState.SetSubstate(menu);
                        menu.GetControl();
                    })
                    .Condition(() => menu.Exit, (s) =>
                       {
                           if (menu.chosen != null)
                           {
                               Warp(menu.chosen);
                               _state.PopState();
                           }
                           else
                           {
                               _state.ChangeState("Leaving");
                           }
                       })
                .End()
                .State("Leaving")
                    .Enter((s) => { Play("open"); SoundManager.PlaySoundEffect("elevator_open"); })
                    .Condition(() => !menuDetector.Hit, (s) => _state.ChangeState("Open"))
                .End()
            .Build();
            _state.ChangeState("Close");
        }

        void Warp(EntityPreset next)
        {
            GlobalState.PLAYER_WARP_TARGET = next.Position + new Vector2(8, 30);
            GlobalState.NEXT_MAP_NAME = GlobalState.CURRENT_MAP_NAME;
            GlobalState.WARP = true;
        }

        public override void Update()
        {
            base.Update();
            _state.Update(GameTimes.DeltaTime);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { menuDetector, openDetector, elevator_frame };
        }

        class ElevatorMenu : ListSubstate
        {
            Vector2 position = new(15, 40);
            Texture2D background = ResourceManager.GetTexture("pop_menu");
            UILabel title;

            EntityPreset parent_preset;

            public EntityPreset chosen = null;

            public ElevatorMenu(EntityPreset e) : base(false)
            {
                parent_preset = e;
                SetLabels();
            }

            protected override void SetLabels()
            {
                var others = EntityManager.GetLinkGroup(parent_preset.LinkID).Where(e => e.LinkID != parent_preset.LinkID).ToList();
                others.Sort((a, b) => a.Frame.CompareTo(b.Frame));

                float lineHeight = GameConstants.FONT_LINE_HEIGHT * 1.3f;
                float yOffset = background.Height / 2 - (lineHeight * (others.Count + 2)) / 2;

                title = CenterLabel(new(Vector2.UnitY * yOffset, true, DialogueManager.GetDialogue("misc", "any", "elevator", 0)));
                options = new();
                for (int i = 0; i < others.Count; ++i)
                {
                    EntityPreset current = others[i];
                    options.Add((CenterLabel(new(Vector2.UnitY * (yOffset + (i + 1) * lineHeight), true, $"{current.Frame + 1}")),
                        new ActionOption(() => { chosen = current; Exit = true; })));
                }
                options.Add((
                    CenterLabel(new(Vector2.UnitY * (yOffset + (others.Count + 1) * lineHeight), true, DialogueManager.GetDialogue("misc", "any", "elevator", 1))),
                    new ActionOption(() => Exit = true)));
            }

            public override void Update()
            {
                base.HandleInput();
                base.Update();
            }

            public override void DrawUI()
            {
                base.DrawUI();
                SpriteDrawer.DrawSprite(background, position, Z: DrawingUtilities.GetDrawingZ(DrawOrder.PAUSE_BG));
                title.Draw();
            }

            UILabel CenterLabel(UILabel label)
            {
                label.Position = position + new Vector2(background.Width / 2 - label.Writer.WriteArea.Width / 2, label.Position.Y);
                return label;
            }
        }

        [Collision(typeof(Player))]
        public class PlayerDetector : Entity
        {
            public bool Hit = false;

            public PlayerDetector(Rectangle r) : base(new(r.X, r.Y), r.Width, r.Height)
            {
                visible = false;
            }

            public override void Update()
            {
                base.Update();
                Hit = false;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                Hit = true;
            }
        }
    }
}
