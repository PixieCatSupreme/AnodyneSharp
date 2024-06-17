using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.RedSea
{
    [NamedEntity("Redsea_NPC", null, 10), Collision(typeof(Player))]
    public class BombDude : Entity, Interactable
    {
        private EntityPreset _preset;
        private Player _player;

        public const string DamageDealer = "Bomb dude";

        public BombDude(EntityPreset preset, Player p)
            : base(preset.Position, new AnimatedSpriteRenderer("redsea_npcs", 16, 16, new Anim("walk", new int[] { 10, 11 }, 4)), DrawOrder.ENTITIES)
        {
            _preset = preset;
            _player = p;

            immovable = true;

            DialogueManager.SetSceneProgress("generic_npc", "bomb", 0);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (DialogueManager.IsSceneFinished("generic_npc", "bomb"))
            {
                GlobalState.SpawnEntity(new Explosion(this));

                _player.ReceiveDamage(1, DamageDealer);

                exists = _preset.Alive = false;
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "bomb");
            }

            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            Separate(this, other);
        }
    }
}
