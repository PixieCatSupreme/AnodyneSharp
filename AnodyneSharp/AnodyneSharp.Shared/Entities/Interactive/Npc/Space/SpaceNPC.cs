using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Space
{
    [NamedEntity("Space_NPC"), Collision(typeof(Player))]
    class SpaceNPC : Entity, Interactable
    {
        private string _dialogue;

        public static AnimatedSpriteRenderer GetSprite(int start) => new("space_npcs", 16, 16,
            new Anim("walk_d", new int[] { start + 0, start + 1 }, 4),
            new Anim("walk_d", new int[] { start + 2, start + 3 }, 4),
            new Anim("walk_d", new int[] { start + 4, start + 5 }, 4),
            new Anim("walk_d", new int[] { start + 6, start + 7 }, 4),
            new Anim("spin", new int[] { start + 0, start + 2, start + 4, start + 6 }, 10),
            new Anim("dead", new int[] { start + 8 }, 1)
            );

        public SpaceNPC(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(preset.Frame >= 10 ? 10 : 0), Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;

            switch (preset.Frame)
            {
                case 0:
                    _dialogue = "gray1";
                    break;
                case 1:
                    _dialogue = "gray2";
                    break;
                case 3:
                    _dialogue = "gray3";
                    break;
                case 10:
                    _dialogue = "color1";
                    break;
                case 11:
                    _dialogue = "color2";
                    break;
                case 12:
                    _dialogue = "color3";
                    break;
                case 2:
                    _dialogue = "grayspin";
                    Play("spin");
                    break;
                case 8:
                    _dialogue = "colordead";

                    Play("dead");
                    break;
                case 18:
                    _dialogue = "graydead";

                    Play("dead");
                    break;
            }
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (CurAnimName.StartsWith("walk"))
            {
                FaceTowards(Position - FacingDirection(player_direction));

                PlayFacing("walk");
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("geoms", _dialogue);

            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }
    }
}
