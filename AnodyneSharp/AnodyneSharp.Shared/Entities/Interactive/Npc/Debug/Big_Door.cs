using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Interactive.Npc.Debug
{
    [NamedEntity, Collision(typeof(Player))]
    public class Big_Door : Entity
    {
        IEnumerator state;
        Entity active_region;
        bool finished = false;

        public Big_Door(EntityPreset preset, Player p) : base(preset.Position,"big_door",32,32,Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;
            active_region = new(Position + new Vector2(15, 34),new SolidColorRenderer(Color.Red,2,2),DrawOrder.ENTITIES);
            state = State(p);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(active_region, 1);
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
        }

        IEnumerator State(Player p)
        {
            while (!p.Hitbox.Intersects(active_region.Hitbox))
                yield return null;

            GlobalState.StartCutscene = LockedScene();

            while (p.Hitbox.Intersects(active_region.Hitbox))
                yield return null;

            while (!p.Hitbox.Intersects(active_region.Hitbox))
                yield return null;

            GlobalState.StartCutscene = OpenCutscene();

            while (!finished) yield return null;

            while (!MathUtilities.MoveTo(ref opacity, 0, 0.6f)) yield return null;

            exists = false;

            yield break;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        static IEnumerator<CutsceneEvent> LockedScene()
        {
            SoundManager.PlaySoundEffect("big_door_locked");
            
            LockSquare[] squares = Enumerable.Range(0, 36).Select(i => new LockSquare(i)).ToArray();

            //Centered text gets "centered" by adding the screenwidth/2 to it
            UILabel score_1 = new(new(-100,30), true, $"{GlobalState.inventory.CardCount}\n---",forceEnglish:true,centerText:true);
            UILabel score_2 = new(new(100,50), true, "20", forceEnglish: true, centerText: true);

            yield return new EntityEvent(squares.Select(e => new DrawAsUI(e)).Concat(new List<Entity> { new UILabelEntity(score_1), new UILabelEntity(score_2)}));

            var SpawnSquares = CoroutineUtils.OnceEvery(
                CoroutineUtils.ForEach(squares, e => e.state = e.Appear()),
                0.06f);

            while(true)
            {
                bool doneScores = false;
                float x1 = score_1.Position.X;
                float x2 = score_2.Position.X;
                if (MathUtilities.MoveTo(ref x1, 0, 240) & MathUtilities.MoveTo(ref x2, 0, 240))
                {
                    doneScores = true;
                }

                score_1.Position = new(x1, score_1.Position.Y);
                score_2.Position = new(x2, score_2.Position.Y);

                if ((doneScores & !SpawnSquares.MoveNext()) && squares.Last().state is null) break;

                yield return null;
            }

            bool cont = false;
            GlobalState.flash.Flash(0.33f, Color.White, ()=>cont=true);

            while (!cont) yield return null;

            SpawnSquares = CoroutineUtils.OnceEvery(
                CoroutineUtils.ForEach(squares, e => e.state = e.Disappear()),
                0.06f);

            while (true)
            {
                bool doneScores = false;
                float x1 = score_1.Position.X;
                float x2 = score_2.Position.X;
                MathUtilities.MoveTo(ref x1, 100, 180);
                MathUtilities.MoveTo(ref x2, -100, 180);
                if (MathUtilities.MoveTo(ref score_1.Writer.Opacity, 0, 1.2f) & MathUtilities.MoveTo(ref score_2.Writer.Opacity, 0, 1.2f))
                {
                    doneScores = true;
                }

                score_1.Position = new(x1, score_1.Position.Y);
                score_2.Position = new(x2, score_2.Position.Y);

                SpawnSquares.MoveNext();

                if (doneScores && !squares.Last().exists) break;

                yield return null;
            }

            yield break;
        }

        public class LockSquare : Entity
        {
            public IEnumerator state;

            public LockSquare(int i) : base(new(10 + 24 * (i % 6), 20 + 24 * (i / 6)),
                    new SolidColorRenderer(i < GlobalState.inventory.CardCount ? Color.Black : Color.Yellow, 20, 20),
                    DrawOrder.UI_OBJECTS)
            {
                scale = 2;
                opacity = 0;
            }

            public override void Update()
            {
                base.Update();
                if (!state?.MoveNext() ?? false) state = null;
            }

            public IEnumerator Appear()
            {
                while (!MathUtilities.MoveTo(ref opacity, 1, 1.8f) & !MathUtilities.MoveTo(ref scale, 1, 2.4f))
                    yield return null;

                yield break;
            }

            public IEnumerator Disappear()
            {
                angularAcceleration = 1.67f * MathF.PI;
                while (!MathUtilities.MoveTo(ref opacity, 0, 1.8f))
                {
                    MathUtilities.MoveTo(ref scale, 0.2f, 0.6f);
                    yield return null;
                }

                exists = false;
                yield break;
            }
        }

        IEnumerator<CutsceneEvent> OpenCutscene()
        {
            RotatingOpenSquare.grow_opacity = true;
            RotatingOpenSquare.rotation_radius = 45;
            RotatingOpenSquare.rotation_speed = 1.8f;

            RotatingOpenSquare[] rot_squares = Enumerable.Range(1, 20).Select(i => new RotatingOpenSquare(i / 20f)).ToArray();

            yield return new EntityEvent(rot_squares.Select(e=>new DrawAsUI(e)));

            while (rot_squares.Last().opacity < 0.8f) yield return null;

            RotatingOpenSquare.grow_opacity = false;
            RotatingOpenSquare.rotation_speed = 3.6f;
            while (!MathUtilities.MoveTo(ref RotatingOpenSquare.rotation_radius, -100, 84)) yield return null;
            
            RotatingOpenSquare.rotation_speed = 5.4f;
            while (!MathUtilities.MoveTo(ref RotatingOpenSquare.rotation_radius, 0, 84)) yield return null;

            bool cont = false;
            GlobalState.flash.Flash(1.6f, Color.White, () => cont = true);

            while (!cont) yield return null;

            foreach (var e in rot_squares) e.exists = false;

            Vector2 screen_pos = MapUtilities.GetInGridPosition(Position) + Vector2.UnitX * width / 2;

            RandomOpenSquare.go = false;
            RandomOpenSquare[] rand_squares = Enumerable.Range(0, 36).Select(i => new RandomOpenSquare(screen_pos)).ToArray();
            yield return new EntityEvent(rand_squares.Select(e => new DrawAsUI(e)));

            while (GlobalState.flash.Active()) yield return null;

            RandomOpenSquare.go = true;

            while (rand_squares.Any(e => e.exists)) yield return null;
            finished = true;
            yield break;
        }

        private class RotatingOpenSquare : Entity
        {
            float rotate_angle;
            float alpha_growth;

            public static float rotation_radius;
            public static float rotation_speed;
            public static bool grow_opacity;

            public RotatingOpenSquare(float percentage) : base(Vector2.Zero,new SolidColorRenderer(Color.Black,20,20),DrawOrder.UI_OBJECTS)
            {
                rotate_angle = percentage * 360; //Intentionally confuse radians with degrees here to get a fun distribution
                alpha_growth = percentage * 0.48f;
                opacity = 0f;
            }

            public override void Update()
            {
                base.Update();
                if(grow_opacity)
                {
                    MathUtilities.MoveTo(ref opacity, 1, alpha_growth);
                }
                rotate_angle += rotation_speed * GameTimes.DeltaTime;
                Position = Vector2.One * 72 + new Vector2(MathF.Cos(rotate_angle), MathF.Sin(rotate_angle)) * rotation_radius;
            }
        }

        private class RandomOpenSquare : Entity
        {
            public static bool go = false;
            Vector2 target;

            public RandomOpenSquare(Vector2 target) : base(Vector2.Zero, new SolidColorRenderer(Color.Black, 20, 20), DrawOrder.UI_OBJECTS)
            {
                scale = GlobalState.RNG.NextSingle() * 2 + 0.5f;
                Position.X = GlobalState.RNG.Next(160);
                Position.Y = GlobalState.RNG.Next(160);
                angularVelocity = GlobalState.RNG.NextSingle() * MathF.Tau * 2 - MathF.Tau;
                opacity = GlobalState.RNG.NextSingle();
                this.target = target;
            }

            public override void Update()
            {
                base.Update();
                if(go)
                {
                    if (MathUtilities.MoveTo(ref Position.X, target.X, 60) & MathUtilities.MoveTo(ref Position.Y, target.Y, 60))
                        exists = false;
                }
            }
        }
    }

    public class DrawAsUI : Entity
    {
        Entity to_draw;

        public DrawAsUI(Entity other) : base(Vector2.Zero)
        {
            to_draw = other;
        }

        public override void Update()
        {
            base.Update();
            to_draw.Update();
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            to_draw.PostUpdate();
        }

        public override void Draw()
        {
            GlobalState.UIEntities.Add(to_draw);
        }
    }

    public class UILabelEntity : Entity
    {
        UILabel to_draw;
        bool draw_added = false;

        public UILabelEntity(UILabel other) : base(Vector2.Zero)
        {
            to_draw = other;
        }

        public override void Update()
        {
            base.Update();
            to_draw.Update();
        }

        public override void Draw()
        {
            if (draw_added)
            {
                to_draw.Draw();
                draw_added = false;
            }
            else
            {
                draw_added = true;
                GlobalState.UIEntities.Add(this);
            }
        }
    }
}
