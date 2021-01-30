using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;

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

    [Collision(typeof(Dust),MapCollision = false)]
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

        public Entity long_attack;
        public Entity wide_attack;

        private Player _root;

        public float visible_timer = 0;

        private bool is_behind_player = false;
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
        }


        public override void Draw()
        {
            if (visible && exists)
            {
                float draw_y = MapUtilities.GetInGridPosition(Position).Y;
                if(!is_behind_player)
                {
                    float player_y = MapUtilities.GetInGridPosition(_root.Position).Y;
                    draw_y = player_y + 0.5f;
                }
                SpriteDrawer.DrawSprite(sprite.Tex, MathUtilities.CreateRectangle(Position.X - offset.X, Position.Y - offset.Y, sprite.Width, sprite.Height), sprite.GetRect(_curAnim.Frame), rotation: rotation, Z: DrawingUtilities.GetDrawingZ(layer, draw_y));

                wide_attack.Draw();
                long_attack.Draw();
            }
        }

        public override void Update()
        {
            if (_curAnim.Finished)
            {
                exists = false;

                wide_attack.visible = false;
                long_attack.visible = false;
                just_released_dust = false;
                if(dust != null)
                {
                    //dust.exists = false;
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
            else if (GlobalState.CURRENT_MAP_NAME == "SUBURB")
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

        private void UpdatePos()
        {
            Vector2 o;
            switch (_root.facing)
            {
                case Facing.LEFT:
                    rotation = 0;
                    Position = new Vector2(_root.Position.X - 14, _root.Position.Y);
                    is_behind_player = true;

                    o = new Vector2(0, -6);

                    if (GlobalState.inventory.EquippedBroom == BroomType.Wide)
                    {
                        SetWideValues(o, new Vector2(-5, 1));
                    }
                    else if (GlobalState.inventory.EquippedBroom == BroomType.Long)
                    {
                        SetLongValues(o, new Vector2(-14, 7));
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

                    o = new Vector2(5, -6);

                    if (GlobalState.inventory.EquippedBroom == BroomType.Wide)
                    {
                        SetWideValues(o, new Vector2(2, 3));
                    }
                    else if (GlobalState.inventory.EquippedBroom == BroomType.Long)
                    {
                        SetLongValues(o, new Vector2(0, 9));
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

                    o = new Vector2(-2, 0);

                    if (GlobalState.inventory.EquippedBroom == BroomType.Wide)
                    {
                        SetWideValues(o, new Vector2(5, -7));
                    }
                    else if (GlobalState.inventory.EquippedBroom == BroomType.Long)
                    {
                        SetLongValues(o, new Vector2(0, -6));
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

                    o = new Vector2(-2, 4);

                    if (GlobalState.inventory.EquippedBroom == BroomType.Wide)
                    {
                        SetWideValues(o, new Vector2(3, -1));
                    }
                    else if (GlobalState.inventory.EquippedBroom == BroomType.Long)
                    {
                        SetLongValues(o, new Vector2(-2, 5));
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

            if (!just_played_extra_anim)
            {
                wide_attack.Play("a");
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

            if (!just_played_extra_anim)
            {
                long_attack.Play("a");
            }
        }

        public void Attack()
        {
            visible_timer = 0;
            exists = true;
            facing = _root.facing;
            Play("stab", true);
            SoundManager.PlaySoundEffect("swing_broom_1", "swing_broom_2", "swing_broom_3");

            if(dust != null)
            {
                dust.Position = _root.Position + FacingDirection(_root.facing) * 16;
                int x = (int)dust.Position.X;
                int y = (int)dust.Position.Y;
                int xdiff = x % 16;
                int ydiff = y % 16;
                if (xdiff <= 9) x -= xdiff;
                else x += (16 - xdiff);

                if (ydiff <= 7) y -= ydiff;
                else y += (16 - ydiff);

                dust.Position = new Vector2(x, y);
                if(GlobalState.CheckTile(dust.Position) == Touching.NONE)
                {
                    dust.exists = true;
                    dust.Play("unpoof",true);
                    dust = null;
                    just_released_dust = true;
                    //dust-dust check done next frame by dust itself
                }
            }
        }

        public override void Collided(Entity other)
        {
            if(dust == null && !just_released_dust && other is Dust d && !ReferenceEquals(d,_root.raft))
            {
                dust = d;
                d.Play("poof");
            }
        }
    }
}
