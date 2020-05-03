using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing
{
    public enum DrawOrder
    {
        BACKGROUND,         //Moving background textures
        MAP_BG,             //Map layer 1
        PLAYER_REFLECTION,  //Reflection of the player
        BROOM_REFLECTION,   //Reflection of broom
        FOOT_OVERLAY,       //Feet stuff like water wrinkles
        MAP_BG2,            //Map layer 2
        SHADOWS,            //Shadows
        PARTICLES,          //Things like particles, keys, blood etc
        ENTITIES,           //Enemies, the player and other npcs
        FG_SPRITES,         //Bullets and other things that have to be drawn over entities
        MAP_FG,             //Foreground layer of the map
        DEC_OVER,           //Screen wide effects
        DARKNESS,           //Darkens the screen
        HEADER,             //Player UI
        UI_OBJECTS,         //Health, keys, mini mini map
        EQUIPPED_BORDER,    //The border of the equipped broom
        HEALTH_UPGRADE,     //Health cicida. Needs to be drawn on UI so it can fly over the header
        PAUSE_BG,           //Background for pause menu. Needs to be drawn over health upgrades. 
        EQUIPMENT_ICON,     //The inventory items for the broom types
        EQUIPPED_ICON,      //The small equipped indicator for the broom types
        AUDIO_SLIDER_BG,    //The black background of the audio slider
        AUDIO_SLIDER_BAR,   //The red bar to show progress of slider
        AUDIO_SLIDER,       //The slider itself.
        PAUSE_SELECTOR,     //Pause menu option selector
        TEXTBOX,            //Textboxes
        TEXT,               //Text in dialogueBoxes
        SUBMENU_SELECTOR,   //Equipment selector, config selector and card selector
        DEATH_FADEIN,       //A fade-in that happens on player death
        DEATH_TEXT,         //Text that appears on player death
        BLACK_OVERLAY       //Black fadeout on map transition and when player continues after death
    }

    public static class DrawingUtilities
    {

        public static float GetDrawingZ(DrawOrder order, float gridy = 0)
        {
            float z = (float)order;
            if(order == DrawOrder.ENTITIES)
            {
                z += gridy / (GameConstants.SCREEN_HEIGHT_IN_PIXELS + 1); //+1 to prevent z-fighting with next layer
            }
            return z/(float)DrawOrder.BLACK_OVERLAY;
        }
    }
}
