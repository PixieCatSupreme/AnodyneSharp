using AnodyneSharp.Dialogue;
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

        public SuburbBlocker(EntityPreset preset, Player p)
            : base(preset.Position, "suburb_walkers", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;

            AddAnimation("walk_d", CreateAnimFrameArray(0, 1), 4, true);
            AddAnimation("walk_r", CreateAnimFrameArray(2, 3), 4, true);
            AddAnimation("walk_u", CreateAnimFrameArray(4, 5), 4, true);
            AddAnimation("walk_l", CreateAnimFrameArray(6, 7), 4, true);
            Play("walk_d");

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
                    _preset.Alive = false;
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
                }

                return true;
            }

            return false;
        }
    }
}
