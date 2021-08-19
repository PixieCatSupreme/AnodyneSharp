using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [NamedEntity("NPC", type: "generic", map:"FIELDS", 8), Collision(typeof(Player), MapCollision = true)]
    public class Olive : Entity, Interactable
    {
        IEnumerator _state;

        public Olive(EntityPreset preset, Player p) : base(preset.Position, "forest_npcs", 16,16,Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("idle", CreateAnimFrameArray(30), 4, true);
            AddAnimation("hop", CreateAnimFrameArray(32, 33, 32), 4, false);
            _state = UpdateFunc();
            immovable = true;
        }

        public override void Update()
        {
            base.Update();
            _state.MoveNext();
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        IEnumerator UpdateFunc()
        {
            while (true) {
                float t = 0;
                velocity = Vector2.Zero;
                Play("idle");
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
                while(t < 1.4f)
                {
                    t += GameTimes.DeltaTime;
                    yield return "idle_left";
                }
                Play("hop");
                velocity.X = 25;
                while(!_curAnim.Finished)
                {
                    yield return "hopping_right";
                }
                velocity = Vector2.Zero;
                Play("idle");
                t = 0;
                while(t < 1.6f)
                {
                    t += GameTimes.DeltaTime;
                    yield return "idle_right";
                }
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
                velocity.X = -25;
                Play("hop");
                while (!_curAnim.Finished)
                {
                    yield return "hopping_left";
                }
            }
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("generic_npc", "easter_egg");
            return true;
        }
    }
}
