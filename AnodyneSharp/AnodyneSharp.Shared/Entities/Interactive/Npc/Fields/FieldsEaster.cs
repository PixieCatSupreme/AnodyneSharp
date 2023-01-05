using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [NamedEntity("NPC", "generic", 13, 14, 15, 16), Collision(typeof(Player))]
    public class FieldsEaster : Entity, Interactable
    {
        string scene;

        private static AnimatedSpriteRenderer GetSprite(int frame)
        {
            int baseFrame = frame * 2 + 80 - 26;

            Anim anim = new("a", new int[] { baseFrame, baseFrame + 1 }, 4);

                return new AnimatedSpriteRenderer("fields_npcs", 16, 16, anim);
        }

        public FieldsEaster(EntityPreset preset, Player p) 
            : base(preset.Position, GetSprite(preset.Frame), Drawing.DrawOrder.ENTITIES)
        {
            scene = preset.Frame switch
            {
                13 => "hamster",
                14 => "chikapu",
                15 => "electric",
                16 => "marvin",
                _ => "ERROR"
            };

            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            //area specification is necessary bc Marvin(frame 16) got moved to BEACH
            GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("generic_npc", "FIELDS", scene);
            return true;
        }
    }
}
