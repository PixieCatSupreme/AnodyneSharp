using AnodyneSharp.Drawing;
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
            offset = new Vector2(6, 4);

            Position += new Vector2(6, 6);

            teleportOffset = new Vector2(10, 8);

            _preview = new NexusPreview(preset.Position, LinkedMapName, player);

            _nexusGem = new Entity(new Vector2(Position.X - 6, Position.Y - 4), "nexus_cardgem", 32, 16, DrawOrder.ENTITIES);
            _nexusGem.SetFrame(0);
            _nexusGem.visible = CardDataManager.GotAllNormalCards(LinkedMapName);
        }

        public override void Update()
        {
            base.Update();
            if(activated && _player.just_landed)
            {
                TeleportPlayer();
                activated = false;
            }
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (_preview.exists && player_direction == Facing.UP)
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
        private class NexusPreview : Entity
        {
            Color target;
            Color start;

            float progress;

            Player _player;
            bool _playerInArea;

            public NexusPreview(Vector2 position, string map, Player player)
                : base(position, "nexus_door_preview_overlay", 32, 32, DrawOrder.BG_ENTITIES)
            {
                height += 25;

                if (!GlobalState.events.ActivatedNexusPortals.Contains(map))
                {
                    exists = false;
                }

                int mapID = (int)Enum.Parse(typeof(GameConstants.MapOrder), map);
                int animStart = mapID * 4;

                AddAnimation("preview", CreateAnimFrameArray(animStart, animStart + 1, animStart + 2, animStart + 3), 10);
                AddAnimation("stop", CreateAnimFrameArray(animStart), 10);

                _player = player;

                color = target = start = new Color(0.5f, 0.5f, 0.5f, 1f);

                Play("stop");
            }

            public void SetActive(bool active)
            {
                if (_playerInArea == active) return;

                _playerInArea = active;

                start = color;
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
                if (color != target)
                {
                    progress = Math.Min(progress + 1.1f * GameTimes.DeltaTime, 1f);

                    color = Color.Lerp(start, target, progress);
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
