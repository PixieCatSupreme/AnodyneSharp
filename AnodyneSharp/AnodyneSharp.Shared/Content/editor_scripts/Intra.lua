--seen.lua
--a simple sprite exporter that outputs a single level node per
--set of sprites

groups = DAME.GetGroups()
groupCount = as3.tolua(groups.length) -1

DAME.SetFloatPrecision(0)

tab1 = "  "
tab2 = "    "
tab3 = "      "
tab4 = "        "

dataDir = as3.tolua(VALUE_DataDir)
levelName = as3.tolua(VALUE_LevelName)


function exportMapCSV( mapLayer, mapName, layerSimpleName )
	-- get the raw mapdata. To change format, modify the strings passed in (rowPrefix,rowSuffix,columnPrefix,columnSeparator,columnSuffix)
	mapText = as3.tolua(DAME.ConvertMapToText(mapLayer,"","\n","",",",""))
	--print("output to "..as3.tolua(VALUE_CSVDir).."/"..layerFileName)
	
	DAME.WriteFile(dataDir.."/Maps/"..mapName.."/"..layerSimpleName..".csv", mapText );
end

function exportSwapperShapes( layer, mapName )
    boxText = "%prop:type%\t%xpos%\t%ypos%\t%width%\t%height%\n"
    shapeText = as3.tolua(DAME.CreateTextForShapes(layer,"",boxText,""))
    
    DAME.WriteFile(dataDir.."/Maps/"..mapName.."/Swapper.dat",shapeText)
end

function exportMapSettings( layer, mapName )
    local propertiesString = "\"Settings\":{%%proploop%%"
	propertiesString = propertiesString.."\"%propname%\":%propvaluestring%%separator:,%"
    propertiesString = propertiesString.."%%proploopend%%}"
    
    local textString = "{"..propertiesString..",\"Event\":\"%text%\"},"
    local allEvents = "\"Events\":["..string.sub(as3.tolua(DAME.CreateTextForShapes(layer,"","",textString)),1,-2).."]"
    
    local shapeString = "{\"X\":%xpos%,\"Y\":%ypos%,\"Width\":%width%,\"Height\":%height%,"..propertiesString.."},"
    local allShapes = "\"Areas\":["..string.sub(as3.tolua(DAME.CreateTextForShapes(layer,"",shapeString,"")),1,-2).."]"
    allShapes = string.gsub(allShapes,"0%.,","0,")
    
    local defaultString = as3.tolua(DAME.GetTextForProperties( propertiesString, layer.properties ))
    
    local Text = "{"..defaultString..","..allEvents..","..allShapes.."}"
    DAME.WriteFile(dataDir.."/Maps/"..mapName.."/Settings.json",Text)
end

-- This is the file for the map level class.
fileText = ""
maps = {}
spriteLayers = {}


for groupIndex = 0,groupCount do
	group = groups[groupIndex]
	groupName = as3.tolua(group.name)
	groupName = string.gsub(groupName, " ", "_")
	layerCount = as3.tolua(group.children.length) - 1
	
	
	-- Go through each layer and store some tables for the different layer types.
	for layerIndex = 0,layerCount do
		layer = group.children[layerIndex]
		isMap = as3.tolua(layer.map)~=nil
		layerSimpleName = as3.tolua(layer.name)
		layerSimpleName = string.gsub(layerSimpleName, " ", "_")
		layerName = groupName..layerSimpleName
		if as3.tolua(layer.IsSpriteLayer()) == true then
			table.insert( spriteLayers,{groupName,layer,layerName,layerSimpleName})
        elseif as3.tolua(layer.IsShapeLayer()) == true then
            if layerSimpleName == "Swapper" then
                exportSwapperShapes(layer, groupName)
            end
            if layerSimpleName == "Settings" then
                exportMapSettings(layer, groupName)
            end
        end
        if isMap == true then
			-- Generate the map file.
			exportMapCSV( layer, groupName, layerSimpleName )
		end
	end
    
    	
    
end



-- create the sprites.
fileText = fileText.."<root>\n";
for i,v in ipairs(spriteLayers) do
	spriteLayer = spriteLayers[i]
	fileText = fileText..tab1.."<map name=\""..tostring(as3.tolua(spriteLayer[1])).."\" type=\""..tostring(as3.tolua(spriteLayer[4])).."\">\n"
	
	val_p = "%%if prop:p%%".." p=\"%prop:p%\"".."%%endprop%%"
	val_alive = "%%if prop:alive%%".." alive=\"%prop:alive%\"".."%%endprop%%"
	val_type = "%%if prop:type%%".." type=\"%prop:type%\" ".."%%endprop%%"
    val_link = "%%if link%% linkid=\"%linkid%\"%%endiflink%%"
    
    val_linksfromthis = "%%if linkto%% linkgroup=\"%linkid%%%linkstoloop%%,%linktoid%%%linkloopend%%\"%%endiflink%%"
	
	creationText = tab2.."<%class% x=\"%xpos%\" y=\"%ypos%\" guid=\"%guid%\" frame=\"%frame%\""..val_p..val_alive..val_type..val_link..val_linksfromthis.."/>\n"
	fileText = fileText..as3.tolua(DAME.CreateTextForSprites(spriteLayers[i][2],creationText,"Avatar"))
	fileText = fileText..tab1.."</map>\n"
end

fileText = fileText.."</root>\n";
	
-- Save the file!

DAME.WriteFile(dataDir.."\\"..levelName..".xml", fileText )

return 1
