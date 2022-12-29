using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using AnodyneSharp.States;
using AnodyneSharp.UI;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Interactive.Npc.Go
{
    [NamedEntity("Sage",map:"GO")]
    public class SageSentinel : Entity
    {
        EndingSage sage;

        public SageSentinel(EntityPreset preset, Player p) : base(Vector2.Zero,DrawOrder.BACKGROUND)
        {
            sage = new(p);
            visible = false;
        }

        public override void Update()
        {
            base.Update();
            sage.Update();
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            sage.PostUpdate();
        }

        public override void Draw()
        {
            GlobalState.UIEntities.Add(sage);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return sage.SubEntities();
        }
    }

    public class EndingSage : UIEntity
    {
        Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
        Player _player;

        bool started = false;

        Entity briar = new(Vector2.Zero, "briar", 16, 16, Drawing.DrawOrder.ENTITIES);

        public EndingSage(Player p) : base(Vector2.Zero, "sage", 16, 16, DrawOrder.UI_OBJECTS)
        {
            _player = p;

            AddAnimation("walk_r", CreateAnimFrameArray(2, 3), 6);
            AddAnimation("walk_l", CreateAnimFrameArray(2, 3), 6);
            AddAnimation("idle_l", CreateAnimFrameArray(7));

            briar.AddAnimation("walk_d", CreateAnimFrameArray(0, 1), 4);
            briar.AddAnimation("walk_r", CreateAnimFrameArray(2, 3), 4);
            briar.AddAnimation("walk_u", CreateAnimFrameArray(4, 5), 4);
            briar.AddAnimation("walk_l", CreateAnimFrameArray(6, 7), 4);
            briar.y_push = 8;
            briar.Position = tl + new Vector2(4 * 16, 60);
            briar.Play("walk_u");
            briar.velocity = Vector2.UnitY * -20;

            visible = false;
            p.grid_entrance = tl + new Vector2(75, 140);
        }

        public override void Update()
        {
            base.Update();
            _player.Position.Y = Math.Min(_player.Position.Y, tl.Y + 144);
            if(!GlobalState.ScreenTransition && !started)
            {
                started = true;
                GlobalState.StartCutscene = EndingCutscene();
            }
        }

        IEnumerator<CutsceneEvent> EndingCutscene()
        {
            while(_player.Position.Y > tl.Y+140 || _player.y_push < 14)
            {
                _player.dontMove = false;
                _player.Position.X = Math.Clamp(_player.Position.X, tl.X + 64, tl.X + 88);
                _player.Position.Y = Math.Max(_player.Position.Y, tl.Y + 16 * 6);
                yield return null;
            }

            _player.y_push = 14;
            _player.exists = false;
            PlayerStandin standin = new(_player);
            yield return new EntityEvent(Enumerable.Repeat(standin, 1));

            yield return new DialogueEvent(DialogueManager.GetDialogue("briar", "final"));

            briar.Position.Y = tl.Y - 16;
            briar.velocity = Vector2.UnitY * 25;
            briar.y_push = 14;
            briar.Play("walk_d");
            standin.Play("idle_u");

            while((briar.Position - standin.Position).Length() > 7)
            {
                MathUtilities.MoveTo(ref briar.Position.X, standin.Position.X, 10);
                yield return null;
            }

            briar.velocity = Vector2.Zero;

            while(!MathUtilities.MoveTo(ref briar.y_push, 6, 1/0.3f))
            {
                standin.y_push = _player.y_push = briar.y_push;
                yield return null;
            }

            yield return new DialogueEvent(DialogueManager.GetDialogue("briar", "final"));

            standin.Play("walk_r");
            while (!MathUtilities.MoveTo(ref standin.Position.X, tl.X + 85, 15))
            {
                _player.Position = standin.Position;
                yield return null;
            }

            standin.Play("walk_l");
            while (!MathUtilities.MoveTo(ref standin.Position.X, tl.X + 65, 15))
            {
                _player.Position = standin.Position;
                yield return null;
            }

            standin.Play("idle_u");
            yield return new DialogueEvent(DialogueManager.GetDialogue("briar", "final"));

            briar.Play("walk_u");
            briar.velocity = Vector2.UnitY * -25;

            while (briar.Position.Y > tl.Y - 16) yield return null;

            Position = new(160, MapUtilities.GetInGridPosition(standin.Position).Y + 20);
            visible = true;
            Play("walk_l");
            velocity = Vector2.UnitX * -24;

            while (Position.X > 128) yield return null;

            standin.Play("idle_r");
            Play("idle_l");
            velocity = Vector2.Zero;

            yield return new DialogueEvent(DialogueManager.GetDialogue("briar", "final"));

            _player.Position = standin.Position;
            _player.facing = Facing.RIGHT;
            _player.exists = true;
            standin.exists = false;

            while(_player.Position.Y > tl.Y+8)
            {
                _player.dontMove = false;
                _player.y_push = 6;
                _player.Position.X = Math.Clamp(_player.Position.X, tl.X + 64, tl.X + 88);
                if (_player.Position.Y < tl.Y + 80)
                    MathUtilities.MoveTo(ref _player.opacity, 0, 0.3f);
                yield return null;
            }

            while (!MathUtilities.MoveTo(ref _player.opacity, 0, 0.3f)) yield return null;

            _player.exists = false;

            {
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, 1, 1)) yield return null;
            }

            GlobalState.wave.active = true;

            GlobalState.gameScreenFade.fadeColor = Color.White;
            while(GlobalState.gameScreenFade.alpha < 1)
            {
                GlobalState.gameScreenFade.ChangeAlpha(0.12f);
                yield return null;
            }

            Play("walk_r");
            velocity = Vector2.UnitX * 12;
            
            GlobalState.black_overlay.fadeColor = Color.Black;
            while (GlobalState.black_overlay.alpha < 1)
            {
                GlobalState.black_overlay.ChangeAlpha(0.3f);
                yield return null;
            }

            VolumeEvent volume = new(0, 0.18f);

            yield return new EntityEvent(Enumerable.Repeat(volume,1));

            while (!volume.ReachedTarget) yield return null;

            yield return new ChangeGameStateEvent(AnodyneGame.GameState.Credits);

            yield break;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(briar,1);
        }

        protected override void AnimationChanged(string name)
        {
            if (name == "walk_l" || name == "idle_l")
            {
                _flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                _flip = SpriteEffects.None;
            }
        }
    }
}
