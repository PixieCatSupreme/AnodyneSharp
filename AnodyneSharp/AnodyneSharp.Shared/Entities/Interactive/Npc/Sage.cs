using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace AnodyneSharp.Entities
{
    [Collision(typeof(Player))]
    abstract class Sage : Entity, Interactable
    {
        protected Player _player;
        protected EntityPreset _preset;
        IEnumerator _state;

        public Sage(EntityPreset preset, Player p) : base(preset.Position, "sage", 16, 16, DrawOrder.ENTITIES)
        {
            _player = p;
            _preset = preset;

            width = height = 10;
            offset = Vector2.One * 3;
            Position += offset;
            immovable = true;

            AddAnimation("walk_d", CreateAnimFrameArray(0, 1), 6, true);
            AddAnimation("walk_r", CreateAnimFrameArray(2, 3), 6, true);
            AddAnimation("walk_l", CreateAnimFrameArray(2, 3), 6, true);
            AddAnimation("walk_u", CreateAnimFrameArray(4, 5), 6, true);
            AddAnimation("idle_d", CreateAnimFrameArray(6));
            AddAnimation("idle_r", CreateAnimFrameArray(7));
            AddAnimation("idle_l", CreateAnimFrameArray(7));
            AddAnimation("idle_u", CreateAnimFrameArray(8));

            Play("idle_d");

            _state = StateLogic();
        }

        protected abstract IEnumerator StateLogic();
        protected abstract string GetInteractionText();

        public override void Update()
        {
            base.Update();
            if (!GlobalState.ScreenTransition)
                _state.MoveNext();
            FaceTowards(_player.Position);
            PlayFacing(velocity == Vector2.Zero ? "idle" : "walk");
        }

        protected override void AnimationChanged(string name)
        {
            if (name == "walk_l" || name == "idle_l")
            {
                _flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                _flip = SpriteEffects.None;
            }
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = GetInteractionText();
            return true;
        }
    }

    [NamedEntity(xmlName: "Sage", map: "NEXUS")]
    class SageNexus : Sage
    {
        public SageNexus(EntityPreset preset, Player p) : base(preset, p)
        {
            if (GlobalState.events.BossDefeated.Contains("TERMINAL"))
            {
                preset.Alive = exists = false;
            }
        }

        protected override IEnumerator StateLogic()
        {
            if (DialogueManager.IsSceneDirty("sage", "enter_nexus"))
                yield break;

            while (_player.state == PlayerState.AIR || (_player.Position - Position).Length() > 64)
            {
                yield return null;
            }

            GlobalState.disable_menu = true;
            _player.BeIdle();
            _player.state = PlayerState.INTERACT;
            MoveTowards(_player.Position, 20);


            while ((_player.Position - Position).Length() > 32)
            {
                yield return null;
            }

            GlobalState.disable_menu = false;
            _player.state = PlayerState.GROUND;
            velocity = Vector2.Zero;

            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "enter_nexus");

            yield break;
        }

        protected override string GetInteractionText()
        {
            return DialogueManager.GetDialogue("sage", "enter_nexus");
        }
    }

    [NamedEntity(xmlName: "Sage", map: "OVERWORLD")]
    class SageOverworld : Sage
    {
        public SageOverworld(EntityPreset preset, Player p) : base(preset, p)
        {
            if (GlobalState.events.BossDefeated.Contains("BEDROOM"))
            {
                preset.Alive = exists = false;
            }
        }

        protected override IEnumerator StateLogic()
        {
            if (DialogueManager.IsSceneDirty("sage", "bedroom_entrance"))
                yield break;

            while (_player.state == PlayerState.AIR || (_player.Position - Position).Length() > 56)
            {
                yield return null;
            }
            GlobalState.disable_menu = true;

            _player.BeIdle();
            _player.state = PlayerState.INTERACT;
            MoveTowards(_player.Position, 20);

            while ((_player.Position - Position).Length() > 28)
            {
                yield return null;
            }

            velocity = Vector2.Zero;

            //Ask for weapon
            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "bedroom_entrance");

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            //wait for broom to be used
            _player.state = PlayerState.GROUND;
            _player.dontMove = true;

            while (!_player.broom.exists)
            {
                yield return null;
            }

            while (_player.broom.exists)
            {
                yield return null;
            }

            _player.dontMove = false;
            GlobalState.disable_menu = false;
            _player.state = PlayerState.GROUND;

            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "bedroom_entrance");

            yield break;
        }

        protected override string GetInteractionText()
        {
            return DialogueManager.GetDialogue("sage", "bedroom_entrance");
        }
    }

    class DungeonSage : Sage
    {
        private int _initDistance;
        private int _stopDistance;

        private string _scene;

        public DungeonSage(EntityPreset preset, Player p, int initDistance, int stopDistance, string scene)
            : base(preset, p)
        {
            if (GlobalState.events.LeftAfterBoss.Contains(GlobalState.CURRENT_MAP_NAME))
            {
                preset.Alive = exists = false;
            }

            _initDistance = initDistance;
            _stopDistance = stopDistance;
            _scene = scene;
        }

        protected override IEnumerator StateLogic()
        {
            if (DialogueManager.IsSceneDirty("sage", _scene))
                yield break;

            while (_player.state == PlayerState.AIR || (_player.Position - Position).Length() > _initDistance)
            {
                yield return null;
            }

            GlobalState.disable_menu = true;

            _player.BeIdle();
            _player.state = PlayerState.INTERACT;
            MoveTowards(_player.Position, 20);

            while ((_player.Position - Position).Length() > _stopDistance)
            {
                yield return null;
            }

            GlobalState.disable_menu = false;
            _player.state = PlayerState.GROUND;
            velocity = Vector2.Zero;

            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", _scene);

            yield break;
        }

        protected override string GetInteractionText()
        {
            return DialogueManager.GetDialogue("sage", _scene);
        }
    }

    [NamedEntity(xmlName: "Sage", map: "BEDROOM")]
    class SageBedroom : DungeonSage
    {
        public SageBedroom(EntityPreset preset, Player p)
            : base(preset, p, 48, 24, "after_boss")
        { }
    }

    [NamedEntity(xmlName: "Sage", map: "REDCAVE")]
    class SageRedCave : DungeonSage
    {
        public SageRedCave(EntityPreset preset, Player p)
            : base(preset, p, 28, 16, "one")
        { }
    }

    [NamedEntity(xmlName: "Sage", map: "CROWD")]
    class SageCrowd : DungeonSage
    {
        public SageCrowd(EntityPreset preset, Player p)
            : base(preset, p, 46, 24, "one")
        { }
    }


    [NamedEntity(xmlName: "Sage", map: "BLANK"), Events(typeof(EndScreenTransition))]
    class SageBlank : Entity
    {
        EntityPreset _preset;
        public SageBlank(EntityPreset preset, Player p) : base(preset.Position, DrawOrder.ENTITIES)
        {
            visible = false;
            _preset = preset;
        }

        public override void OnEvent(GameEvent e)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "intro", _preset.Frame + 1);
            exists = _preset.Alive = false;
        }
    }
}
