using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Doors
{
    [NamedEntity("Door", "17"), Collision(typeof(Player))]
    public class NexusDoor : Door, Interactable
    {
        NexusPreview _preview;
        Entity _nexusGem;

        bool activated = false;

        public NexusDoor(EntityPreset preset, Player player)
            : base(preset, player, "teleport_up")
        {
            visible = false;

            width = 32;
            height = 26;
            offset = new Vector2(0, 4);

            Position += new Vector2(0, 6);

            teleportOffset = new Vector2(10, 8);

            _preview = new NexusPreview(preset.Position, LinkedMapName, player);

            _nexusGem = new Entity(new Vector2(Position.X, Position.Y - 4), "nexus_cardgem", 32, 16, DrawOrder.ENTITIES);
            _nexusGem.visible = CardDataManager.GotAllNormalCards(LinkedMapName);
        }

        public override void Update()
        {
            base.Update();
            if(activated && _player.JustLanded)
            {
                TeleportPlayer();
            }
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (_preview.exists && player_direction == Facing.UP && !activated)
            {
                activated = true;
                _player.AutoJump(0.4f,_player.Position - Vector2.UnitY*16);
                return true;
            }
            return false;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { _preview, _nexusGem };
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }

        [Collision(typeof(Player))]
        public class NexusPreview : Entity
        {
            Color target;
            Color start;

            float progress;

            Player _player;
            bool _playerInArea;

            static int AnimStart(string map)
            {
                int mapID = (int)Enum.Parse(typeof(GameConstants.MapOrder), map);
                return mapID * 4;
            }

            public static AnimatedSpriteRenderer GetSprite(int start) => new("nexus_door_preview_overlay", 32, 32,
                    new Anim("stop",new int[] { start },1),
                    new Anim("preview", new int[] { start, start + 1, start + 2, start + 3 },10)
                );

            public NexusPreview(Vector2 position, string map, Player player)
                : base(position, GetSprite(AnimStart(map)), DrawOrder.BG_ENTITIES)
            {
                height += 25;

                if (!GlobalState.events.ActivatedNexusPortals.Contains(map))
                {
                    exists = false;
                }

                _player = player;

                sprite.Color = target = start = new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            public void SetActive(bool active)
            {
                if (_playerInArea == active) return;

                _playerInArea = active;

                start = sprite.Color;
                progress = 0;

                if (active)
                {
                    target = Color.White;
                    Play("preview");
                }
                else
                {
                    target = new Color(0.5f, 0.5f, 0.5f, 1f);
                    Play("stop");
                }
            }

            public override void Update()
            {
                if (sprite.Color != target)
                {
                    progress = Math.Min(progress + 1.2f * GameTimes.DeltaTime, 1f);

                    sprite.Color = Color.Lerp(start, target, progress);
                }

                if (_playerInArea && !_player.Hitbox.Intersects(Hitbox))
                {
                    SetActive(false);
                }
            }

            public override void Collided(Entity other)
            {
                SetActive(true);
            }
        }
    }
}
