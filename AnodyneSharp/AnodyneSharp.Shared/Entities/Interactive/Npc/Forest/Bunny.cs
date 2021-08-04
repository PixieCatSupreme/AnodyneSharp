using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Forest
{
    class BaseBunny : Entity
    {

        public BaseBunny(EntityPreset preset, Player p)
            : base(preset.Position, "forest_npcs", 16, 16, DrawOrder.ENTITIES)
        {
            AddAnimation("walk_d", CreateAnimFrameArray(30, 31), 4, true);
            AddAnimation("walk_r", CreateAnimFrameArray(32, 33), 4, true);
            AddAnimation("walk_u", CreateAnimFrameArray(34, 35), 4, true);
            AddAnimation("walk_l", CreateAnimFrameArray(36, 37), 4, true);
            Play("walk_d");

            immovable = true;
        }
    }

    [NamedEntity("Forest_NPC", null, 30), Collision(typeof(Player))]

    class Bunny : BaseBunny, Interactable
    {
        private Player _player;

        public Bunny(EntityPreset preset, Player p)
            : base(preset, p)
        {
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

    class BunnyRun : BaseBunny
    {

        private Player _player;

        public BunnyRun(EntityPreset preset, Player p)
            : base(preset, p)
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
                Rectangle screen = new(MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid.ToPoint()).ToPoint(), new(160, 160));
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