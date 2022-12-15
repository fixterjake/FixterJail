fx_version "cerulean"
game "gta5"
author "fixterjake @ https://github.com/fixterjake"
description "https://github.com/fixterjake/FixterJail"

ui_page("ui/index.html")

client_script "FixterJail.Client.net.dll" 
server_script "FixterJail.Server.net.dll"

files {
	"ui/**/*",
	"Newtonsoft.Json.dll",
	"config.json"
}

max_jail_time "600" -- 10 minutes