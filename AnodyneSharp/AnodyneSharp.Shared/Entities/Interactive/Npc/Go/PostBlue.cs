using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
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
            Position = tl + new Vector2(6 * 16 + 2, 8 * 16);
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
                bike.Solid = false;
            }
        }

        public override void Update()
        {
            base.Update();
            if(!_preset.Activated && !started && !GlobalState.ScreenTransition)
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

            Position = tl + new Vector2(160, 5 * 16);
            velocity = Vector2.UnitX*-45;
            visible = true;

            while (Position.X > _player.Position.X - 24) yield return null;

            velocity = Vector2.UnitY * 45;

            while (Position.Y < _player.Position.Y - 4) yield return null;

            OffBike();
            bike.exists = true;
            bike.Position = _player.Position + new Vector2(-18, 10);

            velocity = Vector2.UnitY * 20;
            while (Position.Y <= _player.Hitbox.Bottom) yield return null;

            velocity = Vector2.UnitX * 20;
            while (Position.X <= _player.Position.X) yield return null;

            _player.facing = Facing.DOWN;
            facing = Facing.UP;
            velocity = Vector2.Zero;

            sage.visible = true;
            sage.Position = tl + new Vector2(4 * 16, 160) + sage.offset;
            sage.velocity = Vector2.UnitY * -20;

            while (sage.Position.Y >= tl.Y + 8 * 16 - 5) yield return null;

            sage.velocity = Vector2.UnitX * -20;

            while(sage.Position.X >= tl.X + 2 * 16 + 11) yield return null;

            sage.velocity = Vector2.Zero;
            sage.facing = Facing.RIGHT;

            _player.facing = Facing.LEFT;
            facing = Facing.LEFT;

            DialogueManager.SetSceneProgress("sage", "one", 0);

            for(int i = 0; i < 7; ++i)
            {
                yield return new DialogueEvent(DialogueManager.GetDialogue("sage", "one"));
            }

            SoundManager.PlaySoundEffect("sun_guy_charge");

            SageBullet bullet = new();
            bullet.Position = sage.Position + Vector2.UnitX * sage.width;
            bullet.Flicker(2.5f);

            yield return new EntityEvent(Enumerable.Repeat(bullet, 1));

            Action clampPlayer = () =>
            {
                _player.Position.X = Math.Clamp(_player.Position.X, tl.X + 16, tl.X + 9 * 16);
                _player.Position.Y = Math.Clamp(_player.Position.Y, tl.X + 16, tl.X + 9 * 16);
            };

            while (bullet._flickering)
            {
                clampPlayer();
                _player.dontMove = false;
                _player.actions_disabled = false;
                yield return null;
            }

            SoundManager.PlaySoundEffect("laser-pew");
            bullet.velocity = Vector2.UnitX * 10;

            while(true)
            {
                clampPlayer();

                bool hitPlayer = bullet.Hitbox.Intersects(_player.Hitbox);

                if(hitPlayer || (_player.broom.visible && bullet.Hitbox.Intersects(_player.broom.Hitbox)))
                {
                    bool flashReady = false;
                    GlobalState.flash.Flash(1, hitPlayer ? Color.Red : Color.White, () => flashReady = true);
                    while (!flashReady) yield return null;

                    bullet.visible = false;
                    if(hitPlayer)
                    {
                        _player.velocity = Vector2.Zero;
                        _player.Play("slumped");
                        _player.ANIM_STATE = PlayerAnimState.as_slumped;
                    }
                    yield return new EntityEvent(Enumerable.Repeat(new Explosion(bullet), 1));

                    yield return new DialogueEvent(DialogueManager.GetDialogue("sage", "hit", 0));
                    yield return new DialogueEvent(DialogueManager.GetDialogue("sage", "hit", 1));

                    break;
                }

                if(bullet.Position.X > bike.Position.X)
                {
                    bool flashReady = false;
                    GlobalState.flash.Flash(1, Color.Red, () => flashReady = true);
                    while (!flashReady) yield return null;

                    bullet.visible = false;
                    yield return new EntityEvent(Enumerable.Repeat(new Explosion(bullet), 1));

                    GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "hit", 2);

                    velocity = Vector2.UnitX * -20;
                    while (Position.X > bike.Position.X) yield return null;
                    velocity = Vector2.Zero;
                    facing = Facing.UP;
                    while (!GlobalState.LastDialogueFinished) yield return null;

                    yield return new DialogueEvent(DialogueManager.GetDialogue("sage", "hit", 3));
                    
                    facing = Facing.LEFT;

                    yield return new DialogueEvent(DialogueManager.GetDialogue("sage", "hit", 4));

                    break;
                }

                _player.dontMove = false;
                _player.actions_disabled = false;

                yield return null;
            }

            yield return new DialogueEvent(DialogueManager.GetDialogue("sage", "hit", 5));

            _preset.Activated = true;
            _facePlayer = true;
            Solid = true;
            bike.Solid = true;

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
                if (GlobalState.events.GetEvent("HappyDone") > 0)
                    return DialogueManager.GetDialogue("sage", "posthappy_mitra");
                return DialogueManager.GetDialogue("sage", "hit", 7);
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
            Position = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(40, 7 * 16 + 2) + offset;
            if(!preset.Activated)
            {
                visible = false;
                Solid = false;
            }
        }

        public override void Update()
        {
            base.Update();
            Solid = _facePlayer = preset.Activated;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if(_facePlayer)
            {
                if (GlobalState.events.GetEvent("HappyDone") > 0)
                    GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "posthappy_sage");
                else
                    GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "hit", 6);
                return true;
            }
            return false;
        }
    }

    internal class SageBullet : Entity
    {
        public SageBullet() : base(Vector2.Zero, "sage_attacks", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("a", CreateAnimFrameArray(0, 1), 8);
            Play("a");
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
            y_push = p.y_push;
        }

    }
}
