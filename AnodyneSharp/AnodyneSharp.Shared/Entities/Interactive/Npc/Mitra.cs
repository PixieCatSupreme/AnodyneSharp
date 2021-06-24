using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [Collision(typeof(Player))]
    public class Bike : Entity
    {
        public Bike() : base(Vector2.Zero, "bike", 20, 20, DrawOrder.ENTITIES)
        {
            immovable = true;
            AddAnimation("r", CreateAnimFrameArray(1), 2);
            Play("r");
            width = 16;
            height = 10;
            offset = new(2, 11);
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }
    }

    [Collision(typeof(Player), MapCollision = true)]
    public abstract class Mitra : Entity, Interactable
    {
        protected Bike bike = new();
        protected Player _player;
        protected EntityPreset _preset;
        IEnumerator _state;

        public Mitra(EntityPreset preset, Player p, bool start_on_bike) : base(preset.Position, DrawOrder.ENTITIES)
        {
            _player = p;
            _preset = preset;

            bike.exists = false;
            immovable = true;

            if (start_on_bike)
            {
                OnBike();
            }
            else
            {
                OffBike();
            }

            AddAnimation("walk_d", CreateAnimFrameArray(0, 1), 8);
            AddAnimation("walk_l", CreateAnimFrameArray(2, 3), 8);
            AddAnimation("walk_r", CreateAnimFrameArray(2, 3), 8);
            AddAnimation("walk_u", CreateAnimFrameArray(4, 5), 8);

            AddAnimation("idle_d", CreateAnimFrameArray(6), 8);
            AddAnimation("idle_l", CreateAnimFrameArray(7), 8);
            AddAnimation("idle_r", CreateAnimFrameArray(7), 8);
            AddAnimation("idle_u", CreateAnimFrameArray(8), 8);

            _state = StateLogic();
        }

        protected abstract IEnumerator StateLogic();
        protected abstract string GetInteractionText();

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = GetInteractionText();
            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        protected void OnBike()
        {
            SetTexture("mitra_bike", 20, 20);
            width = height = 10;
            offset = Vector2.One * 5;
        }

        protected void OffBike()
        {
            SetTexture("mitra", 16, 16);
            width = height = 16;
            offset = Vector2.Zero;
        }

        public override void Update()
        {
            base.Update();
            _state.MoveNext();
            if (velocity == Vector2.Zero)
            {
                FaceTowards(_player.Position);
                PlayFacing("idle");
            }
            else
            {
                FaceTowards(Position + velocity);
                PlayFacing("walk");
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return base.SubEntities().Concat(Enumerable.Repeat(bike, 1));
        }

        protected override void AnimationChanged(string name)
        {
            if (name.EndsWith('l'))
            {
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            }
            else
            {
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
            }
        }
    }

    [NamedEntity("Mitra", map: "OVERWORLD")]
    public class MitraOverworld : Mitra
    {
        VolumeEvent volume = new(0.3f);

        public MitraOverworld(EntityPreset preset, Player p) : base(preset, p, true)
        {
            visible = false;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return base.SubEntities().Concat(Enumerable.Repeat(volume, 1));
        }

        protected override string GetInteractionText()
        {
            GlobalState.events.IncEvent("mitra.wares");
            return DialogueManager.GetDialogue("mitra", "initial_overworld");
        }

        protected override IEnumerator StateLogic()
        {
            Vector2 UL = MapUtilities.GetRoomUpperLeftPos(MapUtilities.GetRoomCoordinate(Position));
            Position.X -= 20;

            while (Math.Abs(Position.Y - _player.Position.Y) > 48 || !volume.ReachedTarget)
                yield return null;

            GlobalState.disable_menu = true;
            SoundManager.PlaySong("mitra");
            volume.exists = false; //disable volume changing

            GlobalState.Dialogue = DialogueManager.GetDialogue("mitra", "initial_overworld");

            _preset.Alive = false;

            while (!GlobalState.LastDialogueFinished)
                yield return null;

            _player.state = PlayerState.INTERACT;

            MoveTowards(_player.Position, 80);
            visible = true;

            //Get at least a quarter into the screen, and relatively close to the player
            while ((Position-UL).X < 40 &&
                _player.Position.X - Position.X > 24
                && Math.Abs(_player.Position.X - Position.X) > 16)
            {
                yield return null;
            }

            velocity = Vector2.UnitY * 80;

            //Go down past the player
            while (Position.Y - _player.Position.Y < 24)
                yield return null;

            velocity = Vector2.UnitX * 80;

            immovable = false;

            //Wait for map collision
            while (touching == Touching.NONE)
                yield return null;

            velocity = Vector2.Zero;
            immovable = true;

            GlobalState.screenShake.Shake(0.02f, 0.3f);
            SoundManager.PlaySoundEffect("sun_guy_death_short");
            OffBike();
            bike.exists = true;
            bike.Position = Position - Vector2.UnitX * 4;

            GlobalState.flash.Flash(1, Color.White);

            while (GlobalState.flash.Active())
                yield return null;

            velocity.X = -20;

            while (Position.X > _player.Position.X + 3)
            {
                _player.FaceTowards(Position);
                yield return null;
            }

            MoveTowards(_player.Position, 20);

            while (Position.Y > _player.Position.Y + 26)
            {
                _player.FaceTowards(Position);
                yield return null;
            }

            velocity = Vector2.Zero;

            GlobalState.Dialogue = DialogueManager.GetDialogue("mitra", "initial_overworld");

            while (!GlobalState.LastDialogueFinished)
                yield return null;

            //Player can walk around again
            _player.state = PlayerState.GROUND;

            //Start end 
            while(!DialogueManager.IsSceneFinished("mitra","initial_overworld"))
            {
                if ((Position - _player.Position).Length() > 48)
                {
                    GlobalState.Dialogue = DialogueManager.GetDialogue("mitra", "initial_overworld", 3);
                    while (!GlobalState.LastDialogueFinished)
                        yield return null;
                }
                yield return null;
            }

            while (!GlobalState.LastDialogueFinished)
                yield return null;

            _player.state = PlayerState.INTERACT;

            //back to bike
            velocity = Vector2.UnitX * 20;

            while ((Position - bike.Position).Length() > 8)
                yield return null;

            OnBike();
            bike.exists = false;
            
            //go off-screen
            velocity = Vector2.UnitY * 50;

            while ((Position-UL).Y < 190)
                yield return null;

            volume.exists = true;
            volume.SetTarget(0.2f);

            while (!volume.ReachedTarget)
                yield return null;

            SoundManager.PlaySong("overworld");
            volume.exists = false;
            GlobalState.disable_menu = false;

            _player.state = PlayerState.GROUND;
            _preset.Alive = exists = false;
            yield break;
        }
    }

    [NamedEntity("Mitra",map:"FIELDS")]
    public class MitraFields : Mitra
    {
        bool initial;

        public MitraFields(EntityPreset preset, Player p) : base(preset, p, false)
        {
            initial = !DialogueManager.IsSceneDirty("mitra", "init");
            bike.exists = true;
            bike.Position = Position - new Vector2(20, 0);
        }

        protected override string GetInteractionText()
        {
            if(initial)
            {
                GlobalState.events.IncEvent("mitra.fieldinit");
                if(DialogueManager.IsSceneDirty("mitra","init"))
                {
                    return DialogueManager.GetDialogue("mitra", "init");
                }
                int talked_about_wares = GlobalState.events.GetEvent("mitra.wares");
                string dialog = DialogueManager.GetDialogue("mitra", "init",1-talked_about_wares);
                DialogueManager.SetSceneProgress("mitra", "init", 2);
                return dialog;
            }
            
            if(GlobalState.inventory.tradeState == InventoryManager.TradeState.SHOES)
            {
                GlobalState.inventory.tradeState = InventoryManager.TradeState.NONE;
                GlobalState.inventory.CanJump = true;
                return DialogueManager.GetDialogue("misc", "any", "mitra", 1);
            }
            //TODO: hints
            return DialogueManager.GetDialogue("mitra", "general_banter");
        }

        protected override IEnumerator StateLogic()
        {
            yield break;
        }
    }
}
