$(document).ready(function () {
    $("#Input_AgentSenderId").autocomplete({
        autoFocus: true,
        source: agentsenders,
        minLength: 3,
        delay: 200,
        change: function (event, ui) {
            if (!ui.item) {
                $(this).val('');
            }
        }
    });

    $("#Input_LineSenderId").autocomplete({
        autoFocus: true,
        source: linesenders,
        minLength: 3,
        delay: 200,
        change: function (event, ui) {
            if (!ui.item) {
                $(this).val('');
            }
        }
    });

    $("#Input_AgentSenderId").keydown(function (event) {
        if (event.keyCode == 13 || window.event.keyCode == 13) {
            if ($("#Input_AgentSenderId").val().length > 0) {
                var options = $("#Input_AgentSenderId").autocomplete("option");
                var fromAutocomplete = false;
                jQuery.each(options.source, function (index, device) {
                    if ($("#Input_AgentSenderId").val() == options.source[index]) {
                        fromAutocomplete = true;
                    }
                });
                if (!fromAutocomplete) {
                    $("#Input_AgentSenderId").val("");
                    return true;
                }
            }
            else {
                $("#Input_AgentSenderId").val('');
                return true;
            }
        }
    });
});

$("#Input_LineSenderId").keydown(function (event) {
    if (event.keyCode == 13 || window.event.keyCode == 13) {
        if ($("#Input_LineSenderId").val().length > 0) {
            var options = $("#Input_LineSenderId").autocomplete("option");
            var fromAutocomplete = false;
            jQuery.each(options.source, function (index, device) {
                if ($("#Input_LineSenderId").val() == options.source[index]) {
                    fromAutocomplete = true;
                }
            });
            if (!fromAutocomplete) {
                $("#Input_LineSenderId").val("");
                return true;
            }
        }
        else {
            $("#Input_LineSenderId").val('');
            return true;
        }
    }
});