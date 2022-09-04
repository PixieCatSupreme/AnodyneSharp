using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Interactive.Npc.Go
{
    [NamedEntity("Mitra", map: "GO")]
    public class PostBlue : Mitra
    {
        PostBlueSage sage;

        Vector2 tl;
        bool started = false;

        public PostBlue(EntityPreset preset, Player p) : base(preset,p,!preset.Activated)
        {
            tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
            Position = tl + new Vector2(6 * 16 + 2, 8 * 16 - 20);
            bike.Position = tl + new Vector2(6 * 16 + 2, 7 * 16);
            sage = new(preset, p);
            if(GlobalState.events.GetEvent("BlueDone") == 0)
            {
                exists = false;
                return;
            }
            if(preset.Activated)
            {
                bike.exists = true;
            }
            else
            {
                visible = false;
                _facePlayer = false;
                Solid = false;
            }
        }

        public override void Update()
        {
            base.Update();
            if(!_preset.Activated && !started)
            {
                if(_player.Position.X < tl.X + 7*16 && _player.Position.Y < tl.Y + 7*16)
                {
                    GlobalState.StartCutscene = Cutscene();
                    started = true;
                }
            }
        }

        IEnumerator<CutsceneEvent> Cutscene()
        {
            PlayerStandin standin = new(_player);
            _player.visible = false;
            
            yield return new EntityEvent(Enumerable.Repeat(standin, 1));

            while(standin.Position.X < tl.X + 7*16)
            {
                standin.velocity.X = 60;
                standin.Play("walk_r");
                yield return null;
            }
            standin.velocity = Vector2.Zero;
            standin.Position.X = tl.X + 7 * 16;

            while (standin.Position.Y < tl.Y + 7 * 16)
            {
                standin.velocity.Y = 60;
                standin.Play("walk_d");
                yield return null;
            }
            standin.velocity = Vector2.Zero;
            standin.Position.Y = tl.Y + 7 * 16;

            _player.Position = standin.Position;
            _player.visible = true;
            _player.facing = Facing.RIGHT;
            standin.exists = false;



            yield break;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return base.SubEntities().Concat(new List<Entity>() { sage });
        }

        protected override string GetInteractionText()
        {
            if(_facePlayer)
            {

            }
            return "";
        }
    }

    internal class PostBlueSage : SpriteSage, Interactable
    {
        EntityPreset preset;

        public PostBlueSage(EntityPreset preset, Player p) : base(Vector2.Zero,p)
        {
            _facePlayer = preset.Activated;
            this.preset = preset;
            Position = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(40, 7 * 16 + 2);
            if(!preset.Activated)
            {
                visible = false;
                Solid = false;
            }
        }

        public override void Update()
        {
            base.Update();
            _facePlayer = preset.Activated;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if(_facePlayer)
            {
                //todo: dialogue
                return true;
            }
            return false;
        }
    }

    //Quick copy of player visual data to make for easier cutscene manipulation
    internal class PlayerStandin : Entity
    {
        public PlayerStandin(Player p) : base(p.Position,"young_player",16,16,Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("walk_d", CreateAnimFrameArray(1, 0), 6, true);
            AddAnimation("walk_r", CreateAnimFrameArray(2, 3), 8, true);
            AddAnimation("walk_u", CreateAnimFrameArray(4, 5), 6, true);
            AddAnimation("walk_l", CreateAnimFrameArray(6, 7), 8, true);

            AddAnimation("attack_d", CreateAnimFrameArray(8, 9), 10, false);
            AddAnimation("attack_r", CreateAnimFrameArray(10, 11), 10, false);
            AddAnimation("attack_u", CreateAnimFrameArray(12, 13), 10, false);
            AddAnimation("attack_l", CreateAnimFrameArray(14, 15), 10, false);
            AddAnimation("fall", CreateAnimFrameArray(28, 29, 30, 31), 5, false);
            AddAnimation("slumped", CreateAnimFrameArray(32));

            AddAnimation("whirl", CreateAnimFrameArray(25, 26, 27, 24), 12, true);

            AddAnimation("idle_d", CreateAnimFrameArray(24), 4, true);
            AddAnimation("idle_r", CreateAnimFrameArray(25), 4, true);
            AddAnimation("idle_u", CreateAnimFrameArray(26), 4, true);
            AddAnimation("idle_l", CreateAnimFrameArray(27), 4, true);

            Play(p.CurAnimName);

            height = p.height;
            width = p.width;
            offset = p.offset;
        }

    }
}
