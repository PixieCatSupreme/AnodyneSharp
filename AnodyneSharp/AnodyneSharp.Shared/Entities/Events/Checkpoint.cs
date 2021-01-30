using AnodyneSharp.Drawing;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Events
{
    [NamedEntity("Event", null, 2)]
    public class Checkpoint : Entity, Interactable
    {
        class SpawnedOn : AbstractState
        {
            public bool player_on = false;
        }

        class WaitForResave : TimerState
        {
            public WaitForResave()
            {
                AddTimer(0.1f, "save_again");
            }
        }

        bool Active => GlobalState.checkpoint == new GlobalState.CheckPoint(GlobalState.CURRENT_MAP_NAME, Position);

        IState state;

        SavingIcon saveIcon = new();

        public Checkpoint(EntityPreset preset, Player p) : base(preset.Position, "checkpoint", 16, 16, DrawOrder.BG_ENTITIES)
        {
            width = height = 8;
            offset = Vector2.One * 4;
            Position += offset;

            int i = GlobalState.IsCell ? 4 : 0;
            AddAnimation("inactive", CreateAnimFrameArray(i));
            AddAnimation("active", CreateAnimFrameArray(i + 1, i + 2, i + 3), 10, false);
            AddAnimation("stepped_on", CreateAnimFrameArray(i + 1, i + 2), 12);

            bool playerOn() => p.Hitbox.Intersects(Hitbox);

            Play(Active ? "active" : "inactive");

            state = new StateMachineBuilder()
                .State<SpawnedOn>()
                    .Condition(() => !playerOn(), (s) => state.ChangeState("Wait"))
                .End()
                .State("Wait")
                    .Enter((s) => Play(Active ? "active" : "inactive"))
                    .Condition(playerOn, (s) => state.ChangeState("PlayerOn"))
                .End()
                .State("PlayerOn")
                    .Enter((s) => Play("stepped_on"))
                    .Event("Interact", (s) =>
                    {
                        SoundManager.PlaySoundEffect("button_down");
                        if (!Active)
                        {
                            GlobalState.CUR_HEALTH = GlobalState.MAX_HEALTH;
                            GlobalState.checkpoint = new GlobalState.CheckPoint(GlobalState.CURRENT_MAP_NAME, Position);
                        }
                        GlobalState.SaveGame();
                        saveIcon.opacity = 1f;
                        saveIcon.visible = true;
                        state.ChangeState("Saved");
                    })
                    .Condition(() => !playerOn(), (s) => state.ChangeState("Wait"))
                .End()
                .State("Saved")
                    .Enter((s) => Play("active"))
                    .Condition(() => !playerOn(), (s) => state.ChangeState("WaitForResave"))
                .End()
                .State<WaitForResave>()
                    .Event("save_again", (s) => state.ChangeState("Wait"))
                .End()
                .Build();

            state.ChangeState(playerOn() ? "SpawnedOn" : "Wait");
        }

        public override void Update()
        {
            state.Update(GameTimes.DeltaTime);
            base.Update();
            GlobalState.UIEntities.Add(saveIcon);
            saveIcon.Update();
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            saveIcon.PostUpdate();
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            state.TriggerEvent("Interact");
            return false; //don't disable broom
        }
    }

    class SavingIcon : UIEntity
    {
        public SavingIcon() : base(new Vector2(GameConstants.SCREEN_WIDTH_IN_PIXELS / 2 - 32, 20), "autosave_icon", 64, 16, DrawOrder.TEXT)
        {
            visible = false;
            AddAnimation("a", CreateAnimFrameArray(0, 1, 2, 3, 4, 5), 8);
            Play("a");
        }

        public override void Update()
        {
            if (visible)
            {
                if (MathUtilities.MoveTo(ref opacity, 0, 0.33f))
                {
                    visible = false;
                }
            }
        }
    }
}
