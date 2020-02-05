
$(document).ready(function () {
    hideTextInputs();

    if (isEditMode === 'True') {
        showRelevantInputs(textInputDisplayDriver);

        ddl = $("#CargoItem_Category");

        ddl = $("#CargoItem_Group");
        if (ddl[0][0].value === "") {
            ddl[0][0].parentNode.removeChild(ddl[0][0]);
        }
    }
    else {
        $('#CargoItem_Category').prop('disabled', true);
    }

    $("#divAddCargoItem").children().hide();
    $("#divSkipPage").children().hide();
    $("#_divDescription").children().hide();

    if ($('#CargoItem_Group').val() != '' && $('#CargoItem_Category').val() != '') {
        $("#divAddCargoItem").children().show();
        $("#divSkipPage").children().show();
        $("#_divDescription").children().show();
        
    }

    $('#CargoItem_Group').change(updateAddEnabled);
    $('#CargoItem_Category').change(updateAddEnabled);

});

function updateAddEnabled() {
    if (verifyAdSettings()) {
        $("#divAddCargoItem").children().show();
        $("#_divDescription").children().show();
    } else {
        $("#divAddCargoItem").children().hide();
        $("#_divDescription").children().hide();
    }
    if ($('#CargoItem_Group').val() != '') {
        $("#divSkipPage").children().show();
    }
    else {
        $("#divSkipPage").children().hide();
    }
}

function verifyAdSettings() {
    if ($('#CargoItem_Group').val() != '' && $('#CargoItem_Category').val() != '') {
        return true;
    } else {
        return false
    }
}

function SuccessFunc(data, status, jqXHR) {

    ddl = $("#CargoItem_Group");
    if (ddl[0][0].value === "") {
        ddl[0][0].parentNode.removeChild(ddl[0][0]);
    }
    $('#CargoItem_Category').prop('disabled', false);
    $('#CargoItem_Category').empty();


    $('#CargoItem_Category').append($('<option></option>').val('').text('Please Select'));
    $.each(data, function (i, item) {
        $('#CargoItem_Category').append($('<option></option>').val(item.value).text(item.text));
    });
}

function showRelevantInputs(data) {
    if (data.indexOf("Weight") >= 0) {
        $('#_divGrossWeightOfGoods').show();
    }

    if (data.indexOf("NoCargo") >= 0) {
        $('#_divNumUnits').show();
    }

    else if (data.indexOf("Cargo") >= 0) {

        $('#_divUnitsWithCargo').show();
        $('#_divUnitsWithoutCargo').show();
    }

    $('#_divDescription').show();
    $('#divAddCargoItem').show();

}
function SuccessFuncUi(data, status, jqXHR) {

    ddl = $("#CargoItem_Category");
    if (ddl[0][0].value === "") {
        ddl[0][0].parentNode.removeChild(ddl[0][0]);
    }
   
    showRelevantInputs(data);
}

function hideTextInputs() {
    $('#_divNumUnits').hide();
    $('#_divUnitsWithCargo').hide();
    $('#_divUnitsWithoutCargo').hide();
    $('#_divGrossWeightOfGoods').hide();
    $('#_divDescription').hide();
    $('#divAddCargoItem').hide();
   
}

function clearTextInputs() {
    $('#CargoItem_TotalUnits').val("");
    $('#CargoItem_UnitsWithCargo').val("");
    $('#CargoItem_UnitsWithoutCargo').val("");
    $('#CargoItem_GrossWeight').val("");
    $('#CargoItem_Description').val("");
}

$("#CargoItem_Group").change(function () {
    if ($('#CargoItem_Group').val() === "") {
        return false;
    }
    hideTextInputs();
    clearTextInputs();
    ajaxGetWithData('?handler=CargoCategories', $('#CargoItem_Group').val(), SuccessFunc, null, null, null, null);
});

$("#CargoItem_Category").change(function () {
    hideTextInputs();
    clearTextInputs();
    ajaxGetWithData('?handler=UiFormat', $('#CargoItem_Category').val(), SuccessFuncUi, null, null, null, null);
});

function ajaxGetWithData(url, data, success, error, beforeSend, complete) {
    ajaxCall(url, data, "GET", success, error, beforeSend, complete, false);
}

function ajaxCall(url, data, type, success, error, beforeSend, complete, synchronous) {
    $.ajax(url, {
        data: { input: data },
        type: type,
        async: !synchronous,
        dataType: "json",
        contentType: "application/json",
        cache: false,
        beforeSend: function (jqXHR, settings) {
            if (beforeSend) {
                beforeSend(jqXHR, settings);
            }
        },
        success: function (data, textStatus, jqXHR) {
            if (success) {
                success(data, textStatus, jqXHR);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (error) {
                error(jqXHR, textStatus, errorThrown);
            } else {
                alert("Problem with AJAX call");
            }
        },
        complete: function (jqXHR, textStatus) {
            if (complete) {
                complete(jqXHR, textStatus);
            }
        }
    });
}
