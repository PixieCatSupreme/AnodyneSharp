using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Blank
{

    //This is just the Gadget console again. Yes this allows you to open the door without usiong the button in Melos' room
    [NamedEntity("Console", null, "DEBUG", 0), Collision(typeof(Player))]
    class DebugConsole : Entity, Interactable
    {
        EntityPreset _preset;

        public DebugConsole(EntityPreset preset, Player p) : base(preset.Position, "console", 16, 16, DrawOrder.ENTITIES)
        {
            _preset = preset;
            immovable = true;

            AddAnimation("active", CreateAnimFrameArray(0, 1), 5);
            AddAnimation("green", CreateAnimFrameArray(2), 3);

            if (preset.Activated)
            {
                Play("green");
                GlobalState.PUZZLES_SOLVED++;
            }
            else
            {
                Play("active");
            }
        }

        public override void Collided(Entity other)
        {
            Separate(other, this);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (!_preset.Activated)
            {
                _preset.Activated = true;
                Play("green");
                GlobalState.PUZZLES_SOLVED++;
                SoundManager.PlaySoundEffect("get_small_health");
                return true;
            }
            return false;
        }
    }
}