let currentPlayers = {};

function goToFirstPage() {
	$("#first").css('display', "block");
	$("#second").css('display', "none");
	$("#idErrorPage").css('display', "none");
}
function goToSecondPage() {
	$("#first").css('display', "none");
	$("#second").css('display', "block");
	$("#idErrorPage").css('display', "none");
}

function goToCharIDError() {
	$("#first").css('display', "none");
	$("#second").css('display', "none");
	$("#idErrorPage").css('display', "block");
}

function exampleButton() {
	var jailtime = $("#editjailtime").val();
	var jailreason = $("#editjailreason").val()
	var jailId = document.getElementById('enterid').value;

	clearAllFields();
	$.post("https://fixter-jail/jailNuiCallback", JSON.stringify({
		time: jailtime,
		reason: jailreason,
		id: jailId
	}));


}


function checkID() {
	var id = document.getElementById('enterid').value;
	if (id > 0 && Number.isInteger(Number(id))) {
		goToSecondPage();
	}
	else {
		goToCharIDError();
	}
}

function closeUI() {
	$("#first").css('display', "none");
	$("#second").css('display', "none");
	$("#idErrorPage").css('display', "none");

	clearAllFields();
	$.post('https://fixter-jail/closeUI', JSON.stringify({}));
}

function clearAllFields() {
	$("#enterid").val("");
	$("#editjailtime").val("");
	$("#editjailreason").val("");
	$("#editfirstname").val("");
	$("#editsecondname").val("");
}
$(function () {
	window.addEventListener('message', function (event) {
		if (event.data.type == "DISABLE_ALL_UI") {
			$("#first").css('display', "none"); // For each section you add to HTML you need to add it here as well
			$("#second").css('display', "none");
			$("#idErrorPage").css('display', "none");
		}
		else if (event.data.type == "DISPLAY_JAIL_UI") {
			currentPlayers = event.data.players;
			var x = document.getElementById("enterid");
			x.innerHTML = "";

			var option = document.createElement("option");
			option.value = "";
			option.text = " -- Select a player -- ";
			x.add(option);

			for (key in currentPlayers) {
				var option = document.createElement("option");
				option.value = key;
				option.text = currentPlayers[key];
				x.add(option);
			}

			goToFirstPage();
		}
		else if (event.data.type == "DISPLAY_JAIL_ID_ERROR") {
			goToCharIDError();
		}
	});


	//This limits key controls on fields:
	$('#enterid').bind("paste", function (e) { e.preventDefault(); });
	enterid.onkeypress = function (e) {
		if (!((e.keyCode > 95 && e.keyCode < 106) || (e.keyCode > 47 && e.keyCode < 58) || e.keyCode == 8 || e.keyCode == 9 || (e.keyCode >= 97 && e.keyCode <= 122))) { //if key is not 0-9 or backspace
			return false;
		}
	}
});