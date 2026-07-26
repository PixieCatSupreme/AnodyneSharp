-- Display the settings for the exporter.
DAME.AddHtmlTextLabel("Exports Entity.xml and CSV map files")
DAME.AddBrowsePath("Xml dir:","DataDir",false, "Where you place the xml files.")

DAME.AddTextInput("Level Name", "", "LevelName", true, "The name you wish to call this level." )

return 1
