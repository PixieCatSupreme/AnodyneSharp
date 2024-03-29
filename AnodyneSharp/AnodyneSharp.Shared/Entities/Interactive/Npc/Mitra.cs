﻿using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
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
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [Collision(typeof(Player))]
    public class Bike : Entity
    {
        public Bike() : base(Vector2.Zero, new AnimatedSpriteRenderer("bike", 20, 20, new Anim("r", new int[] { 1 }, 1)), DrawOrder.ENTITIES)
        {
            immovable = true;
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
        protected bool _facePlayer = true;

        public static AnimatedSpriteRenderer GetSprite() => new("mitra", 16, 16,
            new Anim("walk_d", new int[] { 0, 1 }, 8),
            new Anim("walk_l", new int[] { 2, 3 }, 8),
            new Anim("walk_r", new int[] { 2, 3 }, 8),
            new Anim("walk_u", new int[] { 4, 5 }, 8),
            new Anim("idle_d", new int[] { 6 }, 8),
            new Anim("idle_l", new int[] { 7 }, 8),
            new Anim("idle_r", new int[] { 7 }, 8),
            new Anim("idle_u", new int[] { 8 }, 8)
            );

        public Mitra(EntityPreset preset, Player p, bool start_on_bike) : base(preset.Position, GetSprite(), DrawOrder.ENTITIES)
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
        }

        protected abstract string GetInteractionText();

        public bool PlayerInteraction(Facing player_direction)
        {
            string text = GetInteractionText();
            if (text != "")
            {
                GlobalState.Dialogue = text;
                return true;
            }
            return false;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        protected void OnBike()
        {
            sprite.SetTexture("mitra_bike",20,20,false,false);
            width = height = 10;
            offset = Vector2.One * 5;
        }

        protected void OffBike()
        {
            sprite.SetTexture("mitra", 16, 16, false, false);
            width = height = 16;
            offset = Vector2.Zero;
        }

        public override void Update()
        {
            base.Update();
            if (velocity == Vector2.Zero)
            {
                if (_facePlayer)
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

    [NamedEntity("Mitra", map: "CLIFF")]
    public class MitraCliff : Mitra
    {
        bool player_jumped = false;
        bool exiting = false;

        public MitraCliff(EntityPreset preset, Player p) : base(preset, p, true)
        {
            visible = false;
            if (GlobalState.inventory.CanJump)
            {
                _preset.Alive = exists = false;
            }
        }

        public override void Update()
        {
            base.Update();
            if (_player.state == PlayerState.AIR)
            {
                player_jumped = true;
            }

            Vector2 player_grid_pos = MapUtilities.GetInGridPosition(_player.Position);

            if (!visible && player_grid_pos.Y > 50)
            {
                Position = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(180, 80);
                visible = true;
                GlobalState.StartCutscene = Entrance();
            }
            else if (!exiting && visible && (player_grid_pos.X > 135 || player_grid_pos.Y < 49))
            {
                exiting = true;
                GlobalState.StartCutscene = Exit();
            }
        }

        IEnumerator<CutsceneEvent> Entrance()
        {
            GlobalState.SpawnEntity(new FadeSwitchSong("mitra"));
            yield return new DialogueEvent(DialogueManager.GetDialogue("misc", "any", "mitra", 0));

            velocity.X = -37;

            while (Position.X > _player.Position.X)
            {
                yield return null;
            }

            velocity = Vector2.Zero;
            OffBike();
            bike.Position = Position - Vector2.One;
            bike.exists = true;

            if (GlobalState.inventory.tradeState == InventoryManager.TradeState.SHOES)
            {
                yield return new DialogueEvent(DialogueManager.GetDialogue("misc", "any", "mitra", 1));
                GlobalState.inventory.tradeState = InventoryManager.TradeState.NONE;
            }
            else
            {
                yield return new DialogueEvent(DialogueManager.GetDialogue("misc", "any", "mitra", 2));
            }

            GlobalState.inventory.CanJump = true;

            yield break;
        }

        IEnumerator<CutsceneEvent> Exit()
        {
            yield return new DialogueEvent(DialogueManager.GetDialogue("misc", "any", "mitra", 3));

            bike.exists = false;
            OnBike();

            velocity.X = 37;
            Solid = false; //disable collisions to not push player out of the screen

            while (MapUtilities.GetInGridPosition(Position).X > 10) //until wrap around from exiting the screen
            {
                yield return null;
            }

            _preset.Alive = exists = false;
            GlobalState.SpawnEntity(new FadeSwitchSong("cliff"));

            yield break;
        }

        protected override string GetInteractionText()
        {
            if (player_jumped)
            {
                return DialogueManager.GetDialogue("misc", "any", "mitra", 5);
            }
            else
            {
                return DialogueManager.GetDialogue("misc", "any", "mitra", 4);
            }
        }
    }

    [NamedEntity("Mitra", map: "OVERWORLD")]
    public class MitraOverworld : Mitra
    {
        VolumeEvent volume = new(0.3f);

        enum State
        {
            InitialWait,
            Entrance,
            SecondWait,
            Exit
        }

        State s = State.InitialWait;

        public MitraOverworld(EntityPreset preset, Player p) : base(preset, p, true)
        {
            visible = false;
        }

        public override void Update()
        {
            switch (s)
            {
                case State.InitialWait:
                    if (Math.Abs(Position.Y - _player.Position.Y) < 48 && volume.ReachedTarget)
                    {
                        s = State.Entrance;
                        GlobalState.StartCutscene = Entrance();
                    }
                    break;
                case State.SecondWait:
                    if ((Position - _player.Position).Length() > 48)
                    {
                        s = State.Exit;
                        GlobalState.StartCutscene = Exit();
                    }
                    break;
                default:
                    break;
            }

            base.Update();
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return base.SubEntities().Concat(Enumerable.Repeat(volume, 1));
        }

        protected override string GetInteractionText()
        {
            if (GlobalState.events.GetEvent("mitra.wares") == 0)
            {
                GlobalState.events.IncEvent("mitra.wares");
                return DialogueManager.GetDialogue("mitra", "initial_overworld");
            }
            else
            {
                s = State.Exit;
                GlobalState.StartCutscene = Exit();
                return ""; //dialogue gets started by cutscene
            }
        }

        IEnumerator<CutsceneEvent> Entrance()
        {
            Vector2 UL = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
            Position.X -= 20;

            SoundManager.PlaySong("mitra");
            _preset.Alive = false;

            yield return new DialogueEvent(DialogueManager.GetDialogue("mitra", "initial_overworld"));

            MoveTowards(_player.Position, 80);
            visible = true;

            //Get at least a quarter into the screen, and relatively close to the player
            while ((Position - UL).X < 40 || _player.Position.X - Position.X > 24)
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
            while (wasTouching == Touching.NONE)
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

            yield return new DialogueEvent(DialogueManager.GetDialogue("mitra", "initial_overworld"));

            s = State.SecondWait;

            yield break;
        }

        IEnumerator<CutsceneEvent> Exit()
        {
            yield return new DialogueEvent(DialogueManager.GetDialogue("mitra", "initial_overworld", 3));

            //back to bike
            velocity = Vector2.UnitX * 20;

            while ((Position - bike.Position).Length() > 8)
                yield return null;

            OnBike();
            bike.exists = false;

            //go off-screen
            velocity = Vector2.UnitY * 50;

            Vector2 UL = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);

            while ((Position - UL).Y < 190)
                yield return null;

            volume.SetTarget(0.2f);

            while (!volume.ReachedTarget)
                yield return null;

            SoundManager.PlaySong("overworld");
            exists = false;
            yield break;
        }
    }

    [NamedEntity("Mitra", map: "FIELDS")]
    public class MitraFields : Mitra
    {
        bool initial;
        bool given_quest_hint = false;
        bool given_initial_hint = false; //Only give dungeon/windmill hint once

        public MitraFields(EntityPreset preset, Player p) : base(preset, p, false)
        {
            initial = !DialogueManager.IsSceneDirty("mitra", "init");
            bike.exists = true;
            bike.Position = Position - new Vector2(20, 0);
        }

        protected override string GetInteractionText()
        {
            if (initial)
            {
                GlobalState.events.IncEvent("mitra.fieldinit");
                if (DialogueManager.IsSceneDirty("mitra", "init"))
                {
                    return DialogueManager.GetDialogue("mitra", "init");
                }
                int talked_about_wares = GlobalState.events.GetEvent("mitra.wares");
                string dialog = DialogueManager.GetDialogue("mitra", "init", 1 - talked_about_wares);
                DialogueManager.SetSceneProgress("mitra", "init", 2);
                return dialog;
            }

            if (GlobalState.inventory.tradeState == InventoryManager.TradeState.SHOES)
            {
                GlobalState.inventory.tradeState = InventoryManager.TradeState.NONE;
                GlobalState.inventory.CanJump = true;
                return DialogueManager.GetDialogue("misc", "any", "mitra", 1);
            }

            bool all_bosses_dead = new List<string>(6) { "REDCAVE", "BEDROOM", "CROWD", "CIRCUS", "APARTMENT", "HOTEL" }
                .All(s => GlobalState.events.BossDefeated.Contains(s));

            if (GlobalState.inventory.CardCount < 36 && all_bosses_dead)
            {
                if (!given_quest_hint && GlobalState.events.GetEvent("GoQuestProgress") == 0)
                {
                    given_quest_hint = true;
                    return DialogueManager.GetDialogue("mitra", "quest_event");
                }
                if (GlobalState.inventory.CardCount == 0)
                {
                    return DialogueManager.GetDialogue("mitra", "card_hints", 36);
                }
                List<int> missing_cards = GlobalState.inventory.CardStatus.Select((s, i) => !s && i < 36 ? i : -1).Where(i => i >= 0).ToList();
                return DialogueManager.GetDialogue("mitra", "card_hints", missing_cards[GlobalState.RNG.Next(missing_cards.Count)]);
            }

            if (!given_initial_hint)
            {
                given_initial_hint = true;

                int hint = 0;

                if (GlobalState.events.GetEvent("WindmillOpened") == 0)
                {
                    bool redcave = GlobalState.events.BossDefeated.Contains("REDCAVE");
                    bool crowd = GlobalState.events.BossDefeated.Contains("CROWD");
                    if (!redcave && !crowd)
                    {
                        hint = GlobalState.RNG.Next(1, 3);
                    }
                    else if (redcave && crowd)
                    {
                        hint = 3;
                    }
                    else if (redcave && !crowd)
                    {
                        hint = 8;
                    }
                    else
                    {
                        hint = 7;
                    }
                }
                else
                {
                    if (!all_bosses_dead)
                    {
                        hint = 4;
                    }
                    else if (!GlobalState.inventory.HasBroomType(BroomType.Transformer))
                    {
                        hint = 5;
                    }
                    else
                    {
                        hint = 6;
                    }
                }
                return DialogueManager.GetDialogue("mitra", "game_hints", hint);
            }

            if (!given_quest_hint && GlobalState.events.GetEvent("GoQuestProgress") == 0)
            {
                given_quest_hint = true;
                return DialogueManager.GetDialogue("mitra", "quest_event");
            }

            return DialogueManager.GetDialogue("mitra", "general_banter");
        }
    }
}
