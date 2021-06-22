using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.RunningTradeNPCs
{
    [NamedEntity("Trade_NPC", null, 2), Collision(typeof(Player))]
    public class OutsideMonster : Entity, Interactable
    {
        Box b;
        MonsterRun runCutscene;
        Player player;

        public OutsideMonster(EntityPreset preset, Player p) : base(preset.Position, "fields_npcs",16,16,Drawing.DrawOrder.ENTITIES)
        {
            player = p;
            b = new(preset.Position - Vector2.UnitY * 20, preset, p);
            runCutscene = new(preset.Position,p);
            if (GlobalState.events.SpookedMonster)
            {
                exists = false;
            }
            else
            {
                AddAnimation("walk_d", CreateAnimFrameArray(20, 21), 4);
                Play("walk_d");
                immovable = true;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { b, runCutscene };
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if(player.follower != null)
            {
                GlobalState.events.SpookedMonster = true;
                exists = false;
                runCutscene.exists = true;
                GlobalState.Dialogue = DialogueManager.GetDialogue("goldman", "etc", 0);
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("goldman", "outside");
            }
            return true;
        }
    }

    public class MonsterRun : Entity
    {
        IEnumerator _state;
        Player _player;

        public MonsterRun(Vector2 pos, Player p) : base(pos, "fields_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("walk_d", CreateAnimFrameArray(20, 21), 4);
            AddAnimation("walk_r", CreateAnimFrameArray(22, 23), 4);
            AddAnimation("walk_u", CreateAnimFrameArray(24, 25), 4);
            Play("walk_d");
            exists = false;
            _state = CutSceneState();
            _player = p;
        }

        public override void Update()
        {
            base.Update();
            _state.MoveNext();
        }

        IEnumerator CutSceneState()
        {
            while (!GlobalState.LastDialogueFinished)
                yield return "wait";

            GlobalState.disable_menu = true;
            _player.BeIdle();
            _player.state = PlayerState.INTERACT;

            velocity = -Vector2.UnitY * 40;
            Play("walk_u");

            while (MapUtilities.GetInGridPosition(Position).Y > 6 * 16)
                yield return "move up";

            velocity = Vector2.UnitX * 40;
            Play("walk_r");

            while (MapUtilities.GetInGridPosition(Position).X > 10) //wraps around when on next screen
                yield return "move right";

            exists = false;
            GlobalState.disable_menu = false;
            _player.state = PlayerState.GROUND;

            yield break;
        }
    }

    [Collision(typeof(Player))]
    public class Box : Entity, Interactable
    {
        const int closed = 31;
        const int open = 32;

        EntityPreset preset;
        BoxIcky icky;

        public Box(Vector2 position, EntityPreset preset, Player p) : base(position,"fields_npcs",16,16,Drawing.DrawOrder.ENTITIES)
        {
            SetFrame(closed);
            immovable = true;
            icky = new(position - Vector2.UnitY * 10, p);
            this.preset = preset;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public override void Update()
        {
            base.Update();
            if(_curAnim.Frame == open && GlobalState.LastDialogueFinished)
            {
                GlobalState.inventory.tradeState = InventoryManager.TradeState.BOX;
                exists = false;
                icky.exists = true;
                preset.Alive = false;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(icky,1);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            SetFrame(open);
            Sounds.SoundManager.PlaySoundEffect("broom_hit");
            GlobalState.Dialogue = DialogueManager.GetDialogue("goldman", "etc", 2);
            return true;
        }
    }

    [Collision(typeof(Player))]
    public class BoxIcky : Entity, Interactable
    {
        IEnumerator _state;
        Player _player;

        public BoxIcky(Vector2 pos, Player p) : base(pos, "fields_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("walk_d", CreateAnimFrameArray(10, 11), 4);
            AddAnimation("walk_r", CreateAnimFrameArray(12, 13), 4);
            AddAnimation("walk_u", CreateAnimFrameArray(14, 15), 4);
            AddAnimation("walk_l", CreateAnimFrameArray(16, 17), 4);
            Play("walk_r");
            exists = false;
            _state = CutSceneState();
            _player = p;
            immovable = true;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("goldman", "etc", 4);
            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public override void Update()
        {
            base.Update();
            _state.MoveNext();
        }

        IEnumerator CutSceneState()
        {
            while (!GlobalState.LastDialogueFinished)
                yield return "wait";

            GlobalState.disable_menu = true;
            _player.BeIdle();
            _player.state = PlayerState.INTERACT;

            velocity = Vector2.UnitX * 20;
            Play("walk_r");

            while (MapUtilities.GetInGridPosition(Position).X < 85)
                yield return "move right";

            velocity = Vector2.Zero;
            Play("walk_d");

            GlobalState.disable_menu = false;
            _player.state = PlayerState.GROUND;

            GlobalState.Dialogue = DialogueManager.GetDialogue("goldman", "etc", 3);

            while(!GlobalState.LastDialogueFinished)
                yield return "say thanks";

            velocity = Vector2.UnitX * 20;
            Play("walk_r");

            while (MapUtilities.GetInGridPosition(Position).X < 6 * 16)
                yield return "move to final position";

            velocity = Vector2.Zero;
            Play("walk_d");

            yield break;
        }
    }
}
