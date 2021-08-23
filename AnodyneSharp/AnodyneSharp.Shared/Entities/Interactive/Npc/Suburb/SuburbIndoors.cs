using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Suburb
{
    [NamedEntity("Suburb_Indoors"), Collision(typeof(Player))]
    public class SuburbIndoors : Entity, Interactable
    {
        string _dialogue;
        int _line;

        Entity[] _rope;

        public SuburbIndoors(EntityPreset preset, Player p)
            : base(preset.Position, "suburb_walkers", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;

            int off = 0;

            string anim = "walk_d";

            _dialogue = "";
            _line = -1;

            _rope = new Entity[3];

            switch (preset.Frame)
            {
                case 0:
                    off = 0;
                    _dialogue = "paranoid";
                    break;
                case 1:
                    off = 4;
                    _dialogue = "family";
                    _line = 1;
                    break;
                case 2:
                    off = 1;
                    _dialogue = "family";
                    _line = 0;
                    break;
                case 3:
                    off = 2;
                    _dialogue = "older_kid";
                    break;
                case 4:
                    off = 1;
                    anim = "dead";
                    _dialogue = "dead";
                    _line = 0;
                    break;
                case 5:
                    off = 0;
                    _dialogue = "festive";
                    break;
                case 6:
                    off = 0;
                    anim = "hang";
                    _dialogue = "hanged";

                    for (int i = 0; i < _rope.Length; i++)
                    {
                        Entity r = new(new Vector2(Position.X, Position.Y - 16 * (i + 1)), "suburb_walkers", 16, 16, Drawing.DrawOrder.ENTITIES);
                        r.SetFrame(55);
                        _rope[i] = r;
                    }

                    break;
                case 7:
                    off = 0;
                    anim = "dead";
                    _dialogue = "dead";
                    _line = 1;
                    break;
            }

            off *= 9;

            AddAnimation("walk_d", CreateAnimFrameArray(0 + off, 1 + off), 4);
            AddAnimation("walk_r", CreateAnimFrameArray(2 + off, 3 + off), 4);
            AddAnimation("walk_u", CreateAnimFrameArray(4 + off, 5 + off), 4);
            AddAnimation("walk_l", CreateAnimFrameArray(6 + off, 7 + off), 4);
            AddAnimation("hang", CreateAnimFrameArray(54));
            AddAnimation("dead", CreateAnimFrameArray(8 + off));

            Play(anim);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (_curAnim.name != "hang" && _curAnim.name != "dead")
            {
                FaceTowards(Position - FacingDirection(player_direction));

                PlayFacing("walk");
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("suburb_walker", _dialogue, _line);

            return true;

        }

        public override IEnumerable<Entity> SubEntities()
        {
            if (_rope[0] == null)
            {
                return new List<Entity>();
            }
            else
            {
                return _rope;
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }
    }
}
