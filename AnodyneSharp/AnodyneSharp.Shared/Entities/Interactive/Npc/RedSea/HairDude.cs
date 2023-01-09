using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.RedSea
{
    [NamedEntity("Redsea_NPC", null, 0), Collision(typeof(Player))]
    class HairDude : Entity, Interactable
    {
        private Player _player;

        public static AnimatedSpriteRenderer GetSprite() => new("redsea_npcs", 16, 16,
            new Anim("walk_d", new int[] { 0, 1 }, 4),
            new Anim("walk_r", new int[] { 2, 3 }, 4),
            new Anim("walk_u", new int[] { 4, 5 }, 4),
            new Anim("walk_l", new int[] { 6, 7 }, 4)
            );

        public HairDude(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), DrawOrder.ENTITIES)
        {
            _player = p;

            immovable = true;
        }

        public override void Update()
        {
            base.Update();

            FaceTowards(_player.Position);

            PlayFacing("walk");
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (QuestNPCs.QuestNPCHelper.NeedsSecond("hairdude","first","second"))
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "second");
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "first");
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