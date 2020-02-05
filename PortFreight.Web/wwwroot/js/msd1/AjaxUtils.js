$(function () {
    $.ajaxSetup({ cache: false });
});

function FillCargoCategory() {
    alert("Hello from FillCargoCategory Inside Ajax Utils");
}

function ajaxGetWithData(url, data, success, error, beforeSend, complete) {
    ajaxCall(url, data, "GET", success, error, beforeSend, complete, false);
}

function ajaxCall(url, data, type, success, error, beforeSend, complete, synchronous) {
    $.ajax(url, {
        data: data,
        type: type,
        async: !synchronous,
        dataType: "json",
        contentType: "application/json",
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
                window.toastr.error(jqXHR.responseText ? jqXHR.responseText : textStatus);
            }
        },
        complete: function (jqXHR, textStatus) {
            if (complete) {
                complete(jqXHR, textStatus);
            }
        }
    });
}