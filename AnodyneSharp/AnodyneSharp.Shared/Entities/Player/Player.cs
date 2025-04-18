﻿using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Lights;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.States;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AnodyneSharp.Entities
{
    public enum PlayerState
    {
        GROUND,
        AIR,
        AUTO_JUMP,
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

    [Collision(typeof(Dust), MapCollision = true)]
    public class Player : Entity
    {
        public const string Player_Sprite = "young_player";
        public const string Cell_Player_Sprite = "young_player_cell";
        public const string Player_Mobile_Indicator_Sprite = "player_mobile_indicator";

        public const string Player_reflection_Sprite = "young_player_reflection";
        public const string Broom_reflection_sprite = "broom_reflection";

        public const string DrowningDamageDealer = "Drowned";

        private const float jump_period = 0.4f * 1.15f; //length of jump

        public static float FALL_TIMER_DEFAULT => (0.016f * 8 + 0.001f) * (GlobalState.settings.extended_coyote ? 2 : 1);

        public const float action_latency_max = 0.24f;
        public const float ATK_DELAY = 0.2f;
        public const float WATK_DELAY = 0.35f;
        public const float LATK_DELAY = 0.4f;

        public bool dontMove;

        public const int DEFAULT_Y_OFFSET = 4;
        public const string TRANSITION_IDLE = "idle_u";

        private const int HITBOX_HEIGHT = 12;
        private const int HITBOX_WIDTH = 10;

        private int walkSpeed = 70;

        /* Animation state */
        public PlayerAnimState ANIM_STATE;
        private int idle_ticks = 0;

        public PlayerState state;
        internal bool invincible;
        private const float INVINCIBLE_MAX = 0.5f;
        private float invincibility_time = 0;

        private bool Do_bump => bump_timer > 0;
        private const float BUMP_VELOCITY = 50f;
        private const float BUMP_TIMER_MAX = 0.2f;
        private float bump_timer = 0;

        internal Vector2 grid_entrance;
        public Vector2 additionalVel;

        public bool enter_fall = false;
        public bool fall_smack = false;

        private float action_latency;

        public Broom broom;
        public Transformer transformer;
        public Entity follower;

        public bool actions_disabled;
        public bool skipBroom;
        private IEnumerator jump_anim;
        public bool JustLanded { get; private set; }

        private bool Dashing => dash_state != Vector2.Zero;
        Vector2 dash_state = Vector2.Zero;

        private bool ON_HOLE;
        private bool isSlipping;
        private bool hasFallen;
        private bool justFell;
        private float fallTimer;
        private Vector2 fallPoint;

        internal float slowMul;
        internal int slowTicks;

        public bool reversed;
        private float _revTimer;

        private const float step_timer_max = 0.5f;
        private float step_noise_timer = 0f;

        public bool ON_CONVEYOR { get; private set; }
        private bool IS_SINKING = false;
        private Foot_Overlay foot_overlay;
        private PlayerReflection reflection;

        public Dust raft;
        public PlayerLight light;

        public static AnimatedSpriteRenderer GetSprite(bool cell) => new(cell ? Cell_Player_Sprite : Player_Sprite, 16, 16,
            new Anim("walk_d", new int[] { 1, 0 }, 6),
            new Anim("walk_r", new int[] { 2, 3 }, 8),
            new Anim("walk_u", new int[] { 4, 5 }, 6),
            new Anim("walk_l", new int[] { 6, 7 }, 8),
            new Anim("attack_d", new int[] { 8, 9 }, 10, false),
            new Anim("attack_r", new int[] { 10, 11 }, 10, false),
            new Anim("attack_u", new int[] { 12, 13 }, 10, false),
            new Anim("attack_l", new int[] { 14, 15 }, 10, false),
            new Anim("fall", new int[] { 28, 29, 30, 31 }, 5, false),
            new Anim("slumped", new int[] { 32 }, 1),
            new Anim("whirl", new int[] { 25, 26, 27, 24 }, 12),
            new Anim("idle_d", new int[] { 24 }, 4),
            new Anim("idle_r", new int[] { 25 }, 4),
            new Anim("idle_u", new int[] { 26 }, 4),
            new Anim("idle_l", new int[] { 27 }, 4),
            new Anim("jump_d", new int[] { 16, 17 }, 4),
            new Anim("jump_r", new int[] { 18, 19 }, 4),
            new Anim("jump_u", new int[] { 20, 21 }, 4),
            new Anim("jump_l", new int[] { 22, 23 }, 4),
            new Anim("enter_fall", new int[] { 2 },1),
            new Anim("idle_climb", new int[] { 33 }, 1),
            new Anim("climb", new int[] { 34, 35 }, 8),
            new Anim("die", new int[] { 25, 26, 27, 24, 25, 26, 27, 24, 25, 26, 27, 32 },12, false)
            );

        public Player()
            : base(Vector2.Zero, GetSprite(false), Drawing.DrawOrder.ENTITIES)
        {
            height = HITBOX_HEIGHT;
            offset = new Vector2(3, DEFAULT_Y_OFFSET);
            width = HITBOX_WIDTH;

            Play("idle_u");
            ANIM_STATE = PlayerAnimState.as_idle;
            facing = Facing.UP;

            broom = new(this);
            transformer = new(this);
            shadow = new(this, new Vector2(3, -1), fps: 20);
            foot_overlay = new(this);
            reflection = new(this);
            light = new(this);

        }

        public void Reset(bool fullReset = true)
        {
            if (fullReset)
            {
                BeIdle();

                broom.UpdateBroomType();
                broom.dust = null;
                foot_overlay.OnMapChange();

                velocity = Vector2.Zero;
                state = PlayerState.GROUND;
                offset = new Vector2(3, DEFAULT_Y_OFFSET);

                raft = null;
                follower = null;

                broom.exists = false;

                actions_disabled = false;

                Solid = true;
                isSlipping = false;
                hasFallen = false;
                justFell = false;

                IS_SINKING = false;
                dash_state = Vector2.Zero;

                JustLanded = false;
                dontMove = false;

                reversed = false;

                slowMul = 1;

                if(enter_fall)
                {
                    state = PlayerState.ENTER_FALL;
                    offset.Y = 150;
                    SoundManager.PlaySoundEffect("fall_1");
                    angularVelocity = 400;
                    Play("enter_fall");
                }
                enter_fall = false;
            }

            if (GlobalState.IsCell)
            {
                SetTexture(Cell_Player_Sprite, 16, 16);
            }
            else
            {
                SetTexture(Player_Sprite, 16, 16);
            }
        }

        public override void Update()
        {
            JustLanded = false;
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
                if (walkSpeed != 70) Solid = true;
                walkSpeed = 70;
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

            if (reversed)
            {
                _revTimer += GameTimes.DeltaTime;

                if (_revTimer > 0.9f)
                {
                    reversed = false;
                    _revTimer = 0;
                    GlobalState.wave.active = false;
                }
            }

            base.Update();
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            ON_CONVEYOR = false;
            IS_SINKING = false;
            ON_HOLE = false;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            List<Entity> ret = new() { broom, transformer, foot_overlay, reflection, light };
            if (raft != null) ret.Add(raft);
            if (follower != null) ret.Add(follower);
            return ret;
        }

        private bool CommonConditions()
        {

            if (!Solid && justFell)
            {
                Solid = true;
                justFell = false;
            }

            return true;
        }

        public override void Fall(Vector2 fallPoint)
        {
            if (state != PlayerState.AIR && !isSlipping && !hasFallen && !GlobalState.FUCK_IT_MODE_ON)
            {
                isSlipping = true;
                fallTimer = JustLanded ? -1 : FALL_TIMER_DEFAULT;
            }
            this.fallPoint = fallPoint;
            ON_HOLE = true;
        }

        public void DontFall()
        {
            isSlipping = false;
        }

        public void Dash(Facing facing)
        {
            static void CheckDash(ref float dash_val, float addition)
            {
                if(dash_val == 0)
                {
                    dash_val = addition * 1.3f;
                    SoundManager.PlaySoundEffect("dash_pad_1");
                }
                else if(dash_val * addition < 0) //different sign
                {
                    dash_val = 0;
                }
                else if(MathF.Abs(dash_val) < 1.5f)
                {
                    dash_val = addition * 1.77f;
                    SoundManager.PlaySoundEffect("dash_pad_2");
                }
            }

            switch(facing)
            {
                case Facing.UP:
                case Facing.DOWN:
                    CheckDash(ref dash_state.Y, FacingDirection(facing).Y);
                    break;
                case Facing.LEFT:
                case Facing.RIGHT:
                    CheckDash(ref dash_state.X, FacingDirection(facing).X);
                    break;
            }
        }

        public override void Puddle()
        {
            ON_CONVEYOR = true;
        }

        public override void Ladder()
        {
            if (state == PlayerState.GROUND)
            {
                state = PlayerState.LADDER;
            }
        }

        public override void SlowTile()
        {
            slowMul = 0.5f;
        }

        public override void Conveyor(Touching direction)
        {
            ON_CONVEYOR = true;
            const float CONVEYOR_VEL = 35f;
            if (state != PlayerState.AIR)
            {
                additionalVel = direction switch
                {
                    Touching.DOWN => new Vector2(0, CONVEYOR_VEL),
                    Touching.UP => new Vector2(0, -CONVEYOR_VEL),
                    Touching.RIGHT => new Vector2(CONVEYOR_VEL, 0),
                    Touching.LEFT => new Vector2(-CONVEYOR_VEL, 0),
                    _ => Vector2.Zero
                };

                if (raft == null && !GlobalState.FUCK_IT_MODE_ON)
                {
                    slowMul = 0.5f;
                    IS_SINKING = true;
                }
            }
        }

        public override void Collided(Entity other)
        {
            if (other is Dust d)
            {
                if (raft == null && d.Hitbox.Contains(this.Center) && d.ON_CONVEYOR && ON_CONVEYOR)
                {
                    raft = d;
                    d.IS_RAFT = true;
                    IS_SINKING = false;
                    y_push = 0;
                }
            }
        }

        private void Movement()
        {
            switch (state)
            {
                case PlayerState.GROUND:
                    shadow.visible = false;

                    GroundMovement();  //set player vels, also updates bump timer
                    if (dontMove)
                    {
                        velocity = Vector2.Zero;
                    }

                    if (isSlipping)
                    {
                        SlippingLogic();
                    }
                    else if (hasFallen)
                    {
                        ResetAfterFalling();
                    }

                    if (!dontMove) //Don't do sinking logic during transition
                    {
                        if (IS_SINKING)
                        {
                            SinkingLogic();
                        }
                        else
                        {
                            y_push = 0;
                        }

                        if (raft != null)
                        {
                            if (ON_CONVEYOR)
                            {
                                raft.Position = Position - new Vector2(2, 2);
                            }
                            else if (velocity != Vector2.Zero && !isSlipping && !hasFallen)
                            {
                                Vector2 vel = velocity;
                                vel.Normalize();
                                raft.Position -= vel * 3;
                                raft.IS_RAFT = false;
                                raft = null;
                            }
                        }
                    }

                    if (raft != null && ON_CONVEYOR)
                    {
                        raft.Position = Position - Vector2.One * 2;
                    }

                    step_noise_timer -= GameTimes.DeltaTime;
                    if (step_noise_timer < 0f && velocity != Vector2.Zero && ON_CONVEYOR)
                    {
                        if (IS_SINKING)
                        {
                            SoundManager.PlaySoundEffect("water_step");
                            step_noise_timer = step_timer_max;
                        }
                        else
                        {
                            SoundManager.PlaySoundEffect("puddle_step");
                            step_noise_timer = step_timer_max / 2;
                        }
                    }

                    //update_sentinels();

                    if (!hasFallen && !actions_disabled && !GlobalState.InDeathRoom)
                    {
                        Update_actions();
                    }
                    GroundAnimation();
                    break;
                case PlayerState.AIR:
                    if (dontMove)
                    {
                        velocity = Vector2.Zero;
                    }
                    else
                    {
                        AirMovement();
                        jump_anim.MoveNext();
                    }

                    DashLogic();
                    break;
                case PlayerState.AUTO_JUMP:
                    if (!jump_anim.MoveNext())
                    {
                        state = PlayerState.GROUND;

                    }
                    break;
                case PlayerState.ENTER_FALL:
                    velocity = Vector2.Zero;
                    if(MathUtilities.MoveTo(ref offset.Y,0,102) && fall_smack)
                    {
                        state = PlayerState.GROUND;
                        Play("slumped");
                        ANIM_STATE = PlayerAnimState.as_slumped;
                        angularVelocity = 0;
                        rotation = 0;
                        GlobalState.screenShake.Shake(0.05f, 0.4f);
                        SoundManager.PlaySoundEffect("hit_ground_1");
                        fall_smack = false;
                    }
                    break;
                case PlayerState.LADDER:
                    LadderLogic();
                    break;
                default:
                    break;
            }
        }

        private void DashLogic()
        {
            if(dash_state.X > 0 && !KeyInput.IsRebindableKeyPressed(KeyFunctions.Right))
            {
                dash_state.X = 0;
            }
            else if(dash_state.X < 0 && !KeyInput.IsRebindableKeyPressed(KeyFunctions.Left))
            {
                dash_state.X = 0;
            }
            else if(dash_state.X != 0)
            {
                velocity.X = walkSpeed * dash_state.X;
            }

            if (dash_state.Y > 0 && !KeyInput.IsRebindableKeyPressed(KeyFunctions.Down))
            {
                dash_state.Y = 0;
            }
            else if (dash_state.Y < 0 && !KeyInput.IsRebindableKeyPressed(KeyFunctions.Up))
            {
                dash_state.Y = 0;
            }
            else if(dash_state.Y != 0)
            {
                velocity.Y = walkSpeed * dash_state.Y;
            }
        }

        private void LadderLogic()
        {
            velocity = Vector2.Zero;

            if (!dontMove)
            {
                Play("climb");
                if (KeyInput.IsRebindableKeyPressed(KeyFunctions.Up))
                {
                    velocity.Y = -walkSpeed * 0.7f;
                }
                else if (KeyInput.IsRebindableKeyPressed(KeyFunctions.Down))
                {
                    velocity.Y = walkSpeed * 0.7f;
                }
                else
                {
                    Play("idle_climb");
                }

                if (KeyInput.IsRebindableKeyPressed(KeyFunctions.Left))
                {
                    velocity.X = -walkSpeed;
                }
                else if (KeyInput.IsRebindableKeyPressed(KeyFunctions.Right))
                {
                    velocity.X = walkSpeed;
                }

                step_noise_timer -= GameTimes.DeltaTime;
                if (step_noise_timer < 0 && velocity.Y != 0)
                {
                    step_noise_timer = 0.2f;
                    SoundManager.PlaySoundEffect("ladder_step_1", "ladder_step_2");
                }
            }
            else
            {
                Play("idle_climb");
            }
            state = PlayerState.GROUND; //reset will stick as soon as we're not touching a ladder anymore
            ANIM_STATE = PlayerAnimState.as_idle;
        }

        private void SinkingLogic()
        {
            y_push += GameTimes.DeltaTime * 16 / 3;
            if (y_push > 16)
            {
                y_push = 0;
                Position = grid_entrance;
                ReceiveDamage(1,DrowningDamageDealer,knockback:false);
            }
        }

        private void SlippingLogic()
        {
            if(!ON_HOLE)
            {
                isSlipping = false;
                return;
            }
            
            fallTimer -= GameTimes.DeltaTime;

            if ((Dashing && fallTimer < FALL_TIMER_DEFAULT / 2) || fallTimer < 0)
            {
                SoundManager.PlaySoundEffect("fall_in_hole");
                ANIM_STATE = PlayerAnimState.ANIM_FALL;
                dash_state = Vector2.Zero;
                hasFallen = true;
                isSlipping = false;
                dontMove = true;

                Position = fallPoint + new Vector2(3, 5);
            }
        }

        private void ResetAfterFalling()
        {
            if (Frame == 31)
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
                    if (GlobalState.inventory.EquippedBroom == BroomType.Transformer)
                    {
                        transformer.OnAction();
                    }
                    else if (GlobalState.inventory.EquippedBroom != BroomType.NONE && !broom.exists)
                    {
                        broom.Attack();
                        action_latency = action_latency_max;

                        ANIM_STATE = PlayerAnimState.ANIM_ATK;
                    }

                }
                skipBroom = false;


                if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel) && !IS_SINKING && GlobalState.inventory.CanJump)
                {
                    state = PlayerState.AIR;
                    jump_anim = JumpAnim(jump_period);
                    shadow.visible = true;
                    broom.exists = false;
                    isSlipping = false;
                }
            }
        }

        private void GroundAnimation()
        {
            //No change in animation during bump
            if (Do_bump) return;

            if (ANIM_STATE == PlayerAnimState.ANIM_ATK && broom.AnimFinished)
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
                    break;
                case PlayerAnimState.ANIM_DEAD:
                    break;
                case PlayerAnimState.as_idle:
                    if (idle_ticks > 0)
                    {
                        idle_ticks -= 1;
                        break;
                    }
                    if (velocity != Vector2.Zero)
                    {
                        FacingFromVelocity();
                        PlayFacing("walk");

                        ANIM_STATE = PlayerAnimState.as_walk;
                    }
                    else
                    {
                        PlayFacing("idle");
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
                        PlayFacing("idle");
                    }
                    else
                    {
                        FacingFromVelocity();
                        PlayFacing("walk");
                    }
                    break;
                case PlayerAnimState.as_slumped:
                    Play("slumped");
                    if (velocity != Vector2.Zero)
                    {
                        ANIM_STATE = PlayerAnimState.as_walk;
                        PlayFacing("walk");
                    }
                    break;
                default:
                    break;
            }
        }

        private IEnumerator JumpAnim(float period, Vector2? target_pos = null, int jump_height = 24)
        {
            Vector2 base_pos = Position;
            broom.exists = false;
            if (ON_CONVEYOR)
            {
                SoundManager.PlaySoundEffect("puddle_up");
            }
            else
            {
                SoundManager.PlaySoundEffect("player_jump_up");
            }
            Parabola_Thing parabola = new(this,jump_height,period);
            ANIM_STATE = PlayerAnimState.as_idle; // Always land in idle state.
            PlayFacing("jump");

            while (!parabola.Tick())
            {
                if (target_pos.HasValue)
                {
                    Position.X = MathHelper.Lerp(base_pos.X, target_pos.Value.X, parabola.Progress());
                    Position.Y = MathHelper.Lerp(base_pos.Y, target_pos.Value.Y, parabola.Progress());
                }
                yield return null;
            }

            if (target_pos.HasValue)
            {
                Position = target_pos.Value;
            }

            if (ON_CONVEYOR)
            {
                SoundManager.PlaySoundEffect("puddle_down");
            }
            else
            {
                SoundManager.PlaySoundEffect("player_jump_down");
            }

            offset.Y = DEFAULT_Y_OFFSET;

            yield return null;

            JustLanded = true;
            state = PlayerState.GROUND;
            Solid = true;
            rotation = 0;
            angularVelocity = 0;

            yield break;
        }

        public void AutoJump(float period, Vector2 target, int height = 24, float rotation_speed = 0f)
        {
            jump_anim = JumpAnim(period, target, height);
            Solid = false;
            state = PlayerState.AUTO_JUMP;
            velocity = Vector2.Zero;
            angularVelocity = rotation_speed;
            y_push = 0;
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
            DashLogic();
        }

        private void AirMovement()
        {
            Set_init_vel(0.83f);

            if (raft != null)
            {
                if (ON_CONVEYOR)
                {
                    raft.Position = Position - Vector2.One * 2;
                }
            }

            velocity += additionalVel;
        }

        private void Set_init_vel(float mul = 1)
        {
            if (broom.exists)
            {
                velocity = Vector2.Zero;
                return;
            }

            if (KeyInput.IsRebindableKeyPressed(Reverse(KeyFunctions.Up)))
            {
                velocity.Y = -mul;
                if ((touching & Touching.UP) != 0)
                {
                    Touching tl = GlobalState.Map.GetCollisionData(Position + new Vector2(0, -8));
                    Touching tr = GlobalState.Map.GetCollisionData(Position + new Vector2(width, -8));
                    if(!tl.HasFlag(Touching.DOWN) && !tr.HasFlag(Touching.DOWN))
                    {
                        //nothing
                    }
                    else if ((Position.X + width) % 16 < 6 && (tl & Touching.DOWN) == 0)
                    {
                        additionalVel.X -= 30;
                    }
                    else if ((Position.X % 16) > 9 && (tr & Touching.DOWN) == 0)
                    {
                        additionalVel.X += 30;
                    }
                }
            }
            else if (KeyInput.IsRebindableKeyPressed(Reverse(KeyFunctions.Down)))
            {
                velocity.Y = mul;
                if ((touching & Touching.DOWN) != 0)
                {
                    Touching bl = GlobalState.Map.GetCollisionData(Position + new Vector2(0, height + 8));
                    Touching br = GlobalState.Map.GetCollisionData(Position + new Vector2(width, height + 8));
                    if (!bl.HasFlag(Touching.UP) && !br.HasFlag(Touching.UP))
                    {
                        //nothing
                    }
                    else if ((Position.X + width) % 16 < 6 && (bl & Touching.UP) == 0)
                    {
                        additionalVel.X -= 30;
                    }
                    else if ((Position.X % 16) > 9 && (br & Touching.UP) == 0)
                    {
                        additionalVel.X += 30;
                    }
                }
            }
            else
            {
                velocity.Y = 0;
            }

            if (KeyInput.IsRebindableKeyPressed(Reverse(KeyFunctions.Left)))
            {
                velocity.X = -mul;
                if ((touching & Touching.LEFT) != 0)
                {
                    Touching tl = GlobalState.Map.GetCollisionData(Position + new Vector2(-8, 0));
                    Touching bl = GlobalState.Map.GetCollisionData(Position + new Vector2(-8, height));
                    if (!tl.HasFlag(Touching.RIGHT) && !bl.HasFlag(Touching.RIGHT))
                    {
                        //nothing
                    }
                    else if ((Position.Y + height) % 16 < 6 && (tl & Touching.RIGHT) == 0)
                    {
                        additionalVel.Y -= 30;
                    }
                    else if (Position.Y % 16 > 9 && (bl & Touching.RIGHT) == 0)
                    {
                        additionalVel.Y += 30;
                    }
                }
            }
            else if (KeyInput.IsRebindableKeyPressed(Reverse(KeyFunctions.Right)))
            {
                velocity.X = mul;
                if ((touching & Touching.RIGHT) != 0)
                {
                    Touching tr = GlobalState.Map.GetCollisionData(Position + new Vector2(width + 8, 0));
                    Touching br = GlobalState.Map.GetCollisionData(Position + new Vector2(width + 8, height));
                    if (!tr.HasFlag(Touching.LEFT) && !br.HasFlag(Touching.LEFT))
                    {
                        //nothing
                    }
                    else if ((Position.Y + height) % 16 < 6 && (tr & Touching.LEFT) == 0)
                    {
                        additionalVel.Y -= 30;
                    }
                    else if (Position.Y % 16 > 9 && (br & Touching.LEFT) == 0)
                    {
                        additionalVel.Y += 30;
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

        private KeyFunctions Reverse(KeyFunctions key)
        {
            if (!reversed)
            {
                return key;
            }

            return key switch
            {
                KeyFunctions.Up => KeyFunctions.Down,
                KeyFunctions.Down => KeyFunctions.Up,
                KeyFunctions.Left => KeyFunctions.Right,
                KeyFunctions.Right => KeyFunctions.Left,
                _ => key,
            };
        }

        internal void ReceiveDamage(int amount, string damageDealer, bool knockback = true, bool playSound = true)
        {
            if (!invincible && !hasFallen && !GlobalState.FUCK_IT_MODE_ON)
            {
                if(!GlobalState.settings.invincible) //Setting only disables health change
                    GlobalState.CUR_HEALTH -= amount;

                if (playSound)
                {
                    SoundManager.PlaySoundEffect("player_hit_1");
                }

                GlobalState.DamageDealer = damageDealer;

                if (GlobalState.CUR_HEALTH > 0)
                {
                    invincible = true;
                    invincibility_time = INVINCIBLE_MAX;
                    Flicker(INVINCIBLE_MAX);

                    if (knockback)
                    {
                        bump_timer = BUMP_TIMER_MAX;
                    }
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
