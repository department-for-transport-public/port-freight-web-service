$(document).ready(function () {
    accessibleAutocomplete({
        element: document.querySelector('#Input_ReportingPort-container'),
        id: 'Input_ReportingPort',
        source: reportingports,
        autoselect: true,
        name: "reportingport"
    });

    
    accessibleAutocomplete({
        element: document.querySelector('#Input_AssociatedPort-container'),
        id: 'Input_AssociatedPort',
        source: associatedports,
        autoselect: true,
        name: "associatedport"
    });

    $("#Input_AssociatedPort").keydown(function (event) {
        if (event.keyCode == 13 || window.event.keyCode == 13) {
            if ($("#Input_AssociatedPort").val().length > 0) {
                var options = $("#Input_AssociatedPort").autocomplete("option");
                var fromAutocomplete = false;
                jQuery.each(options.source, function (index, device) {
                    if ($("#Input_AssociatedPort").val() == options.source[index]) {
                        fromAutocomplete = true;
                    }
                });
                if (!fromAutocomplete) {
                    $("#Input_AssociatedPort").val("");
                    return true;
                }
            }
            else {
                $("#Input_AssociatedPort").val('');
                return true;
            }
        }
    });

    $('#_divVoyageDetails').hide();

    $('#Input_IsInbound').click(function () {
        showVoyageDiv();
        setPortOfLoadingText();
    });

    $('#Input_IsOutbound').click(function () {
        showVoyageDiv();
        setPortOfDischargeText();
    });

    if ($('#Input_IsInbound').is(':checked')) {
        showVoyageDiv();
        setPortOfLoadingText();
    }
    else if ($('#Input_IsOutbound').is(':checked')) {
        showVoyageDiv();
        setPortOfDischargeText();
    }
});

function showVoyageDiv() {
    $('#_divVoyageDetails').show();
}

function setPortOfLoadingText() {
    $('#_lblPort').text('Port of loading');
    $('#_spPortHint').text('Port at which the cargo was loaded onto the vessel');
    $('#_dateOfVoyageHint').text('If there was only one voyage, enter the date the vessel arrived');
}

function setPortOfDischargeText() {
    $('#_lblPort').text('Port of discharge');
    $('#_spPortHint').text('Port at which the cargo was discharged from the vessel');
    $('#_dateOfVoyageHint').text('If there was only one voyage, enter the date the vessel departed')
}

function storeVariable(reportingPort){
    localStorage['myKey'] = reportingPort;
}



