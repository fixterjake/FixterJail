fx_version "cerulean"
game "gta5"
author "fixterjake @ https://github.com/fixterjake"
description "https://github.com/fixterjake/FixterJail"

ui_page("ui/index.html")

client_scripts "FixterJail.Client.net.dll" 
server_script "FixterJail.Server.net.dll"

files {
	"ui/**/*",
	"Newtonsoft.Json.dll"
}