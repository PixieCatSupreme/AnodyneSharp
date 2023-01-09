using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.RunningTradeNPCs
{
    [NamedEntity("Trade_NPC",null,3), Collision(typeof(Player))]
    public class ShopKeep : Entity, Interactable
    {
        //preset.Activated gets turned on when the shopkeep has been given his box
        EntityPreset _preset;

        List<ShopItem> _items;

        public ShopKeep(EntityPreset preset, Player p) : base(preset.Position, new AnimatedSpriteRenderer("fields_npcs", 16, 16, new Anim("a",new int[] { 50, 51 },4)), Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;
            _preset = preset;

            Vector2 itemBasePos = Position + new Vector2(-30,32);

            _items = new()
            {
                new(itemBasePos, 54),
                new(itemBasePos + Vector2.UnitX * 34, 55),
                new(itemBasePos + Vector2.UnitX * 34 * 2, (_preset.Activated || GlobalState.inventory.CanJump) ? 57 : 56)
            };
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return _items;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            static string getDiag(int i) => Dialogue.DialogueManager.GetDialogue("misc", "any", "tradenpc", i);

            if (GlobalState.events.GetEvent("shopkeep.init") == 0)
            {
                GlobalState.Dialogue = getDiag(0);
                GlobalState.events.IncEvent("shopkeep.init");
            }
            else if(_preset.Activated)
            {
                GlobalState.Dialogue = getDiag(1);
            }
            else if(GlobalState.inventory.tradeState == InventoryManager.TradeState.BOX)
            {
                _preset.Activated = true;
                if(GlobalState.inventory.CanJump)
                {
                    //Young can already jump, so shopkeep exchanges something else for the box
                    GlobalState.inventory.tradeState = InventoryManager.TradeState.NONE;
                    if(GlobalState.inventory.CardStatus[43])
                    {
                        GlobalState.Dialogue = getDiag(2) + " " + getDiag(11) + " " + getDiag(3);
                        GlobalState.CUR_HEALTH = GlobalState.MAX_HEALTH;
                    }
                    else
                    {
                        GlobalState.Dialogue = getDiag(2) + " " + getDiag(11);
                        Gadget.Treasures.CardTreasure card = new(Position + Vector2.UnitX * 20, 43);
                        card.GetTreasure();
                        GlobalState.SpawnEntity(card);
                    }
                }
                else
                {
                    GlobalState.Dialogue = getDiag(2) + " " + getDiag(4);
                    GlobalState.inventory.tradeState = InventoryManager.TradeState.SHOES;
                    _items[2].ActivateAnim();
                }
            }
            else
            {
                GlobalState.Dialogue = getDiag(5);
            }
            return true;
        }
    }

    [Collision(typeof(Player))]
    class ShopItem : Entity, Interactable
    {
        bool _checked = false;
        Vector2 _startPos;

        public ShopItem(Vector2 pos, int frame) : base(pos,new AnimatedSpriteRenderer("fields_npcs",16,16, new Anim("a",new int[] { frame },1)),Drawing.DrawOrder.BG_ENTITIES)
        {
            _startPos = pos;
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public override void Update()
        {
            base.Update();
            if(velocity.Y < -10 && velocity.Y > -15)
            {
                Flicker(1);
            }
            else if(velocity.Y < -25)
            {
                acceleration = Vector2.Zero;
                velocity = Vector2.Zero;
                Position = _startPos;
                SetFrame(Frame + 1);
                opacity = 0;
            }
            MathUtilities.MoveTo(ref opacity, 1, 0.3f);
        }

        public void ActivateAnim()
        {
            acceleration.Y = -10;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            static string getDiag(int i) => Dialogue.DialogueManager.GetDialogue("misc", "any", "tradenpc", i);

            if (_checked)
            {
                GlobalState.Dialogue = getDiag(6);
            }
            else
            {
                GlobalState.Dialogue = getDiag(Frame - 47);
                _checked = true;
            }
            return true;
        }
    }
}
