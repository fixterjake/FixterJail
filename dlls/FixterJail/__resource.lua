resource_manifest_version '44febabe-d386-4d18-afbe-5e627f4af937'

ui_page('ui/index.html')

client_scripts {
	"Fixter.Jail.Client.net.dll",
	"JailNUIHelper.lua"
} 
server_script "Fixter.Jail.Server.net.dll"

files {
	'ui/index.html',
	'ui/app.js',
	'ui/css/dojjail.css',
	'ui/vendor/bootstrap/css/bootstrap.min.css',
	'ui/vendor/bootstrap/css/bootstrap.min.css.map',
	'ui/vendor/bootstrap/js/bootstrap.min.js',
	'ui/vendor/bootstrap/js/bootstrap.min.js.map',
    'Newtonsoft.Json.dll'
}