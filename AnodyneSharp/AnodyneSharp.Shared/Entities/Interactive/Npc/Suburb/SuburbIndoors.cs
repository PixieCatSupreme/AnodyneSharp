using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Suburb
{
    [Collision(typeof(Player))]
    public class SuburbIndoors : Entity, Interactable
    {
        protected string _dialogue = "";
        protected int _line = -1;

        public SuburbIndoors(EntityPreset preset, AnimatedSpriteRenderer sprite) : base(preset.Position,sprite,Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (CurAnimName.StartsWith("walk"))
            {
                FaceTowards(Position - FacingDirection(player_direction));

                PlayFacing("walk");
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("suburb_walker", _dialogue, _line);

            return true;

        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }
    }

    [NamedEntity("Suburb_Indoors", null, 6)]
    public class SuburbHanged : SuburbIndoors
    {
        Entity[] rope = new Entity[3];
        public SuburbHanged(EntityPreset preset, Player p) : base(preset, new AnimatedSpriteRenderer("suburb_walkers",16,16,new Anim("hanged",new int[] { 54 },1)))
        {
            _dialogue = "hanged";
            for (int i = 0; i < rope.Length; i++)
            {
                Entity r = new(new Vector2(Position.X, Position.Y - 16 * (i + 1)), new AnimatedSpriteRenderer("suburb_walkers", 16, 16, new Anim("a",new int[] { 55 },1)), Drawing.DrawOrder.ENTITIES);
                rope[i] = r;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return rope;
        }
    }

    [NamedEntity("Suburb_Indoors",null,4,7)]
    public class SuburbDead : SuburbIndoors
    {
        public SuburbDead(EntityPreset preset, Player p) : base(preset, new AnimatedSpriteRenderer("suburb_walkers",16,16,new Anim("dead",new int[] {preset.Frame == 4 ? 17 : 8},1)))
        {
            _line = preset.Frame == 4 ? 0 : 1;
            _dialogue = "dead";
        }
    }

    [NamedEntity("Suburb_Indoors",null,0,1,2,3,5)]
    public class SuburbAlive : SuburbIndoors
    {
        static int Offset(int frame) => frame switch { 
            0 => 0,
            1 => 4,
            2 => 1,
            3 => 2,
            _ => 0
        } * 9;

        public static AnimatedSpriteRenderer GetSprite(int off) => new("suburb_walkers", 16, 16,
            new Anim("walk_d", new int[] { 0 + off, 1 + off }, 4),
            new Anim("walk_r", new int[] { 2 + off, 3 + off }, 4),
            new Anim("walk_u", new int[] { 4 + off, 5 + off }, 4),
            new Anim("walk_l", new int[] { 6 + off, 7 + off }, 4)
            );

        public SuburbAlive(EntityPreset preset, Player p) : base(preset, GetSprite(Offset(preset.Frame)))
        {
            switch (preset.Frame)
            {
                case 0:
                    _dialogue = "paranoid";
                    break;
                case 1:
                    _dialogue = "family";
                    _line = 1;
                    break;
                case 2:
                    _dialogue = "family";
                    _line = 0;
                    break;
                case 3:
                    _dialogue = "older_kid";
                    break;
                case 5:
                    _dialogue = "festive";
                    break;
                default:
                    DebugLogger.AddWarning($"Missing dialog for npc with frame {preset.Frame}.");
                    break;
            }
        }
    }

}
