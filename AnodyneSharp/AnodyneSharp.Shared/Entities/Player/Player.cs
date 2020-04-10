using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.States;
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
        private Vector2 additionalVel;

        private PlayState parent;

        public Player(PlayState parent)
            : base(Vector2.Zero, ORIGINAL_WIDTH, ORIGINAL_HEIGHT)
        {
            //DEATH_FRAME = 32;
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
                    //my_shadow.visible = false;
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
                    Ground_animation();

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

        private void Ground_animation()
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

        protected override void AnimationChanged(string name)
        {
            switch(name)
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
            velocity *= GameTimes.DeltaTime;
        }

        private void Set_init_vel(float mul = 1)
        {
            if (KeyInput.IsKeyPressed(Keys.Up))
            {
                velocity.Y = -mul;
                if ((touching & Touching.UP) != 0)
                {
                    Touching tl = parent.GetTileCollisionFlags(Position + new Vector2(0,-8));
                    Touching tr = parent.GetTileCollisionFlags(Position + new Vector2(width,-8));
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
                    Touching bl = parent.GetTileCollisionFlags(Position + new Vector2(0, height+8));
                    Touching br = parent.GetTileCollisionFlags(Position + new Vector2(width,height+8));
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
                    Touching tl2 = parent.GetTileCollisionFlags(Position + new Vector2(-8,0));
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
                    Touching tr2 = parent.GetTileCollisionFlags(Position + new Vector2(width+8, 0));
                    Touching br2 = parent.GetTileCollisionFlags(Position + new Vector2(width+8, height));
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
    }
}
