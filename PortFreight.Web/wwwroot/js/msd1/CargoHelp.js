$(document).ready(function () {
    var acc = document.getElementsByClassName("accordion");
    var i;
    for (i = 0; i < acc.length; i++) {
        acc[i].addEventListener("click", function () {
            this.classList.toggle("active");
            var panel = this.nextElementSibling;
            if (panel.style.maxHeight) {
                panel.style.maxHeight = null;
            } else {
                panel.style.maxHeight = panel.scrollHeight + "px";
            }
        });
    }
    $('.closeall').click(function () {
        var pnl = document.getElementsByClassName("panel");
        var acc = document.getElementsByClassName("accordion");
        var i;
        for (i = 0; i < pnl.length; i++) {
            if (pnl[i].style.maxHeight) {
                acc[i].classList.toggle("active");
                pnl[i].style.maxHeight = null;
            } else {
            }
        }
    });
    $('.openall').click(function () {
        var pnl = document.getElementsByClassName("panel");
        var acc = document.getElementsByClassName("accordion");
        var i;
        for (i = 0; i < pnl.length; i++) {
            if (pnl[i].style.maxHeight) {
            } else {
                acc[i].classList.toggle("active");
                pnl[i].style.maxHeight = pnl[i].scrollHeight + "px";
            }
        }
    });
});