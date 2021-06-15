using AnodyneSharp.Dialogue;
using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.RunningTradeNPCs
{
    public class MiaoGraphics : Entity
    {
        public MiaoGraphics(Vector2 position) : base(position, "fields_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("walk_d", CreateAnimFrameArray(0, 1), 4);
            AddAnimation("walk_r", CreateAnimFrameArray(2, 3), 4);
            AddAnimation("walk_u", CreateAnimFrameArray(4, 5), 4);
            AddAnimation("walk_l", CreateAnimFrameArray(6, 7), 4);
            width = height = 12;
            offset.X = offset.Y = 2;
            Play("walk_d");
        }
    }

    [NamedEntity("Trade_NPC", null, 0), Collision(typeof(Player))]
    public class MiaoXiao : MiaoGraphics, Interactable
    {
        MiaoXiaoFollower follower;

        public MiaoXiao(EntityPreset preset, Player p) : base(preset.Position)
        {
            immovable = true;
            follower = new(preset.Position, p);
            if(p.follower != null)
            {
                exists = false;
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(follower, 1);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            //TODO: philosophy talk after monster spooked
            //Ask to shadow, then swap places
            GlobalState.Dialogue = DialogueManager.GetDialogue("miao", "init");
            exists = false;
            follower.Activate();
            return true;
        }
    }

    //Follower logic
    [Collision(MapCollision = true), Events(typeof(StartWarp),typeof(EndScreenTransition))]
    class MiaoXiaoFollower : MiaoGraphics
    {
        Player follow;

        Entity dust = new(Vector2.Zero, "dust", 16, 16, Drawing.DrawOrder.BG_ENTITIES);

        public MiaoXiaoFollower(Vector2 pos, Player p) : base(pos)
        {
            follow = p;
            dust.AddAnimation("poof", CreateAnimFrameArray(0, 1, 2, 3, 4), 13, false);
            dust.AddAnimation("unpoof", CreateAnimFrameArray(3, 2, 1, 0), 13, false);
            dust.exists = false;

            exists = false;
        }
        
        public void Activate()
        {
            exists = true;
            follow.follower = this;
        }

        public override void Update()
        {
            base.Update();
            
            Vector2 offset = follow.Center - Center;
            if(offset.LengthSquared() > 16*16)
            {
                offset.Normalize();
                velocity = offset * 70;
            }
            else
            {
                velocity = Vector2.Zero;
            }
        }

        public override void OnEvent(GameEvent e)
        {
            //TODO: check bounds of walkable grid
            if(e is StartWarp)
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("miao", "randoms", 4);
                exists = false;
                dust.exists = false;
            }
        }

    }
}
