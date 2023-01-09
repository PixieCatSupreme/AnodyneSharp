using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Interactive.Npc.QuestNPCs
{
    [NamedEntity("NPC", type: "generic", 0), Collision(typeof(Player))]
    class SuburbBlocker : Entity, Interactable
    {
        EntityPreset _preset;

        bool dying;

        public static AnimatedSpriteRenderer GetSprite() => new("suburb_walkers", 16, 16,
            new Anim("walk_d", new int[] { 0, 1 }, 4),
            new Anim("walk_r", new int[] { 2, 3 }, 4),
            new Anim("walk_u", new int[] { 4, 5 }, 4),
            new Anim("walk_l", new int[] { 6, 7 }, 4)
            );

        public SuburbBlocker(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;

            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public override void Update()
        {
            base.Update();

            if (dying)
            {
                if (MathUtilities.MoveTo(ref opacity, 0, 0.6f))
                {
                    exists = false;
                }
            }
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (!dying)
            {
                FaceTowards(Position - FacingDirection(player_direction));

                PlayFacing("walk");
                var dialog = GlobalState.events.GetEvent("SuburbKilled") switch
                {
                    < 6 => 0,
                    < 9 => 1,
                    < 10 => 2,
                    _ => 3,
                };

                GlobalState.Dialogue = DialogueManager.GetDialogue("suburb_blocker", "one", dialog);

                if (dialog == 3)
                {
                    Flicker(0.5f);

                    dying = true;
                    _preset.Alive = false;
                }

                return true;
            }

            return false;
        }
    }
}
