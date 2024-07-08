using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Happy
{
    [NamedEntity("Happy_NPC"), Collision(typeof(Player), MapCollision = true)]
    public class HappyNPC : Entity, Interactable
    {
        private enum NpcType
        {
            Pace,
            Run
        }

        private const float _walkWimerMax = 1;

        private string _dialogue;

        private NpcType _npcType;

        private float _walkTimer;
        private int _ctr;

        private bool _talking;

        static int AnimOffset(int frame) => frame switch
        {
            0 => 9,
            1 => 9,
            2 => 0,
            3 => 9,
            _ => 0
        };

        public static AnimatedSpriteRenderer GetSprite(int start) => new("happy_npcs", 16, 16,
            new Anim("idle_d", new int[] { start + 0 }, 4),
            new Anim("idle_r", new int[] { start + 2 }, 4),
            new Anim("idle_u", new int[] { start + 4 }, 4),
            new Anim("idle_l", new int[] { start + 6 }, 4),
            new Anim("walk_d", new int[] { start + 0, start + 1 }, 4),
            new Anim("walk_r", new int[] { start + 2, start + 3 }, 4),
            new Anim("walk_u", new int[] { start + 4, start + 5 }, 4),
            new Anim("walk_l", new int[] { start + 6, start + 7 }, 4)
            );

        public HappyNPC(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(AnimOffset(preset.Frame)), Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;

            switch (preset.Frame)
            {
                case 0:
                    _dialogue = "hot";
                    _npcType = NpcType.Pace;
                    break;
                case 1:
                    _dialogue = "dump";
                    _npcType = NpcType.Run;
                    break;
                case 2:
                    _dialogue = "gold";
                    _npcType = NpcType.Run;
                    break;
                case 3:
                    _dialogue = "drink";
                    _npcType = NpcType.Run;
                    break;
                case 4:
                    _dialogue = "beautiful";
                    _npcType = NpcType.Pace;
                    break;
            }
        }

        public override void Update()
        {
            base.Update();

            _walkTimer += GameTimes.DeltaTime;

            if (_talking)
            {
                velocity.X = 0;
                if (GlobalState.LastDialogueFinished)
                {
                    _talking = false;
                }
                else
                {
                    return;
                }
            }

            if (_walkTimer > _walkWimerMax)
            {
                _walkTimer = 0;

                switch (_npcType)
                {
                    case NpcType.Pace:

                        if (GlobalState.RNG.Next(0, 2) == 0)
                        {
                            if (_ctr == 0)
                            {
                                _ctr = 1;
                                velocity.X = 20;
                                Play("walk_r");
                            }
                            else
                            {
                                _ctr = 0;
                                velocity.X = -20;
                                Play("walk_l");
                            }
                        }
                        else
                        {
                            velocity.X = 0;
                            RandomIdle();
                        }
                        break;
                    case NpcType.Run:
                        if (_ctr == 0)
                        {
                            Play("walk_r");
                            velocity.X = 40;
                            _ctr = 1;
                        }
                        else
                        {
                            Play("walk_l");
                            velocity.X = -40;
                            _ctr = 0;
                        }
                        break;
                }
            }


        }

        public bool PlayerInteraction(Facing player_direction)
        {
            FaceTowards(Position - FacingDirection(player_direction));
            PlayFacing("idle");

            GlobalState.Dialogue = DialogueManager.GetDialogue("happy_npc", _dialogue);

            _talking = true;

            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        private void RandomIdle()
        {
            facing = (Facing)GlobalState.RNG.Next(0, 5);
        }
    }
}
