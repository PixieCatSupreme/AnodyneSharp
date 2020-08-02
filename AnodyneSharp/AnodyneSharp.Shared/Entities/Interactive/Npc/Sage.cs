using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;

namespace AnodyneSharp.Entities
{
    [NamedEntity(map: "BLANK")]
    class Sage : Entity
    {
        EntityPreset _preset;
        public Sage(EntityPreset preset, Player p) : base(preset.Position, DrawOrder.ENTITIES)
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
