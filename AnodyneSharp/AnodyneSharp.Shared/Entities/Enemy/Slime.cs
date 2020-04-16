using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy
{
    [NamedEntity, Collision(typeof(Player),typeof(Broom),MapCollision = true)]
    public class Slime : Entity
    {
        public Slime(EntityPreset preset) : base(preset.Position, 16,16, Drawing.DrawOrder.ENTITIES)
        {
            SetTexture("slime");

            AddAnimation("Move", CreateAnimFrameArray(0, 1), 3);
            AddAnimation("Hurt", CreateAnimFrameArray(0, 8), 15);
            AddAnimation("Dead", CreateAnimFrameArray(0, 8, 0, 8, 15, 9, 9), 12, false);

            Play("Move");
        }

        public override void Collided(Entity other)
        {
            //Do something sensible, like making the player take damage
            Separate(this, other); //This line, combined with no AI for the slime makes it a pinball bc velocity isn't reset
        }
    }
}