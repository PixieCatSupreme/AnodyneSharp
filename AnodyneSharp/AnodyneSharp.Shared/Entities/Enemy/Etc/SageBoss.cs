using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Enemy.Etc
{
	[NamedEntity("Sage_Boss"), Collision(typeof(Player), typeof(Broom)), Enemy]
	internal class SageBoss : Entity
	{
		private Vector2 _topLeft;

		//private EntityPool<SBullet> _sBullets;
		private EntityPool<LongAttack> _lBullets;
		private EntityPool<Dust> _dusts;

		private IEnumerator _stateLogic;

		private Player _player;

		private int _health;
		private bool _canHurt;

		public SageBoss(EntityPreset preset, Player p)
			: base(preset.Position, "sage_boss", 16, 24, Drawing.DrawOrder.ENTITIES)
		{
			_topLeft = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);

			AddAnimation("idle", CreateAnimFrameArray(4));
			AddAnimation("idle_d", CreateAnimFrameArray(0, 1, 2, 3), 5, true);
			AddAnimation("dash_d", CreateAnimFrameArray(0, 1, 2, 3), 5, true);
			AddAnimation("dash_u", CreateAnimFrameArray(0, 1, 2, 3), 5, true);
			AddAnimation("walk_u", CreateAnimFrameArray(0, 1, 2, 3), 5, true);
			Play("idle");

			width = height = 10;
			offset.X = 3;
			offset.Y = 8;

			Position.X = _topLeft.X + 72;
			Position += new Vector2(3, 3);

			_player = p;

			_lBullets = new EntityPool<LongAttack>(6, () => new LongAttack());
			_dusts = new EntityPool<Dust>(2, () => new Dust(Vector2.Zero, _player));

			_stateLogic = Intro();

#if DEBUG
			int dVal = 1;
            if (dVal == 1)
            {
				Position.X = _topLeft.X + 80 - width / 2;
				Position.Y = _topLeft.Y + 25;

				SoundManager.PlaySong("sagefight");
				Play("idle_d");

				_stateLogic = Stage1();
			}
#endif
        }

		public override void Update()
		{
			base.Update();

			if (_health == 0)
			{
				_health = -1;
				//_stateLogic = Dying();
			}
			else
			{
				_stateLogic.MoveNext();
			}
		}

        public override IEnumerable<Entity> SubEntities()
        {
            return _lBullets.Entities;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (!_canHurt)
            {
				return;
            }

            if (other is Player p)
            {
				p.ReceiveDamage(1);
            }
            else if (!_flickering && other is Broom)
            {
				Flicker(1);
				_health--;
				SoundManager.PlaySoundEffect("broom_hit");
            }
        }

        private IEnumerator Intro()
		{
			_canHurt = false;

			facing = Facing.DOWN;
			Play("idle");

			VolumeEvent volumeEvent = new VolumeEvent(0, 0.6f);

			GlobalState.SpawnEntity(volumeEvent);

			while (!volumeEvent.ReachedTarget)
			{
				yield return null;
			}

			while (Vector2.Distance(Position, _player.Position) >= 32 || _player.state != PlayerState.GROUND)
			{
				yield return null;
			}

			GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "before_fight");

			while (!GlobalState.LastDialogueFinished)
			{
				yield return null;
			}

			Play("idle_d");
			SoundManager.PlaySoundEffect("teleguy_up");

			visible = false;
			opacity = 0;

			Position.X = _topLeft.X + 80 - width / 2;
			Position.Y = _topLeft.Y + 24;

			while (!MathUtilities.MoveTo(ref opacity, 0.4f, 0.48f))
			{
				yield return null;
			}

			visible = true;

			SoundManager.PlaySoundEffect("teleguy_down");

			while (!MathUtilities.MoveTo(ref opacity, 1f, 0.48f))
			{
				yield return null;
			}

			SoundManager.PlaySong("sagefight");

			GlobalState.flash.Flash(2.18f, Color.Black);
			GlobalState.screenShake.Shake(0.02f, 1.5f);

			while (GlobalState.flash.Active())
			{
				yield return null;
			}

			GlobalState.flash.Flash(2.15f, Color.Black);
			GlobalState.screenShake.Shake(0.025f, 1.5f);

			while (GlobalState.flash.Active())
			{
				Position.X = _topLeft.X + GlobalState.RNG.Next(40, 100);
				Position.Y = _topLeft.Y + GlobalState.RNG.Next(30, 70);

				yield return null;
			}

			Position = _player.Position - new Vector2(2, 16);

			GlobalState.flash.Flash(2.15f, Color.Black);
			GlobalState.screenShake.Shake(0.03f, 1.5f);


			while (GlobalState.flash.Active())
			{
				yield return null;
			}

			GlobalState.flash.Flash(6f, Color.Black);
			GlobalState.screenShake.Shake(0.035f, 1.7f);

			Position.X = _topLeft.X + 80 - width / 2;
			Position.Y = _topLeft.Y + 25;

			opacity = 0;

			while (!MathUtilities.MoveTo(ref opacity, 1f, 0.3f))
			{
				yield return null;
			}

			_stateLogic = Stage1();

			yield break;
		}

		private IEnumerator Stage1()
		{
			int maxHealth = 3;
			_health = maxHealth;
			_canHurt = true;

			float bClock = 0;
			float[] bDelay = new float[] { 1.6f, 1.4f, 1.2f };


			while (_health > 0)
			{
				bClock += GameTimes.DeltaTime;

				int i = maxHealth - _health;
				float s1Vel = 50;

				if (i < 0)
				{
					i = 0;
				}

				if (bClock > bDelay[i])
				{
					bClock = 0;
					_lBullets.Spawn((l) => l.Spawn(_topLeft + new Vector2(48 + (64 - l.width) /2, 20), new Vector2(0,s1Vel)));
				}

				yield return null;
			}

            foreach (LongAttack lAttack in _lBullets.Entities)
            {
				lAttack.Poof();
            }

			_canHurt = false;


			yield break;
		}

        [Collision(typeof(Player), typeof(Broom), KeepOnScreen = true)]
        class LongAttack : HurtingEntity
        {
            public LongAttack() 
				: base("sage_fight_long_dust", 64, 16)
            {
				width = 56;
				height = 10;
				offset.X = 4;
				offset.Y = 3;
			}

			public void Spawn(Vector2 pos, Vector2 velocity)
            {
				Play("spin");

				Position = pos;
				this.velocity = velocity;

				visible = true;
            }

            public override void Update()
            {
                base.Update();

                if (touching != Touching.NONE)
                {
					Poof();
                }
            }
        }

        class HurtingEntity : Entity
		{
			public HurtingEntity(string texture, int w, int h)
				: base(Vector2.Zero, texture, w, h, Drawing.DrawOrder.ENTITIES)
			{
				AddAnimation("spin", CreateAnimFrameArray(0, 1), 24, true);
				AddAnimation("poof", CreateAnimFrameArray(0, 1, 2), 12, false);

				visible = false;
			}

            public override void Update()
            {
                base.Update();

                if (CurAnimName == "poof" && _curAnim.Finished)
                {
					exists = false;
                }
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);

                if (visible && exists && CurAnimName != "poof")
                {
                    if (other is Player p && p.state == PlayerState.GROUND)
                    {
						p.ReceiveDamage(1);
                    }
                }
            }

			public void Poof()
            {
                if (CurAnimName != "poof")
                {
					Play("poof");
                }

				velocity = Vector2.Zero;
            }
        }
	}
}
