using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [NamedEntity("NPC", "rock"), Collision(typeof(Player))]
    class Rock : Entity, Interactable
    {
        string scene;

        public Rock(EntityPreset preset, Player p) 
            : base(preset.Position, 16, 16, DrawOrder.ENTITIES)
        {
            immovable = true;

            string texName = "note_rock";
            int f = 0;

            if (GlobalState.CURRENT_MAP_NAME == "SPACE")
            {
                texName = "space_npcs";

                f = MapUtilities.GetRoomCoordinate(Position).X > 5 ? 31 : 30;
            }
            else if (GlobalState.CURRENT_MAP_NAME == "CELL")
            {
                f = 1;
            }

            SetTexture(texName, 16, 16);

            SetFrame(f);

            scene = MathUtilities.IntToString(preset.Frame + 1);
        }

        public override void Collided(Entity other)
        {
            Separate(other, this);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("rock", scene);

            if (GlobalState.events.GetEvent("RockTalk") == 0)
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("misc", "any", "rock", 0) + "^\n" + GlobalState.Dialogue;

                GlobalState.events.IncEvent("RockTalk");
            }
            return true;
        }

    }
}
