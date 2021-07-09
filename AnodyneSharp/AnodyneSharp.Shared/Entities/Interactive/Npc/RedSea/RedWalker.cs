using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.RedSea
{
    [NamedEntity("Red_Walker"), Collision(typeof(Player), KeepOnScreen = true)]
    public class RedWalker : Entity
    {

        public RedWalker(EntityPreset preset, Player p)
            : base(preset.Position, "redwalker", 32, 48, DrawOrder.ENTITIES)
        {
            int startFrame = GlobalState.RNG.Next(0, 5);

            AddAnimation("walk", CreateAnimFrameArray(startFrame, (startFrame + 1) % 5, (startFrame + 2) % 5, (startFrame + 3) % 5, (startFrame + 4) % 5), 5);
            Play("walk");

            velocity.X = 5 + GlobalState.RNG.Next(0, 15);

            width = 20;
            height = 8;

            offset = new Microsoft.Xna.Framework.Vector2(4, 40);
            Position += offset;

            immovable = true;
        }

        public override void Update()
        {
            base.Update();

            if (touching != Touching.NONE)
            {
                velocity *= -1;

                if (_flip == SpriteEffects.None)
                {
                    _flip = SpriteEffects.FlipHorizontally;
                }
                else
                {
                    _flip = SpriteEffects.None;
                }
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            Separate(this, other);
            touching = Touching.NONE;
        }
    }
}
