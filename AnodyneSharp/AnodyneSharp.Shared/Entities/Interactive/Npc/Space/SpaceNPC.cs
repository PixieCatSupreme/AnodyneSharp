using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Space
{
    [NamedEntity("Space_NPC"), Collision(typeof(Player))]
    class SpaceNPC : Entity, Interactable
    {
        private string _dialogue;

        public SpaceNPC(EntityPreset preset, Player p)
            : base(preset.Position, "space_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;

            switch (preset.Frame)
            {
                case 0:
                    _dialogue = "gray1";
                    SetWalkAnims(0);
                    break;
                case 1:
                    _dialogue = "gray2";
                    SetWalkAnims(0);
                    break;
                case 3:
                    _dialogue = "gray3";
                    SetWalkAnims(0);
                    break;
                case 10:
                    _dialogue = "color1";
                    SetWalkAnims(10);
                    break;
                case 11:
                    _dialogue = "color2";
                    SetWalkAnims(10);
                    break;
                case 12:
                    _dialogue = "color3";
                    SetWalkAnims(10);
                    break;
                case 2:
                    _dialogue = "grayspin";

                    AddAnimation("spin", CreateAnimFrameArray(0, 2, 4, 6), 10);
                    Play("spin");
                    break;
                case 8:
                    _dialogue = "colordead";

                    SetFrame(8);
                    break;
                case 18:
                    _dialogue = "graydead";

                    SetFrame(18);
                    break;
            }
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (CurAnimName != "forcedFrame" && CurAnimName != "spin")
            {
                FaceTowards(Position - FacingDirection(player_direction));

                PlayFacing("walk");
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("geoms", _dialogue);

            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        private void SetWalkAnims(int start)
        {
            AddAnimation("walk_d", CreateAnimFrameArray(start +0, start +1), 4);
            AddAnimation("walk_r", CreateAnimFrameArray(start +2, start +3), 4);
            AddAnimation("walk_u", CreateAnimFrameArray(start +4, start +5), 4);
            AddAnimation("walk_l", CreateAnimFrameArray(start +6, start +7), 4);

            Play("walk_d");
        }
    }
}
