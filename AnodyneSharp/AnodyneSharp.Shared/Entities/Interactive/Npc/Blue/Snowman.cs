﻿using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Blue
{
    [NamedEntity("Snowman"), AlwaysSpawn, Collision(typeof(Player))]
    class Snowman : Entity, Interactable
    {
        private EntityPreset _preset;

        private int _dialogue;

        private float _idleTimer;

        private bool _talked;

        public Snowman(EntityPreset preset, Player p)
            : base(preset.Position, "blue_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("melt", CreateAnimFrameArray(2, 3, 4, 5, 6), 7, false);
            SetFrame(0);

            _dialogue = 2 * preset.Frame;

            if (!preset.Alive)
            {
                SetFrame(6);
            }

            immovable = true;

            _preset = preset;

            _idleTimer = 3;
        }

        public override void Update()
        {
            base.Update();

            if (_talked )
            {
                if (GlobalState.LastDialogueFinished && GetFrame() != 6)
                {
                    Play("melt");
                }
            }
            else if (_preset.Alive)
            {
                _idleTimer -= GameTimes.DeltaTime;

                if (_idleTimer <= 0)
                {
                    GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "one", _dialogue+1);
                    _talked = true;
                    _preset.Alive = false;
                }
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (GetFrame() != 6)
            {
                Separate(this, other);
            }
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (!_preset.Alive )
            {
                GlobalState.Dialogue = "...";
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "one", _dialogue);
                _talked = true;
                _preset.Alive = false;
            }



            return true;
        }
    }
}