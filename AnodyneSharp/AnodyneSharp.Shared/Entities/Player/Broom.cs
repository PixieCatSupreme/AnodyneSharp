using AnodyneSharp.Drawing;
using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace AnodyneSharp.Entities
{
    public enum BroomType
    {
        NONE,
        Normal,
        Long,
        Wide,
        Transformer
    }

    [Collision(typeof(Dust), MapCollision = false)]
    public class Broom : Entity
    {
        private const int WATK_W = 24;
        private const int WATK_H = 12;
        private const int LATK_W = 12;
        private const int LATK_H = 22;

        public static string Broom_Sprite = "broom";
        public static string Knife_Sprite = "knife";
        public static string Cell_Sprite = "broom_cell";
        public static string Icon_Broom_Sprite = "broom-icon";
        public static string Wide_Attack_v = "wide_attack_v";
        public static string Long_Attack_v = "long_attack_h";

        public bool AnimFinished => _curAnim.Finished;

        public Dust dust;
        public bool just_released_dust;
        private Dust pickup_candidate = null;

        public Entity long_attack;
        public Entity wide_attack;

        private Player _root;

        public float visible_timer = 0;

        public bool is_behind_player = false;
        private bool just_played_extra_anim = false;


        public Broom(Player root)
            : base(new Vector2(root.Position.X - 10, root.Position.Y), "broom", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            _root = root;
            exists = false;
            immovable = true;

            AddAnimation("stab", CreateAnimFrameArray(1, 2, 2, 1, 0, 0), 20, false);

            wide_attack = new Entity(Position, Wide_Attack_v, WATK_H, WATK_W, Drawing.DrawOrder.PARTICLES);
            long_attack = new Entity(Position, Long_Attack_v, LATK_H, LATK_W, Drawing.DrawOrder.PARTICLES);

            wide_attack.AddAnimation("a", CreateAnimFrameArray(0, 1, 2, 3, 4), 14, false);
            long_attack.AddAnimation("a", CreateAnimFrameArray(0, 1, 2, 3, 4), 14, false);

            LayerParent = root;
            LayerOffset = 1;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { long_attack, wide_attack };
        }

        public override void Update()
        {
            if (pickup_candidate != null)
            {
                dust = pickup_candidate;
                pickup_candidate = null;
                dust.Play("poof");

#if RELEASE

                if (GlobalState.events.GetEvent("SweptDust") == 0)
                {
                    GlobalState.events.IncEvent("SweptDust");

                    GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("misc", "any", "dust", 0);
                }
#endif
            }
            if (_curAnim.Finished)
            {
                exists = false;

                wide_attack.visible = false;
                long_attack.visible = false;

                just_played_extra_anim = false;

                if (just_released_dust)
                {
                    dust = null;
                    just_released_dust = false;
                }
            }
            else
            {
                UpdatePos();
            }

            wide_attack.Update();
            long_attack.Update();

            base.Update();
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

            wide_attack.PostUpdate();
            long_attack.PostUpdate();
        }

        public void UpdateBroomType()
        {
            Position = new Vector2(_root.Position.X - 10, _root.Position.Y);
            width = height = 16;
            offset = Vector2.Zero;

            if (GlobalState.IsCell)
            {
                SetTexture(Cell_Sprite, sprite.Width, sprite.Height);
            }
            else if (GlobalState.IsKnife)
            {
                SetTexture(Knife_Sprite, sprite.Width, sprite.Height);
            }
            else
            {
                SetTexture(Broom_Sprite, sprite.Width, sprite.Height);
            }
        }

        public override void ReloadTexture(bool ignoreChaos = false)
        {
            base.ReloadTexture();
            long_attack.ReloadTexture();
            wide_attack.ReloadTexture();
        }

        public override void Draw()
        {
            if(visible && exists)
                base.Draw();
        }

        private void UpdatePos()
        {
            facing = _root.facing;

            switch (_root.facing)
            {
                case Facing.LEFT:
                    rotation = 0;
                    Position = new Vector2(_root.Position.X - 14, _root.Position.Y);
                    is_behind_player = true;
                    LayerOffset = -1;

                    if (GlobalState.inventory.EquippedBroom == BroomType.Wide)
                    {
                        SetWideValues(new Vector2(1, -5), new Vector2(-3, 0));
                    }
                    else if (GlobalState.inventory.EquippedBroom == BroomType.Long)
                    {
                        SetLongValues(new Vector2(-13, 0), new Vector2(-1, 1));
                    }

                    switch (_curAnim.Frame)
                    {
                        case 0: Position.X += 10; break;
                        case 1: Position.X += 6; break;
                        case 2: Position.X -= 1; break;
                    }

                    break;
                case Facing.RIGHT:
                    rotation = MathHelper.ToRadians(180);
                    Position = new Vector2(_root.Position.X + _root.width, _root.Position.Y - 2);
                    is_behind_player = false;
                    LayerOffset = 1;

                    if (GlobalState.inventory.EquippedBroom == BroomType.Wide)
                    {
                        SetWideValues(new Vector2(3, -3), new Vector2(5, 0));
                    }
                    else if (GlobalState.inventory.EquippedBroom == BroomType.Long)
                    {
                        SetLongValues(new Vector2(6, 2), new Vector2(0, 0));
                    }

                    switch (_curAnim.Frame)
                    {
                        case 0: Position.X -= 12; break;
                        case 1: Position.X -= 8; break;
                        case 2: Position.X -= 1; break;
                    }
                    break;
                case Facing.UP:
                    rotation = MathHelper.ToRadians(90);
                    Position = new Vector2(_root.Position.X + -2, _root.Position.Y - 16);
                    is_behind_player = true;
                    LayerOffset = -1;

                    if (GlobalState.inventory.EquippedBroom == BroomType.Wide)
                    {
                        SetWideValues(new Vector2(-3, -1), new Vector2(6, -6));
                    }
                    else if (GlobalState.inventory.EquippedBroom == BroomType.Long)
                    {
                        SetLongValues(new Vector2(3, -10), new Vector2(-5, 4));
                    }

                    switch (_curAnim.Frame)
                    {
                        case 0: Position.Y += 12; break;
                        case 1: Position.Y += 6; break;
                        case 2: Position.Y += 2; break;
                    }
                    break;
                case Facing.DOWN:
                    rotation = MathHelper.ToRadians(270);
                    Position = new Vector2(_root.Position.X + -6, _root.Position.Y + _root.height);
                    is_behind_player = false;
                    LayerOffset = 1;

                    if (GlobalState.inventory.EquippedBroom == BroomType.Wide)
                    {
                        SetWideValues(new Vector2(-5, 4), new Vector2(6, -6));
                    }
                    else if (GlobalState.inventory.EquippedBroom == BroomType.Long)
                    {
                        SetLongValues(new Vector2(1, 6), new Vector2(-5, 3));
                    }

                    switch (_curAnim.Frame)
                    {
                        case 0: Position.Y -= 8; break;
                        case 1: Position.Y -= 5; break;
                        case 2: Position.Y -= 2; break;
                    }
                    break;
            }
        }

        private void SetWideValues(Vector2 offset, Vector2 wideAttackOffset)
        {
            Position += offset;
            this.offset = offset;

            wide_attack.rotation = rotation;
            wide_attack.visible = true;
            wide_attack.Position = Position + wideAttackOffset;

            width = WATK_H;
            height = WATK_W;

            if (FacingDirection(facing).X == 0)
            {
                (width, height) = (height, width);
            }

            if (!just_played_extra_anim)
            {
                wide_attack.Play("a", true);

                just_played_extra_anim = true;
            }
        }

        private void SetLongValues(Vector2 offset, Vector2 longAttackOffset)
        {
            Position += offset;
            this.offset = offset;

            long_attack.rotation = rotation;
            long_attack.visible = true;
            long_attack.Position = Position + longAttackOffset;
            width = LATK_H;
            height = LATK_W;

            if (FacingDirection(facing).X == 0)
            {
                (width, height) = (height, width);
            }

            if (!just_played_extra_anim)
            {
                long_attack.Play("a", true);

                just_played_extra_anim = true;
            }
        }

        public void Attack()
        {
            visible_timer = 0;
            exists = true;
            facing = _root.facing;
            Play("stab", true);
            SoundManager.PlaySoundEffect("swing_broom_1", "swing_broom_2", "swing_broom_3");

            UpdatePos();

            GlobalState.FireEvent(new BroomUsed());

            if (GlobalState.IsKnife && GlobalState.events.GetEvent("Stabbed") == 0)
            {
                GlobalState.events.IncEvent("Stabbed");

                GlobalState.inventory.EquippedBroomChanged = true;
            }

            if (dust != null)
            {
                dust.Position = (_root.Center / 16 + FacingDirection(_root.facing)).ToPoint().ToVector2() * 16;
                
                if (GlobalState.Map.GetCollisionData(dust.Position) == Touching.NONE)
                {
                    dust.exists = true;
                    dust.Play("unpoof", true);
                    just_released_dust = true;
                    //dust-dust check done next frame by dust itself
                }
            }
        }

        public override void Collided(Entity other)
        {
            if (dust == null && !just_released_dust && other is Dust d && !ReferenceEquals(d, _root.raft))
            {
                if (pickup_candidate == null || (_root.Center - pickup_candidate.Center).LengthSquared() < (d.Center - _root.Center).LengthSquared())
                {
                    pickup_candidate = d;
                }
            }
        }
    }
}
