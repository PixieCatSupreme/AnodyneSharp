using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Space
{
    [NamedEntity("CubeKing"), Collision(typeof(Player))]
    public class CubeKing : Entity, Interactable
    {
        private string _dialogue;

        public static AnimatedSpriteRenderer GetSprite(int frame) => new("space_npcs", 32, 32,
            new Anim("a",new int[] { frame, frame + 1 },4)
            );

        public CubeKing(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(preset.Frame), Drawing.DrawOrder.ENTITIES)
        {
            if (preset.Frame == 10)
            {
                _dialogue = "gray";
            }
            else
            {
                _dialogue = "color";
            }

            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("cube_king", _dialogue);

            return true;
        }
    }
}
