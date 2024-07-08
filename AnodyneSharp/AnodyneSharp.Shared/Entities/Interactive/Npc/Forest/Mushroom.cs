using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Forest
{
    [NamedEntity("Forest_NPC", null, 20), Collision(typeof(Player), typeof(Broom))]
    public class Mushroom : Entity
    {
        public static AnimatedSpriteRenderer GetSprite() => new("forest_npcs", 16, 16,
            new Anim("idle", new int[] { 20 },1),
            new Anim("move", new int[] { 20, 21, 20, 22, 20, 21, 20, 22, 20 },8,false)
            );

        public Mushroom(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), DrawOrder.ENTITIES)
        {
            immovable = true;
        }

        public override void Update()
        {
            base.Update();

            if (CurAnimName == "move" && AnimFinished)
            {
                Play("idle");
            }
        }


        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (other is Broom)
            {
                if (CurAnimName != "move")
                {
                    Play("move");

                    SoundManager.PlaySoundEffect("cross2", "cross3", "cross4");
                }
            }
            else
            {
                Separate(this, other);
            }
        }
    }
}
