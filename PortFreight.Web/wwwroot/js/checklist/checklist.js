$(document).ready(function () {
    var date = new Date();
    var currentQtr = parseInt(date.getMonth() / 3) + 1;
    $("#cargoQTR1, #cargoQTR2, #cargoQTR3, #cargoQTR4").addClass("disabled");
    $("#agentQTR1, #agentQTR2, #agentQTR3, #agentQTR4").addClass("disabled");

    $('.cargo-qtr1, .cargo-qtr2, .cargo-qtr3, .cargo-qtr4').addClass("outstanding").html('Outstanding');
    $('.agent-qtr1, .agent-qtr2, .agent-qtr3, .agent-qtr4').addClass("outstanding").html('Outstanding');

    $('.cargo-pqtr1, .cargo-pqtr2, .cargo-pqtr3, .cargo-pqtr4').addClass("outstanding").html('Outstanding');
    $('.agent-pqtr1, .agent-pqtr2, .agent-pqtr3, .agent-pqtr4').addClass("outstanding").html('Outstanding');

    switch (currentQtr) {
        case 1:
            $("#cargoQTR1, #agentQTR1").removeClass("disabled");
            $('.cargo-qtr2, .cargo-qtr3, .cargo-qtr4, .agent-qtr2, .agent-qtr3, .agent-qtr4').removeClass('outstanding').addClass("future").html('Not available');
            break;
        case 2:
            $("#cargoQTR1, #cargoQTR2, #agentQTR1, #agentQTR2").removeClass("disabled")
            $('.cargo-qtr3, .cargo-qtr4, .agent-qtr3, .agent-qtr4').removeClass('outstanding').addClass("future").html('Not available');
            break;
        case 3:
            $("#cargoQTR1, #cargoQTR2, #cargoQTR3, #agentQTR1, #agentQTR2, #agentQTR3").removeClass("disabled");
            $('.cargo-qtr4, .agent-qtr4').removeClass('outstanding').addClass("future").addClass("disabled").html('Not available');
            break;
        case 4:
            $("#cargoQTR1, #cargoQTR2, #cargoQTR3, #cargoQTR4, #agentQTR1, #agentQTR2, #agentQTR3, #agentQTR4").removeClass("disabled");
            break;
    };

    pagerefresh();

    function pagerefresh() {
        if (currentYearQtrsCargo.indexOf(1) >= 0) {
            $('.cargo-qtr1').removeClass('outstanding future').addClass('submitted').html('Submitted');
        }
        if (currentYearQtrsAgent.indexOf(1) >= 0) {
            $('.agent-qtr1').removeClass('outstanding future').addClass('submitted').html('Submitted');
        }
        if (currentYearQtrsCargo.indexOf(2) >= 0) {
            $('.cargo-qtr2').removeClass('outstanding future').addClass('submitted').html('Submitted');
        }
        if (currentYearQtrsAgent.indexOf(2) >= 0) {
            $('.agent-qtr2').removeClass('outstanding future').addClass('submitted').html('Submitted');
        }
        if (currentYearQtrsCargo.indexOf(3) >= 0) {
            $('.cargo-qtr3').removeClass('outstanding future').addClass('submitted').html('Submitted');
        }
        if (currentYearQtrsAgent.indexOf(3) >= 0) {
            $('.agent-qtr3').removeClass('outstanding future').addClass('submitted').html('Submitted');
        }
        if (currentYearQtrsCargo.indexOf(4) >= 0) {
            $('.cargo-qtr4').removeClass('outstanding future').addClass('submitted').html('Submitted');
        }
        if (currentYearQtrsAgent.indexOf(4) >= 0) {
            $('.agent-qtr4').removeClass('outstanding future').addClass('submitted').html('Submitted');
        }
        if (previousYearQtrsCargo.indexOf(1) >= 0) {
            $('.cargo-pqtr1').removeClass('outstanding').addClass('submitted').html('Submitted');
        }
        if (previousYearQtrsAgent.indexOf(1) >= 0) {
            $('.agent-pqtr1').removeClass('outstanding').addClass('submitted').html('Submitted');
        }
        if (previousYearQtrsCargo.indexOf(2) >= 0) {
            $('.cargo-pqtr2').removeClass('outstanding').addClass('submitted').html('Submitted');
        }
        if (previousYearQtrsAgent.indexOf(2) >= 0) {
            $('.agent-pqtr2').removeClass('outstanding').addClass('submitted').html('Submitted');
        }
        if (previousYearQtrsCargo.indexOf(3) >= 0) {
            $('.cargo-pqtr3').removeClass('outstanding').addClass('submitted').html('Submitted');
        }
        if (previousYearQtrsAgent.indexOf(3) >= 0) {
            $('.agent-pqtr3').removeClass('outstanding').addClass('submitted').html('Submitted');
        }
        if (previousYearQtrsCargo.indexOf(4) >= 0) {
            $('.cargo-pqtr4').removeClass('outstanding').addClass('submitted').html('Submitted');
        }
        if (previousYearQtrsAgent.indexOf(4) >= 0) {
            $('.agent-pqtr4').removeClass('outstanding').addClass('submitted').html('Submitted');
        }
    }
});