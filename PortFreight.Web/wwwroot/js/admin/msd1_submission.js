$(document).ready(function () {
    $("#Input_StatisticalPort").autocomplete({
        autoFocus: true,
        source: statPortsList,
        minLength: 3,
        delay: 200,
        change: function (event, ui) {
            if (!ui.item) {
                $(this).val('');
            }
        }
    });
});