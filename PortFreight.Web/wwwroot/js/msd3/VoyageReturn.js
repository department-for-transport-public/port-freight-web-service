$(document).ready(function () {
    $("#Input_SenderId").autocomplete({
        autoFocus: true,
        source: senders,
        minLength: 3,
        delay: 200,
        change: function (event, ui) {
            if (!ui.item) {
                $(this).val('');
            }
        }
    });
});