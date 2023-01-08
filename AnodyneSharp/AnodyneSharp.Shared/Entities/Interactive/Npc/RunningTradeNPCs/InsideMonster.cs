using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.RunningTradeNPCs
{
    [NamedEntity("Trade_NPC", null, 1), Collision(typeof(Player), MapCollision = true), Events(typeof(DustFallEvent), typeof(Gadget.Treasures.EmptyTreasureEvent))]
    public class InsideMonster : Entity, Interactable
    {
        EntityPreset _preset;
        List<Dust> _extra_dust = new();
        int _dust_left = 12;

        public InsideMonster(EntityPreset preset, Player p) : base(preset.Position, OutsideMonster.GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;
            Play("walk_d");
            immovable = true;

            if (!GlobalState.events.SpookedMonster)
            {
                exists = false;
            }
            else if(_preset.Activated)
            {
                Position += Vector2.UnitX * 32;
            }
            else
            {
                Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
                _extra_dust = new()
                {
                    new Dust(tl + Vector2.One * 16,p),
                    new Dust(tl + Vector2.One * 16 + Vector2.UnitY * 32, p),
                    new Dust(tl + Vector2.One * 16 + Vector2.UnitY * 64, p)
                };
            }
        }

        public override void Update()
        {
            base.Update();
            if(_preset.Activated && velocity.X != 0 && Position.X >= _preset.Position.X + 31.5)
            {
                velocity.X = 0;
                Play("walk_d");
            }
        }

        public override void OnEvent(GameEvent e)
        {
            if (e is DustFallEvent)
            {
                _dust_left--;
                if (_dust_left == 0)
                {
                    GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("goldman", "etc", 1);
                    _preset.Activated = true;
                    velocity.X = 20;
                    Play("walk_r");
                }
            }
            else
            {
                //shopkeep must have stolen it!
                GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("misc", "any", "treasure", 8);
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return _extra_dust;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if(_preset.Activated)
            {
                GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("goldman", "etc", 1);
            }
            else
            {
                GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("goldman", "inside");
            }
            return true;
        }
    }
}
