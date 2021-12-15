using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    [Events(typeof(StartScreenTransition))]
    public class Transformer : Entity
    {
        Entity selector, selected_tile;
        Player player;

        public Transformer(Player p) : base(p.Position, Drawing.DrawOrder.ENTITIES)
        {
            visible = false;
            player = p;
            
            selector = new(Vector2.Zero, "selector", 16, 16, Drawing.DrawOrder.ENTITIES);
            selector.AddAnimation("a", CreateAnimFrameArray(0, 1), 4);
            selector.Play("a");
            selector.exists = false;

            selected_tile = new(Vector2.Zero, "selector", 16, 16, Drawing.DrawOrder.BG_ENTITIES);
            selected_tile.SetFrame(1);
            selected_tile.exists = false;
        }

        public override void Update()
        {
            base.Update();
            selector.Position = (player.Center/16 + FacingDirection(player.facing)).ToPoint().ToVector2() * 16;
        }

        public override void OnEvent(GameEvent e)
        {
            base.OnEvent(e);
            selected_tile.exists = false;
            selector.exists = false;
        }

        public void OnAction()
        {
            var check = GlobalState.Map.CheckSwapper(GlobalState.Map.ToMapLoc(selector.Center));
            if(GlobalState.events.GetEvent("SeenCredits") == 0)
            {
                if(check != MapData.SwapperControl.State.Allow)
                {
                    GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("misc","any", "swap",2);
                    return;
                }
            }
            else if(check == MapData.SwapperControl.State.Disallow)
            {
                GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("misc", "any", "swap", (GlobalState.CURRENT_MAP_NAME == "DEBUG") ? 0 : 1);
                return;
            }

            if(!selector.exists)
            {
                selector.exists = true;
            }
            else if(!selected_tile.exists)
            {
                selected_tile.Position = selector.Position;
                selected_tile.exists = true;
                Sounds.SoundManager.PlaySoundEffect("menu_move");
            }
            else
            {
                Point p1 = GlobalState.Map.ToMapLoc(selector.Center);
                Point p2 = GlobalState.Map.ToMapLoc(selected_tile.Center);
                int t1 = GlobalState.Map.GetTile(MapData.Layer.BG, p1);
                int t2 = GlobalState.Map.GetTile(MapData.Layer.BG, p2);
                GlobalState.Map.ChangeTile(MapData.Layer.BG, p1, t2);
                GlobalState.Map.ChangeTile(MapData.Layer.BG, p2, t1);
                selector.exists = false;
                selected_tile.exists = false;
                Sounds.SoundManager.PlaySoundEffect("menu_select");
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { selector, selected_tile };
        }
    }
}
