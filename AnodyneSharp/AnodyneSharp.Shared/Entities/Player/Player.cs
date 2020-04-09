using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AnodyneSharp.Entities.Player
{
    public enum PlayerState
    {
        GROUND,
        AIR,
        RAFT,
        AUTO_JUMP,
        INTERACT,
        ENTER_FALL,
        LADDER
    }

    public enum PlayerAnimState
    {
        ANIM_DEFAULT,
        ANIM_ATK_R,
        ANIM_ATK_L,
        ANIM_ATK_U,
        ANIM_ATK_D,
        ANIM_FALL,
        ANIM_DEAD,
        as_idle,
        as_walk,
        as_slumped
    }

    public class Player : Entity
    {
        public static Texture2D Player_Sprite;
        public static Texture2D Cell_Player_Sprite;
        public static Texture2D Player_Mobile_Indicator_Sprite;

        public static Texture2D Player_reflection_Sprite;
        internal float grid_entrance_y;
        public static Texture2D Broom_reflection_sprite;
        internal bool dontMove;

        public static void SetSprites(ContentManager content)
        {
            string folder = "sprites";

            Player_Sprite = TextureUtilities.LoadTexture(folder, "young_player", content);
            Cell_Player_Sprite = TextureUtilities.LoadTexture(folder, "young_player_cell", content);
            Player_Mobile_Indicator_Sprite = TextureUtilities.LoadTexture(folder, "player_mobile_indicator", content);

            Player_reflection_Sprite = TextureUtilities.LoadTexture(folder, "young_player_reflection", content);
            Broom_reflection_sprite = TextureUtilities.LoadTexture(folder, "broom_reflection", content);
        }

        public const int DEFAULT_Y_OFFSET = 4;
        public const string TRANSITION_IDLE = "idle_u";

        private const int ORIGINAL_WIDTH = 16;
        private const int ORIGINAL_HEIGHT = 16;
        private const int HITBOX_HEIGHT = 12;
        private const int HITBOX_WIDTH = 10;

        private int walkSpeed = 70;

        /* Animation state */
        public PlayerAnimState ANIM_STATE;
        private int idle_ticks = 0;

        private int[] last_frame = { 0, 0, 0, 0 };
        public int DEATH_FRAME;

        public PlayerState state;
        internal bool invincible;
        internal float grid_entrance_x;

        public Player()
            : base(Vector2.Zero, ORIGINAL_WIDTH, ORIGINAL_HEIGHT)
        {
            //DEATH_FRAME = 32;

            AddAnimation("walk_d", CreateAnimFrameArray(1, 0), 6, true);
            AddAnimation("walk_r", CreateAnimFrameArray(2, 3), 8, true);
            AddAnimation("walk_u", CreateAnimFrameArray(4, 5), 6, true);
            AddAnimation("walk_l", CreateAnimFrameArray(6, 7), 8, true);

            AddAnimation("attack_down", CreateAnimFrameArray(8, 9), 10, false);
            AddAnimation("attack_right", CreateAnimFrameArray(10, 11), 10, false);
            AddAnimation("attack_up", CreateAnimFrameArray(12, 13), 10, false);
            AddAnimation("attack_left", CreateAnimFrameArray(14, 15), 10, false);
            AddAnimation("fall", CreateAnimFrameArray(28, 29, 30, 31), 5, false);
            AddAnimation("die", CreateAnimFrameArray(25, 26, 27, 24, 25, 26, 27, 24, 25, 26, 27, 32), 6, false);
            AddAnimation("slumped", CreateAnimFrameArray(32));

            AddAnimation("whirl", CreateAnimFrameArray(25, 26, 27, 24), 12, true);

            AddAnimation("idle_d", CreateAnimFrameArray(24), 4, true);
            AddAnimation("idle_r", CreateAnimFrameArray(25), 4, true);
            AddAnimation("idle_u", CreateAnimFrameArray(26), 4, true);
            AddAnimation("idle_l", CreateAnimFrameArray(27), 4, true);

            AddAnimation("jump_d", CreateAnimFrameArray(16, 17), 4, true);
            AddAnimation("jump_r", CreateAnimFrameArray(18, 19), 4, true);
            AddAnimation("jump_u", CreateAnimFrameArray(20, 21), 4, true);
            AddAnimation("jump_l", CreateAnimFrameArray(22, 23), 4, true);
            AddAnimation("idle_climb", CreateAnimFrameArray(33));
            AddAnimation("climb", CreateAnimFrameArray(34, 35), 8, true);

            height = HITBOX_HEIGHT;
            offset.Y = DEFAULT_Y_OFFSET;
            width = HITBOX_WIDTH;

            Play("idle_u");
            ANIM_STATE = PlayerAnimState.as_idle;
        }

        public void Reset()
        {
            if (GlobalState.CURRENT_MAP_NAME == "TRAIN")
            {
                Texture = Cell_Player_Sprite;
            }
            else
            {
                Texture = Player_Sprite;
            }
        }

        public override void Update()
        {
            if (GlobalState.FUCK_IT_MODE_ON)
            {
                solid = false;
                walkSpeed = 200;

                if (KeyInput.CanPressKey(Keys.V))
                {
                    visible = !visible;
                }
            }
            else
            {
                walkSpeed = 70;
                solid = true;
            }

            //if (!common_conditions())
            //{
            //    return;
            //}

            if (!dontMove)
            {
                Movement();
            }

            base.Update();
        }

        private void Movement()
        {
            switch (state)
            {
                case PlayerState.GROUND:
                    //my_shadow.visible = false;
                    if (dontMove)
                    {
                        velocity = Vector2.Zero;
                    }
                    else
                    {
                        ground_movement();  //modify player vels
                    }

                    //falling_logic();


                    //update_sentinels();

                    //for each(var t: * in Registry.subgroup_interactives) {
                    //    if (t != null)
                    //    {
                    //        if (t.active_region != null && t.active_region.overlaps(this))
                    //        {
                    //            actions_disabled = true;
                    //            break; // As soon as this is true we can stop checking
                    //        }
                    //    }
                    //}
                    //if (!hasFallen && !actions_disabled)
                    //{
                    //    if (action_latency > 0)
                    //    {
                    //        action_latency -= FlxG.elapsed;
                    //    }
                    //    update_actions(Registry.keywatch.ACTION_1, Registry.keywatch.JP_ACTION_1, Registry.bound_item_1);
                    //    update_actions(Registry.keywatch.ACTION_2, Registry.keywatch.JP_ACTION_2, Registry.bound_item_2);
                    //}
                    ground_animation();

                    //just_landed = false;
                    //dash_logic();

                    break;
                case PlayerState.AIR:
                    break;
                case PlayerState.RAFT:
                    break;
                case PlayerState.AUTO_JUMP:
                    break;
                case PlayerState.INTERACT:
                    break;
                case PlayerState.ENTER_FALL:
                    break;
                case PlayerState.LADDER:
                    break;
                default:
                    break;
            }
        }

        private void ground_animation()
        {
            switch (ANIM_STATE)
            {
                case PlayerAnimState.ANIM_ATK_R:
                case PlayerAnimState.ANIM_ATK_L:
                case PlayerAnimState.ANIM_ATK_U:
                case PlayerAnimState.ANIM_ATK_D:
                    //TODO broom stuff
                    break;
                case PlayerAnimState.ANIM_FALL:
                    Play("fall");
                    return;
                case PlayerAnimState.ANIM_DEAD:
                    return;
                case PlayerAnimState.as_idle:
                    if (idle_ticks > 0)
                    {
                        idle_ticks -= 1;
                        return;
                    }
                    if (velocity.Y < 0)
                    {
                        facing = Facing.UP;
                        Play("walk_u");
                    }
                    else if (velocity.Y > 0)
                    {
                        facing = Facing.DOWN;
                        Play("walk_d");
                    }
                    else if (velocity.X < 0)
                    {
                        facing = Facing.LEFT;
                        Play("walk_l");
                    }
                    else if (velocity.X > 0)
                    {
                        facing = Facing.RIGHT;
                        Play("walk_r");
                    }
                    else
                    {
                        switch (facing)
                        {
                            case Facing.UP:
                                Play("idle_u");
                                break;
                            case Facing.LEFT:
                                Play("idle_l");
                                break;
                            case Facing.DOWN:
                                Play("idle_d");
                                break;
                            case Facing.RIGHT:
                                Play("idle_r");
                                break;
                        }
                        break;
                    }
                    ANIM_STATE = PlayerAnimState.as_walk;
                    _curFrame = last_frame[(int)facing];
                    _curIndex = _curAnim.frames[_curFrame];

                    break;

                case PlayerAnimState.as_walk:
                    if (idle_ticks > 0)
                    {
                        idle_ticks--;
                    }
                    if (velocity.X == 0 && velocity.Y == 0)
                    {
                        ANIM_STATE = PlayerAnimState.as_idle;
                        last_frame[(int)facing] = _curFrame;
                        switch (facing)
                        {
                            case Facing.UP: Play("idle_u"); break;
                            case Facing.DOWN: Play("idle_d"); break;
                            case Facing.LEFT: Play("idle_l"); break;
                            case Facing.RIGHT: Play("idle_r"); break;
                        }
                    }
                    else
                    {
                        if (velocity.Y < 0)
                        {
                            if (facing != Facing.UP || (_curAnim != null && _curAnim.name != "walk_u"))
                            {
                                facing = Facing.UP;
                                Play("walk_u");
                            }
                        }
                        else if (velocity.Y > 0)
                        {
                            if (facing != Facing.DOWN)
                            {
                                facing = Facing.DOWN;
                                Play("walk_d");
                            }
                        }
                        else if (velocity.X < 0)
                        {
                            if (facing != Facing.LEFT)
                            {
                                facing = Facing.LEFT;
                                Play("walk_l");
                            }
                        }
                        else
                        {
                            if (facing != Facing.RIGHT)
                            {
                                facing = Facing.RIGHT;
                                Play("walk_r");
                            }
                        }
                    }
                    break;
                case PlayerAnimState.as_slumped:
                    break;
                default:
                    break;
            }
        }

        private void ground_movement()
        {
            set_init_vel();
        }

        private void set_init_vel(float mul = 1)
        {
            if (KeyInput.IsKeyPressed(Keys.Up))
            {
                velocity.Y = -mul * walkSpeed /* c_vel*/;
            }
            else if (KeyInput.IsKeyPressed(Keys.Down))
            {
                velocity.Y = mul * walkSpeed /* c_vel*/;

            }
            else
            {
                velocity.Y = 0;
            }

            if (KeyInput.IsKeyPressed(Keys.Left))
            {
                velocity.X = -mul * walkSpeed /* c_vel*/;
            }
            else if (KeyInput.IsKeyPressed(Keys.Right))
            {
                velocity.X = mul * walkSpeed /* c_vel*/;

            }
            else
            {
                velocity.X = 0;
            }

            velocity *= GameTimes.DeltaTime;
        }
    }
}
