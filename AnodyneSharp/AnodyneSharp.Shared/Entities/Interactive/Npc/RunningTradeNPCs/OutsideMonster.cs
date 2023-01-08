using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Interactive.Npc.RunningTradeNPCs
{
    [NamedEntity("Trade_NPC", null, 2), Collision(typeof(Player))]
    public class OutsideMonster : Entity, Interactable
    {
        Box b;
        Player player;

        public static AnimatedSpriteRenderer GetSprite() => new("fields_npcs", 16, 16,
            new Anim("walk_d", new int[] { 20, 21 },4),
            new Anim("walk_r", new int[] { 22, 23 }, 4),
            new Anim("walk_u", new int[] { 24, 25 }, 4)
            );

        public OutsideMonster(EntityPreset preset, Player p) : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            player = p;
            b = new(preset.Position - Vector2.UnitY * 20, preset, p);
            immovable = true;
            if (GlobalState.events.SpookedMonster)
            {
                exists = false;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { b };
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
                GlobalState.StartCutscene = CutSceneState();
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("goldman", "outside");
            }
            return true;
        }

        public IEnumerator<CutsceneEvent> CutSceneState()
        {
            yield return new DialogueEvent(DialogueManager.GetDialogue("goldman", "etc", 0));

            Solid = false;

            velocity = -Vector2.UnitY * 40;
            Play("walk_u");

            while (MapUtilities.GetInGridPosition(Position).Y > 6 * 16)
                yield return null;

            velocity = Vector2.UnitX * 40;
            Play("walk_r");

            while (MapUtilities.GetInGridPosition(Position).X > 10) //wraps around when on next screen
                yield return null;

            exists = false;

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

        public static AnimatedSpriteRenderer GetSprite() => new("fields_npcs", 16, 16,
            new Anim("closed", new int[] { 31 },1),
            new Anim("open", new int[] { 32 },1)
            );

        public Box(Vector2 position, EntityPreset preset, Player p) : base(position,GetSprite(),Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;
            icky = new(position - Vector2.UnitY * 10, p);
            this.preset = preset;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(icky,1);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (GlobalState.events.SpookedMonster)
            {
                Play("open");
                Sounds.SoundManager.PlaySoundEffect("broom_hit");
                GlobalState.StartCutscene = OnOpened();
                return true;
            }
            else
            {
                return false;
            }
        }

        IEnumerator<CutsceneEvent> OnOpened()
        {
            yield return new DialogueEvent(DialogueManager.GetDialogue("goldman", "etc", 2));

            GlobalState.inventory.tradeState = InventoryManager.TradeState.BOX;
            exists = false;
            icky.exists = true;
            preset.Alive = false;

            GlobalState.StartCutscene = icky.CutSceneState();

            yield break;
        }
    }

    [Collision(typeof(Player))]
    public class BoxIcky : Entity, Interactable
    {
        IEnumerator _state;

        public BoxIcky(Vector2 pos, Player p) : base(pos, Icky.GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            Play("walk_r");
            exists = false;
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
            _state?.MoveNext();
        }

        public IEnumerator<CutsceneEvent> CutSceneState()
        {
            GlobalState.events.IncEvent("icky.rescued");

            velocity = Vector2.UnitX * 20;
            Play("walk_r");

            while (MapUtilities.GetInGridPosition(Position).X < 85)
                yield return null;

            velocity = Vector2.Zero;
            Play("walk_d");

            yield return new DialogueEvent(DialogueManager.GetDialogue("goldman", "etc", 3));

            _state = MoveLastBit();

            yield break;
        }

        IEnumerator MoveLastBit()
        {
            velocity = Vector2.UnitX * 20;
            Play("walk_r");

            while (MapUtilities.GetInGridPosition(Position).X < 6 * 16)
                yield return null;

            velocity = Vector2.Zero;
            Play("walk_d");

            yield break;
        }
    }
}
