using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Forest
{
    [NamedEntity("Forest_NPC", null, 0), Collision(typeof(Player))]
    public class Thorax : Entity, Interactable
    {
        private Player _player;

        public static AnimatedSpriteRenderer GetSprite() => new("forest_npcs", 16, 16,
            new Anim("move", new int[] { 0, 1 },4),
            new Anim("stand", new int[] { 1 },1),
            new Anim("squat", new int[] { 0 },1)
            );

        public Thorax(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), DrawOrder.ENTITIES)
        {
            _player = p;

            immovable = true;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("forest_npc", "thorax");

            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            Separate(this, other);
        }
    }
}
