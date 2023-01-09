using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;

namespace AnodyneSharp.Entities
{
    [NamedEntity(xmlName:null,type:null,frames:0), Collision(typeof(Player))]
    public class Console : Entity, Interactable
    {
        EntityPreset _preset;

        public static AnimatedSpriteRenderer GetSprite() => new("console", 16, 16,
            new Anim("active", new int[] { 0, 1 },5),
            new Anim("green",new int[] { 2 },1)
            );

        public Console(EntityPreset preset, Player p) : base(preset.Position, GetSprite(), DrawOrder.ENTITIES)
        {
            _preset = preset;
            immovable = true;

            if(preset.Activated)
            {
                Play("green");
                GlobalState.PUZZLES_SOLVED++;
            }
            else
            {
                Play("active");
            }
        }

        public override void Collided(Entity other)
        {
            Separate(other, this);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if(!_preset.Activated)
            {
                _preset.Activated = true;
                Play("green");
                GlobalState.PUZZLES_SOLVED++;
                SoundManager.PlaySoundEffect("get_small_health");
                return true;
            }
            return false;
        }
    }
}
