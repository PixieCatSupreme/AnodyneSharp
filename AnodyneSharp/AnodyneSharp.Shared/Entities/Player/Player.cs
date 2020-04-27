using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AnodyneSharp.Entities
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

    [Collision(MapCollision = true)]
    public class Player : Entity
    {
        public const string Player_Sprite = "young_player";
        public const string Cell_Player_Sprite = "young_player_cell";
        public const string Player_Mobile_Indicator_Sprite = "player_mobile_indicator";

        public const string Player_reflection_Sprite = "young_player_reflection";
        public const string Broom_reflection_sprite = "broom_reflection";

        private const float jump_period = 0.4f * 1.15f; //length of jump

        public const float action_latency_max = 0.24f;
        public const float ATK_DELAY = 0.2f;
        public const float WATK_DELAY = 0.35f;
        public const float LATK_DELAY = 0.4f;

        internal bool dontMove;

        internal float grid_entrance_y;

        public const int DEFAULT_Y_OFFSET = 4;
        public const string TRANSITION_IDLE = "idle_u";

        private const int ORIGINAL_WIDTH = 16;
        private const int ORIGINAL_HEIGHT = 16;
        private const int HITBOX_HEIGHT = 12;
        private const int HITBOX_WIDTH = 10;

        private int walkSpeed = 70;
        private bool anim_air_did_up;

        /* Animation state */
        public PlayerAnimState ANIM_STATE;
        private int idle_ticks = 0;

        private int[] last_frame = { 0, 0, 0, 0 };
        public int DEATH_FRAME;

        public PlayerState state;
        internal bool invincible;
        private const float INVINCIBLE_MAX = 0.5f;
        private float invincibility_time = 0;

        internal float grid_entrance_x;
        private Vector2 additionalVel;

        private float action_latency;

        public Broom broom;

        private PlayState parent;
        private bool hasFallen;
        private bool actions_disabled;
        private bool just_landed;
        private bool anim_air_did_down;
        private float jump_timer;
        private bool sinking;

        public Player(PlayState parent)
            : base(Vector2.Zero, ORIGINAL_WIDTH, ORIGINAL_HEIGHT, Drawing.DrawOrder.ENTITIES)
        {
            DEATH_FRAME = 32;
            this.parent = parent;

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
            offset = new Vector2(3, DEFAULT_Y_OFFSET);
            width = HITBOX_WIDTH;

            Play("idle_u");
            ANIM_STATE = PlayerAnimState.as_idle;
            facing = Facing.UP;

            broom = new Broom(this);
            shadow = new Base.Shadow(this, new Vector2(3, -1), fps: 20);
        }

        public void Reset()
        {
            if (GlobalState.CURRENT_MAP_NAME == "TRAIN")
            {
                SetTexture(Cell_Player_Sprite);
            }
            else
            {
                SetTexture(Player_Sprite);
            }
        }

        public override void Draw()
        {
            base.Draw();

            broom.Draw();
        }

        public override void Update()
        {
            if (invincibility_time > 0)
            {
                invincibility_time -= GameTimes.DeltaTime;
                if (invincibility_time <= 0) invincible = false;
            }
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

            additionalVel = Vector2.Zero;

            base.Update();
        }

        //      private bool Common_conditions() 
        //{
        //	//Registry.CUR_HEALTH = health_bar.cur_health;

        //	if (parent.state == parent.S_TRANSITION) {
        //		dontMove = true;
        //		velocity.X = velocity.Y = 0;
        //		//if (ON_RAFT) {
        //		//	raft.x = x - 2;
        //		//	raft.y = y - 3;
        //		//	conveyer_fudge_factor = 5; // <_<
        //		//}

        //		if (state ==  PlayerState.AIR) {
        //			my_shadow.x = x + JUMP_SHADOW_X_OFF + 1;
        //			my_shadow.y = y + JUMP_SHADOW_Y_OFF - 3;
        //		}
        //		return false;
        //	}

        //	if (parent.SWITCH_MAPS || !alive)
        //          {
        //		base.Update();
        //		return false;
        //	}

        //	if (!solid && just_fell)
        //          {
        //		solid = true;
        //		just_fell = false;
        //	}

        //	if (invincible_timer > 0)
        //          {
        //              invincible_timer -= GameTimes.DeltaTime;
        //	}
        //          else
        //          {
        //		invincible = false;
        //		if (!GlobalState.FUCK_IT_MODE_ON)
        //              {
        //			visible = true;
        //		}
        //	}

        //	return true;
        //}

        private void Movement()
        {
            switch (state)
            {
                case PlayerState.GROUND:
                    shadow.visible = false;
                    if (dontMove)
                    {
                        velocity = Vector2.Zero;
                    }
                    else
                    {
                        Ground_movement();  //modify player vels
                    }

                    //falling_logic();


                    //update_sentinels();

                    if (!hasFallen && !actions_disabled)
                    {
                        Update_actions();
                        broom.Update();
                        broom.PostUpdate();
                    }
                    Ground_animation();

                    just_landed = false;
                    //dash_logic();

                    break;
                case PlayerState.AIR:
                    if (dontMove)
                    {
                        velocity = Vector2.Zero;
                    }
                    else
                    {
                        Air_movement();
                        Air_animation();
                    }

                    //dash_logic();
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

        private void Update_actions()
        {
            if (action_latency > 0)
            {
                action_latency -= GameTimes.DeltaTime;
            }

            if (state != PlayerState.AIR)
            {
                if (KeyInput.CanPressKey(Keys.C) && action_latency <= 0)
                {
                    if (InventoryState.EquippedBroom != BroomType.NONE && !broom.visible)
                    {
                        broom.visible_timer = 0;
                        broom.visible = true;
                        broom.facing = facing;
                        action_latency = action_latency_max;

                        broom.Play("stab", true);

                        switch (facing)
                        {
                            case Facing.LEFT: ANIM_STATE = PlayerAnimState.ANIM_ATK_L; break;
                            case Facing.UP: ANIM_STATE = PlayerAnimState.ANIM_ATK_U; break;
                            case Facing.DOWN: ANIM_STATE = PlayerAnimState.ANIM_ATK_D; break;
                            case Facing.RIGHT: ANIM_STATE = PlayerAnimState.ANIM_ATK_R; break;
                        }
                    }
                    else if (InventoryState.EquippedBroom == BroomType.Transformer)
                    {
                        //TODO transformer stuff
                    }

                }


                if (KeyInput.CanPressKey(Keys.X) && !sinking)
                {
                    state = PlayerState.AIR;
                    shadow.visible = true;
                    broom.visible = false;
                }
            }
        }

        private void Ground_animation()
        {
            if (broom.finished && broom.visible)
            {
                //We just finished an attack
                ANIM_STATE = PlayerAnimState.as_idle;
            }
            switch (ANIM_STATE)
            {
                case PlayerAnimState.ANIM_ATK_D:
                    Play("attack_down");
                    break;
                case PlayerAnimState.ANIM_ATK_L:
                    Play("attack_left");
                    break;
                case PlayerAnimState.ANIM_ATK_R:
                    Play("attack_right");
                    break;
                case PlayerAnimState.ANIM_ATK_U:
                    Play("attack_up");
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
                        break;
                    }
                    if (velocity == Vector2.Zero)
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

                    ANIM_STATE = PlayerAnimState.as_walk;
                    _curIndex = last_frame[(int)facing];
                    _curFrame = _curAnim.frames[_curIndex];

                    break;

                case PlayerAnimState.as_walk:
                    if (idle_ticks > 0)
                    {
                        idle_ticks--;
                    }
                    if (velocity == Vector2.Zero)
                    {
                        ANIM_STATE = PlayerAnimState.as_idle;
                        last_frame[(int)facing] = _curIndex;
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
                            if (facing != Facing.UP)
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

        private void Air_animation()
        {
            if (!anim_air_did_up)
            {
                broom.visible = false;
                anim_air_did_up = true;

                //if (ON_CONVEYER)
                //{
                //    Registry.sound_data.puddle_up.play();
                //}
                //else
                //{
                //    play_sfx("jump_up");
                //}

                shadow.Play("get_small");
                ANIM_STATE = PlayerAnimState.as_idle; // Always land in idle state.
                switch (facing)
                { // Play the jump animation
                    case Facing.UP:
                        Play("jump_u");
                        break;
                    case Facing.DOWN:
                        Play("jump_d");
                        break;
                    case Facing.LEFT:
                        Play("jump_l");
                        break;
                    case Facing.RIGHT:
                        Play("jump_r");
                        break;
                }
            }

            if (!anim_air_did_down && jump_timer > jump_period / 2)
            {
                shadow.Play("get_big");
                anim_air_did_down = true;
            }

            offset.Y = DEFAULT_Y_OFFSET + (((-4 * 24) / (jump_period * jump_period)) * jump_timer * (jump_timer - jump_period));
            jump_timer += GameTimes.DeltaTime;

            if (jump_timer > jump_period)
            {
                jump_timer = 0;
                //if (ON_CONVEYER)
                //{
                //    Registry.sound_data.puddle_down.play();
                //}
                //else
                //{
                //    play_sfx("jump_down");
                //}
                state = PlayerState.GROUND;

                //my_shadow.visible = false;
                offset.Y = DEFAULT_Y_OFFSET;
                just_landed = true;
                anim_air_did_down = anim_air_did_up = false;
            }
        }

        protected override void AnimationChanged(string name)
        {
            switch (name)
            {
                case "walk_l":
                case "attack_left":
                    offset.X = 4;
                    break;
                case "walk_r":
                case "attack_right":
                case "walk_d":
                case "attack_down":
                case "walk_u":
                case "attack_up":
                    offset.X = 3;
                    break;
            }
        }

        private void Ground_movement()
        {
            Set_init_vel();

            if (velocity.X != 0 && velocity.Y != 0)
            {
                velocity *= .7f;
            }

            velocity += additionalVel;
        }

        private void Air_movement()
        {
            Set_init_vel(0.83f);

            if (velocity != Vector2.Zero)
            {
                velocity *= .7f;
            }

            //if (ON_RAFT)
            //{
            //    if (ON_CONVEYER != NONE)
            //    {
            //        raft.x = x - 2;
            //        raft.y = y - 2;
            //    }
            //}

            velocity += additionalVel;
        }

        private void Set_init_vel(float mul = 1)
        {
            if (broom.visible)
            {
                velocity = Vector2.Zero;
                return;
            }

            if (KeyInput.IsKeyPressed(Keys.Up))
            {
                velocity.Y = -mul;
                if ((touching & Touching.UP) != 0)
                {
                    Touching tl = parent.GetTileCollisionFlags(Position + new Vector2(0, -8));
                    Touching tr = parent.GetTileCollisionFlags(Position + new Vector2(width, -8));
                    if ((Position.X + width) % 16 < 6 && (tl & Touching.DOWN) == 0)
                    {
                        additionalVel.X = -30;
                    }
                    else if ((Position.X % 16) > 9 && (tr & Touching.DOWN) == 0)
                    {
                        additionalVel.X = 30;
                    }
                }

            }
            else if (KeyInput.IsKeyPressed(Keys.Down))
            {
                velocity.Y = mul;
                if ((touching & Touching.DOWN) != 0)
                {
                    Touching bl = parent.GetTileCollisionFlags(Position + new Vector2(0, height + 8));
                    Touching br = parent.GetTileCollisionFlags(Position + new Vector2(width, height + 8));
                    if ((Position.X + width) % 16 < 6 && (bl & Touching.UP) == 0)
                    {
                        additionalVel.X = -30;
                    }
                    else if ((Position.X % 16) > 9 && (br & Touching.UP) == 0)
                    {
                        additionalVel.X = 30;
                    }
                }
            }
            else
            {
                velocity.Y = 0;
            }

            if (KeyInput.IsKeyPressed(Keys.Left))
            {
                velocity.X = -mul;
                if ((touching & Touching.LEFT) != 0)
                {
                    Touching tl2 = parent.GetTileCollisionFlags(Position + new Vector2(-8, 0));
                    Touching bl2 = parent.GetTileCollisionFlags(Position + new Vector2(-8, height));
                    if ((Position.Y + height) % 16 < 6 && (tl2 & Touching.RIGHT) == 0)
                    {
                        additionalVel.Y = -30;
                    }
                    else if (Position.Y % 16 > 9 && (bl2 & Touching.RIGHT) == 0)
                    {
                        additionalVel.Y = 30;
                    }
                }
            }
            else if (KeyInput.IsKeyPressed(Keys.Right))
            {
                velocity.X = mul;
                if ((touching & Touching.RIGHT) != 0)
                {
                    Touching tr2 = parent.GetTileCollisionFlags(Position + new Vector2(width + 8, 0));
                    Touching br2 = parent.GetTileCollisionFlags(Position + new Vector2(width + 8, height));
                    if ((Position.Y + height) % 16 < 6 && (tr2 & Touching.LEFT) == 0)
                    {
                        additionalVel.Y = -30;
                    }
                    else if (Position.Y % 16 > 9 && (br2 & Touching.LEFT) == 0)
                    {
                        additionalVel.Y = 30;
                    }
                }
            }
            else
            {
                velocity.X = 0;
            }

            velocity *= walkSpeed /* c_vel*/;
        }

        internal void ReceiveDamage(int amount)
        {
            if (!invincible)
            {
                GlobalState.CUR_HEALTH -= amount;
                invincible = true;
                invincibility_time = INVINCIBLE_MAX;
            }
        }
    }
}
