using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Forest
{
    [NamedEntity("Forest_NPC", null, 30), Collision(typeof(Player))]
    public class Bunny : Entity, Interactable
    {
        private Player _player;

        public static AnimatedSpriteRenderer GetSprite() => new("forest_npcs", 16, 16,
            new Anim("walk_d", new int[] { 30, 31 }, 4),
            new Anim("walk_r", new int[] { 32, 33 }, 4),
            new Anim("walk_u", new int[] { 34, 35 }, 4),
            new Anim("walk_l", new int[] { 36, 37 }, 4)
            );

        public Bunny(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), DrawOrder.ENTITIES)
        {
            immovable = true;
            _player = p;
        }

        public override void Update()
        {
            base.Update();

            FaceTowards(_player.Position);

            PlayFacing("walk");
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("forest_npc", "bunny");

            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            Separate(this, other);
        }
    }

    [NamedEntity("Forest_NPC", null, 34), Collision(typeof(Player))]
    class BunnyRun : Entity
    {

        private Player _player;

        public BunnyRun(EntityPreset preset, Player p)
            : base(preset.Position,Bunny.GetSprite(),DrawOrder.ENTITIES)
        {
            if (GlobalState.RNG.NextDouble() < 0.25)
            {
                facing = (Facing)GlobalState.RNG.Next(0, 4);

                PlayFacing("walk");

                _player = p;
            }
            else
            {
                exists = false;
            }

        }

        public override void Update()
        {
            base.Update();

            if (velocity != Vector2.Zero)
            {
                Rectangle screen = new(MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid).ToPoint(), new(160, 160));
                if (!screen.Intersects(Hitbox))
                {
                    exists = false;
                }
            }
            else if ((_player.Position - Position).Length() < 48)
            {
                velocity = FacingDirection(facing) * 100;

                SoundManager.PlaySoundEffect("rat_move");
            }
        }
    }
}