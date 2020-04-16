using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities
{
    public enum BroomType
    {
        NONE,
        Normal,
        Wide,
        Long,
        Transformer
    }

    [Collision(MapCollision = false)]
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

        //public Dust dust;
        public bool just_released_dust;

        public Entity long_attack;
        public Entity wide_attack;

        private Entity _root;

        public float visible_timer = 0;

        private bool is_behind_player = false;
        private bool just_played_extra_anim = false;
        private bool is_wide;
        private bool is_long;

        public Broom(Entity root)
            : base(new Vector2(root.Position.X - 10, root.Position.Y), 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            _root = root;
            visible = false;
            immovable = true;

            SetTexture(Broom_Sprite);

            AddAnimation("stab", CreateAnimFrameArray(1, 2, 2, 1, 0, 0), 20, false);

            wide_attack = new Entity(Position, Wide_Attack_v, WATK_H, WATK_W, Drawing.DrawOrder.UNDER_ENTITIES);
            long_attack = new Entity(Position, Long_Attack_v, LATK_H, LATK_W, Drawing.DrawOrder.UNDER_ENTITIES);

            wide_attack.AddAnimation("a", CreateAnimFrameArray(0, 1, 2, 3, 4), 14, false);
            long_attack.AddAnimation("a", CreateAnimFrameArray(0, 1, 2, 3, 4), 14, false);
        }


        public override void Draw()
        {
            if (visible)
            {
                float draw_y = MapUtilities.GetInGridPosition(Position).Y;
                if(!is_behind_player)
                {
                    float player_y = MapUtilities.GetInGridPosition(_root.Position).Y;
                    draw_y = player_y + 0.5f;
                }
                SpriteDrawer.DrawSprite(Texture, MathUtilities.CreateRectangle(Position.X - offset.X, Position.Y - offset.Y, frameWidth, frameHeight), spriteRect, rotation: rotation, Z: DrawingUtilities.GetDrawingZ(layer, draw_y));

                wide_attack.Draw();
                long_attack.Draw();
            }
        }

        public override void Update()
        {
            if (visible)
            {
                if (finished)
                {
                    visible = false;

                    wide_attack.visible = false;
                    long_attack.visible = false;
                }
                else
                {
                    UpdatePos();
                }

                //if (visible)
                //{
                //    visible_timer += GameTimes.DeltaTime;
                //    // needs to be long enough to ensure the broom makes a full anim
                //    if (visible_timer > 0.4)
                //    {
                //        long_attack_h.visible = long_attack_v.visible = visible = false;
                //        wide_attack_h.visible = wide_attack_v.visible = false;
                //    }
                //}
                //else
                //{
                //    visible_timer = 0;
                //}
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

            if (GlobalState.CURRENT_MAP_NAME == "TRAIN")
            {
                SetTexture(Cell_Sprite);
                is_wide = is_long = false;
                return;
            }
            else if (GlobalState.CURRENT_MAP_NAME == "SUBURB")
            {
                SetTexture(Knife_Sprite);
                is_wide = is_long = false;
                return;
            }
            else
            {
                SetTexture(Broom_Sprite);
            }

            switch (GlobalState.EquippedBroom)
            {
                case BroomType.Wide:
                    is_wide = true;
                    is_long = false;
                    break;
                case BroomType.Long:
                    is_wide = false;
                    is_long = true;
                    break;
                case BroomType.Transformer:
                    break;
                default:
                    is_wide = is_long = false;
                    break;
            }
        }

        private void UpdatePos()
        {
            switch (_root.facing)
            {
                case Facing.LEFT:
                    rotation = 0;
                    Position = new Vector2(_root.Position.X - 14, _root.Position.Y);
                    is_behind_player = true;

                    if (is_wide)
                    {
                        SetWideValues(new Vector2(0, -6), new Vector2(-1, 4));
                    }
                    else if (is_long)
                    {
                        SetLongValues(new Vector2(0, -6), new Vector2(-9, 4));
                    }

                    switch (_curFrame)
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

                    if (is_wide)
                    {
                        SetWideValues(new Vector2(5, -6), new Vector2(-3, 6));
                    }
                    else if (is_long)
                    {
                        SetLongValues(new Vector2(5, -6), new Vector2(5, 6));
                    }

                    switch (_curFrame)
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

                    if (is_wide)
                    {
                        SetWideValues(new Vector2(-2, 0), new Vector2(3, -4));
                    }
                    else if (is_long)
                    {
                        SetLongValues(new Vector2(-2, 0), new Vector2(3, -6));
                    }

                    switch (_curFrame)
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

                    if (is_wide)
                    {
                        SetWideValues(new Vector2(-2, 4), new Vector2(1, 0));
                    }
                    else if (is_long)
                    {
                        SetLongValues(new Vector2(-2, 4), new Vector2(1, 0));
                    }

                    switch (_curFrame)
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
    }
}
