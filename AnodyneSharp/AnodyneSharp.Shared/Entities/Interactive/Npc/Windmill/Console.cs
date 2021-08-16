using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Interactive.Npc.Windmill
{
    [NamedEntity("Console", map: "WINDMILL")]
    class Console : Entity, Interactable
    {
        public Console(EntityPreset preset, Player p) : base(preset.Position, "windmill_inside", 48, 48, Drawing.DrawOrder.BG_ENTITIES)
        {
            AddAnimation("active", CreateAnimFrameArray(0, 1), 5);
            if (GlobalState.events.GetEvent("WindmillOpened") == 0)
            {
                Play("active");
            }
            else
            {
                SetFrame(1);
            }
            offset = Vector2.One * 16;
            Position += offset;
            width = height = 16;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (CurAnimName == "active")
            {
                GlobalState.StartCutscene = WindmillCutscene();
                SetFrame(1);
            }
            SoundManager.PlaySoundEffect("get_small_health");
            return true;
        }

        IEnumerator<CutsceneEvent> WindmillCutscene()
        {
            VolumeEvent e = new(0f, 0.6f);
            Entity statue = new(Vector2.Zero, "big_statue", 32, 48, Drawing.DrawOrder.ENTITIES)
            {
                visible = false
            };

            yield return new EntityEvent(new List<Entity>() { e, statue });

            while (!e.ReachedTarget)
            {
                yield return null;
            }

            GlobalState.gameScreenFade.fadeColor = Color.Black;
            while (!MathUtilities.MoveTo(ref GlobalState.gameScreenFade.alpha, 1, 0.6f))
            {
                yield return null;
            }

            List<(string map, Point grid, Vector2 tile)> locs = new()
            {
                ("BEDROOM", new(4, 0), new(5, 2)),
                ("REDCAVE", new(6, 2), new(4, 4)),
                ("CROWD", new(9, 4), new(4, 2))
            };

            for (int i = 0; i < 3; ++i)
            {
                var (map, grid, tile) = locs[i];
                statue.Position = MapUtilities.GetRoomUpperLeftPos(grid) + tile * 16;
                statue.SetFrame(i);
                statue.visible = true;
                yield return new WarpEvent(map, grid);

                while (!MathUtilities.MoveTo(ref GlobalState.gameScreenFade.alpha, 0.45f, 0.6f))
                {
                    yield return null;
                }
                SoundManager.PlaySoundEffect("red_cave_rise");
                while (!MathUtilities.MoveTo(ref GlobalState.gameScreenFade.alpha, 0, 0.6f))
                {
                    yield return null;
                }
                Vector2 target = statue.Position + FacingDirection(DungeonStatue.MoveDir(i)) * 32;
                while (!MathUtilities.MoveTo(ref statue.Position.X, target.X, 12)
                     | !MathUtilities.MoveTo(ref statue.Position.Y, target.Y, 12))
                {
                    yield return null;
                }
                GlobalState.screenShake.Shake(0.05f, 0.5f);
                SoundManager.PlaySoundEffect("wb_hit_ground");

                while (!MathUtilities.MoveTo(ref GlobalState.gameScreenFade.alpha, 1, 0.6f))
                {
                    yield return null;
                }
                statue.visible = false;
            }
            statue.exists = false;

            yield return new ReturnWarp();
            e.SetTarget(1);
            while (!MathUtilities.MoveTo(ref GlobalState.gameScreenFade.alpha, 0, 0.6f) | !e.ReachedTarget)
            {
                yield return null;
            }

            SoundManager.PlaySong("windmill");
            GlobalState.events.IncEvent("WindmillOpened");
            GlobalState.FireEvent(new OpenedWindmill());

            yield return new DialogueEvent("A voice: Nice work! You've gotten as far as this demo goes. You can explore the newly unlocked areas, but they're largely incomplete still.");

            yield break;
        }
    }
}
