using AnodyneSharp.Registry;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [NamedEntity("NPC","generic",13,14,15,16), Collision(typeof(Player))]
    public class FieldsEaster : Entity, Interactable
    {
        string scene;

        public FieldsEaster(EntityPreset preset, Player p) : base(preset.Position, "fields_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            int base_frame = preset.Frame * 2 + 80 - 26;
            AddAnimation("a", CreateAnimFrameArray(base_frame, base_frame + 1));
            Play("a");
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
