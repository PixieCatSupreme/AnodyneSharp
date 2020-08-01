using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace AnodyneSharp.Entities
{
    public enum PlayerState
    {
        GROUND,
        AIR,
        AUTO_JUMP,
        INTERACT,
        ENTER_FALL,
        LADDER
    }

    public enum PlayerAnimState
    {
        ANIM_DEFAULT,
        ANIM_ATK,
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

        public const float FALL_TIMER_DEFAULT = 0.016f * 8 + 0.001f;

        public const float action_latency_max = 0.24f;
        public const float ATK_DELAY = 0.2f;
        public const float WATK_DELAY = 0.35f;
        public const float LATK_DELAY = 0.4f;

        public bool dontMove;

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

        private bool Do_bump => bump_timer > 0;
        private const float BUMP_VELOCITY = 50f;
        private const float BUMP_TIMER_MAX = 0.2f;
        private float bump_timer = 0;

        internal Vector2 grid_entrance;
        private Vector2 additionalVel;

        private float action_latency;

        public Broom broom;

        private PlayState parent;

        private bool actions_disabled;
        public bool skipBroom;
        private bool just_landed;
        private bool anim_air_did_down;
        private float jump_timer;
        private bool sinking;
        private bool dashing;

        private bool isSlipping;
        private bool hasFallen;
        private bool justFell;
        private bool fallingDisabled;
        private float fallTimer;
        private Vector2 fallPoint;

        internal float slowMul;
        internal int slowTicks;

        public bool ON_CONVEYER { get; private set; }

        public Player(PlayState parent)
            : base(Vector2.Zero, ORIGINAL_WIDTH, ORIGINAL_HEIGHT, Drawing.DrawOrder.ENTITIES)
        {
            DEATH_FRAME = 32;
            this.parent = parent;

            AddAnimation("walk_d", CreateAnimFrameArray(1, 0), 6, true);
            AddAnimation("walk_r", CreateAnimFrameArray(2, 3), 8, true);
            AddAnimation("walk_u", CreateAnimFrameArray(4, 5), 6, true);
            AddAnimation("walk_l", CreateAnimFrameArray(6, 7), 8, true);

            AddAnimation("attack_d", CreateAnimFrameArray(8, 9), 10, false);
            AddAnimation("attack_r", CreateAnimFrameArray(10, 11), 10, false);
            AddAnimation("attack_u", CreateAnimFrameArray(12, 13), 10, false);
            AddAnimation("attack_l", CreateAnimFrameArray(14, 15), 10, false);
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
            shadow = new Shadow(this, new Vector2(3, -1), fps: 20);
        }

        public void Reset()
        {
            if (GlobalState.AlwaysCellGraphics || GlobalState.CURRENT_MAP_NAME == "CELL")
            {
                SetTexture(Cell_Player_Sprite);
            }
            else
            {
                SetTexture(Player_Sprite);
            }

            broom.UpdateBroomType();
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
                Solid = false;
                walkSpeed = 200;

                if (KeyInput.JustPressedKey(Keys.V))
                {
                    visible = !visible;
                }
            }
            else
            {
                walkSpeed = 70;
                Solid = true;
            }

            if (!CommonConditions())
            {
                return;
            }

            Movement();

            if (slowTicks > 0)
            {
                slowTicks--;
            }
            else
            {
                slowMul = 1;
            }

            additionalVel = Vector2.Zero;

            base.Update();
        }

        private bool CommonConditions()
        {
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

            if (!Solid && justFell)
            {
                Solid = true;
                justFell = false;
            }

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

            return true;
        }

        public void Fall(Vector2 fallPoint)
        {
            if (!fallingDisabled && !isSlipping && !hasFallen)
            {
                isSlipping = true;
                fallTimer = FALL_TIMER_DEFAULT;
                this.fallPoint = fallPoint;
            }
        }

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
                        GroundMovement();  //modify player vels
                    }

                    if (isSlipping)
                    {
                        SlippingLogic();
                    }
                    else if (hasFallen)
                    {
                        ResetAfterFalling();
                    }


                    //update_sentinels();

                    if (!hasFallen && !actions_disabled)
                    {
                        Update_actions();
                        broom.Update();
                        broom.PostUpdate();
                    }
                    GroundAnimation();

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
                        AirMovement();
                        AirAnimation();
                    }

                    //dash_logic();
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

        private void SlippingLogic()
        {
            fallTimer -= GameTimes.DeltaTime;
            if (just_landed)
            {
                fallTimer = -1;
                just_landed = false;
            }

            if ((dashing && fallTimer < FALL_TIMER_DEFAULT / 2) || fallTimer < 0)
            {
                SoundManager.PlaySoundEffect("fall_in_hole");
                ANIM_STATE = PlayerAnimState.ANIM_FALL;
                hasFallen = true;
                isSlipping = false;
                dontMove = true;

                Position = fallPoint + new Vector2(3, 5);
            }
        }

        private void ResetAfterFalling()
        {
            fallTimer -= GameTimes.DeltaTime;

            if (_curFrame == 31)
            {
                Position = grid_entrance;
                hasFallen = false;
                Flicker(1);
                Play("idle_d");
                Solid = false;
                justFell = true;
                dontMove = false;
                ANIM_STATE = PlayerAnimState.as_idle;

                //TODO: Re-enable for Kaizo mode?
                //ReceiveDamage(1, false, false);
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
                if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) && action_latency <= 0 && !skipBroom)
                {
                    if (InventoryManager.EquippedBroom != BroomType.NONE && !broom.exists)
                    {
                        broom.visible_timer = 0;
                        broom.exists = true;
                        broom.facing = facing;
                        action_latency = action_latency_max;

                        broom.Play("stab", true);
                        SoundManager.PlaySoundEffect("swing_broom_1", "swing_broom_2", "swing_broom_3");

                        ANIM_STATE = PlayerAnimState.ANIM_ATK;
                    }
                    else if (InventoryManager.EquippedBroom == BroomType.Transformer)
                    {
                        //TODO transformer stuff
                    }

                }
                skipBroom = false;


                if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel) && !sinking)
                {
                    state = PlayerState.AIR;
                    shadow.visible = true;
                    broom.exists = false;
                }
            }
        }

        private void GroundAnimation()
        {
            //No change in animation during bump
            if (Do_bump) return;

            if (broom.finished && broom.exists)
            {
                //We just finished an attack
                ANIM_STATE = PlayerAnimState.as_idle;
            }
            switch (ANIM_STATE)
            {
                case PlayerAnimState.ANIM_ATK:
                    PlayFacing("attack");
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
                        PlayFacing("idle");
                    }
                    else
                    {
                        FacingFromVelocity();
                        PlayFacing("walk");

                        ANIM_STATE = PlayerAnimState.as_walk;
                        _curIndex = last_frame[(int)facing];
                        _curFrame = _curAnim.frames[_curIndex];
                    }
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
                        PlayFacing("idle");
                    }
                    else
                    {
                        FacingFromVelocity();
                        PlayFacing("walk");
                    }
                    break;
                case PlayerAnimState.as_slumped:
                    break;
                default:
                    break;
            }
        }

        private void AirAnimation()
        {
            if (!anim_air_did_up)
            {
                broom.exists = false;
                anim_air_did_up = true;

                if (ON_CONVEYER)
                {
                    SoundManager.PlaySoundEffect("puddle_up");
                }
                else
                {
                    SoundManager.PlaySoundEffect("player_jump_up");
                }

                shadow.Play("get_small");
                ANIM_STATE = PlayerAnimState.as_idle; // Always land in idle state.
                PlayFacing("jump");
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
                if (ON_CONVEYER)
                {
                    SoundManager.PlaySoundEffect("puddle_down");
                }
                else
                {
                    SoundManager.PlaySoundEffect("player_jump_down");
                }
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
                case "attack_l":
                    offset.X = 4;
                    break;
                case "walk_r":
                case "attack_r":
                case "walk_d":
                case "attack_d":
                case "walk_u":
                case "attack_u":
                    offset.X = 3;
                    break;
            }
        }

        public override void ReloadTexture(bool ignoreChaos = false)
        {
            base.ReloadTexture();
            broom.ReloadTexture();
        }

        private void GroundMovement()
        {
            Set_init_vel();

            if (Do_bump)
            {
                bump_timer -= GameTimes.DeltaTime;
                if (bump_timer > 0)
                {
                    velocity = -FacingDirection(facing) * BUMP_VELOCITY;
                }
            }

            velocity += additionalVel;
            velocity *= slowMul;
        }

        private void AirMovement()
        {
            Set_init_vel(0.83f);

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
            if (broom.exists)
            {
                velocity = Vector2.Zero;
                return;
            }

            if (KeyInput.IsRebindableKeyPressed(KeyFunctions.Up))
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
            else if (KeyInput.IsRebindableKeyPressed(KeyFunctions.Down))
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

            if (KeyInput.IsRebindableKeyPressed(KeyFunctions.Left))
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
            else if (KeyInput.IsRebindableKeyPressed(KeyFunctions.Right))
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
            if (velocity.X != 0 && velocity.Y != 0)
            {
                velocity *= 0.7f;
            }
        }

        internal void ReceiveDamage(int amount, bool knockback = true, bool playSound = true)
        {
            if (!invincible)
            {
                GlobalState.CUR_HEALTH -= amount;
                invincible = true;
                invincibility_time = INVINCIBLE_MAX;
                Flicker(INVINCIBLE_MAX);

                if (playSound)
                {
                    SoundManager.PlaySoundEffect("player_hit_1");
                }
                if (knockback)
                {
                    bump_timer = BUMP_TIMER_MAX;
                }

            }
        }

        public void BeIdle()
        {
            ANIM_STATE = PlayerAnimState.as_idle;
            idle_ticks = 5;
            PlayFacing("idle");
        }

        private Facing FacingFromVelocity()
        {
            if (velocity == Vector2.Zero)
            {
                return facing;
            }

            if (velocity.Y < 0)
            {
                facing = Facing.UP;
            }
            else if (velocity.Y > 0)
            {
                facing = Facing.DOWN;
            }
            else if (velocity.X < 0)
            {
                facing = Facing.LEFT;
            }
            else
            {
                facing = Facing.RIGHT;
            }

            return facing;

        }
    }
}
