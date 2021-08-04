using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Forest
{
    [NamedEntity("Black_Thing", map: "FOREST")]
    public class SkittishSecret : Entity
    {
        float yell_timer = 0.5f;
        float lol_timer = 1f;
        float initial_pos;

        Player player;

        public SkittishSecret(EntityPreset preset, Player p) : base(preset.Position, "forest_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("walk_r", CreateAnimFrameArray(32, 33), 4, true);
            Play("walk_r");

            Position.X = initial_pos = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid).X;

            player = p;
        }

        public override void Update()
        {
            base.Update();
            yell_timer -= GameTimes.DeltaTime;
            if (yell_timer < 0)
            {
                Yell();
                yell_timer = 0.5f;
            }

            lol_timer -= GameTimes.DeltaTime;
            if (lol_timer < 0)
            {
                Position.X += 1;
                lol_timer = 1f;
                if (Position.X - initial_pos > 80)
                {
                    lol_timer = 80f;
                    GlobalState.PUZZLES_SOLVED = 1;
                    if (Position.X - initial_pos > 160)
                    {
                        GlobalState.PUZZLES_SOLVED = 2;
                        exists = false;
                    }
                }
            }

            if (player.velocity != Vector2.Zero)
            {
                MathUtilities.MoveTo(ref Position.X, initial_pos-width, 60);
            }
        }

        private void Yell()
        {
            SoundManager.PlaySoundEffect("rat_move");
        }
    }
}
