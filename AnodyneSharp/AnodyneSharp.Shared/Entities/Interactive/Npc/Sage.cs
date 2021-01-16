using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace AnodyneSharp.Entities
{
    [NamedEntity(xmlName: "Sage", map: "NEXUS"), Collision(typeof(Player))]
    class SageNexus : Entity, Interactable
    {
        Player _player;
        EntityPreset _preset;
        IEnumerator _state;

        public SageNexus(EntityPreset preset, Player p) : base(preset.Position,"sage",16,16,DrawOrder.ENTITIES)
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

            if(EventRegister.BossDefeated.Contains("TERMINAL"))
            {
                preset.Alive = exists = false;
            }
            _state = StateLogic();
        }

        public override void Update()
        {
            base.Update();
            _state.MoveNext();
            FaceTowards(_player.Position);
            PlayFacing(velocity == Vector2.Zero ? "idle" : "walk");
        }

        protected override void AnimationChanged(string name)
        {
            if(name == "walk_l" || name == "idle_l")
            {
                _flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                _flip = SpriteEffects.None;
            }
        }

        protected IEnumerator StateLogic()
        {
            if (DialogueManager.IsSceneDirty("sage", "enter_nexus"))
                yield break;

            while(GlobalState.ScreenTransition || _player.state == PlayerState.AIR || (_player.Position - Position).Length() > 64)
            {
                yield return null;
            }
            
            _player.state = PlayerState.INTERACT;
            MoveTowards(_player.Position, 20);
            
            while((_player.Position-Position).Length() > 32)
            {
                yield return null;
            }

            _player.state = PlayerState.GROUND;
            velocity = Vector2.Zero;

            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "enter_nexus");

            yield break;
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("sage","enter_nexus");
            return true;
        }
    }

    [NamedEntity(xmlName:"Sage",map: "BLANK")]
    class SageBlank : Entity
    {
        EntityPreset _preset;
        public SageBlank(EntityPreset preset, Player p) : base(preset.Position, DrawOrder.ENTITIES)
        {
            visible = false;
            _preset = preset;
        }

        public override void Update()
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "intro", _preset.Frame+1);
            exists = _preset.Alive = false;
        }
    }
}
