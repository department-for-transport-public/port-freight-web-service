const colsToSearch = [0, 1, 5];
const filterArrSessionStorage = "filterArr";
var isInCell = false;
var filterArr = [];
var table;

var tr;

function ReadyPage(htmlTable) {
    table = htmlTable;
    tr = table.getElementsByTagName("tr");

    showHideClearBtn("none");

    filterArr = JSON.parse(sessionStorage.getItem(filterArrSessionStorage));

    if (filterArr !== null) {
        filterTable();
    }
}

document.querySelector("#filterInput").addEventListener("keydown", event => {
    if(event.key !== "Enter") return;
    filterTable();
    event.preventDefault();
});

function filterTable() {
    let input = document.getElementById("filterInput");
    let filter = input.value.toUpperCase();

    setFilterArr(filter);
        
    for (var i = 0; i < filterArr.length; i++) {
        filter = filterArr[i];

        for (var rowNo = 1; rowNo < tr.length; rowNo++) {
            for (var x = 0; x <= colsToSearch.length - 1; x++) {
                var val = colsToSearch[x];

                let td = tr[rowNo].getElementsByTagName("td")[val];

                if (td) {
                    let txtValue = td.textContent || td.innerText;

                    if (txtValue.toUpperCase().localeCompare(filter.toUpperCase().trim()) === 0) {
                        isInCell = true;
                    }
                }
            }

            if (!isInCell) {
                tr[rowNo].style.display = "none";
                tr[rowNo].style.transitionDuration = 400, 0;
                tr[rowNo].style.transitionDelay = 0, 800;
            }

            isInCell = false;
        }
    }

    input.value = "";
    displayCurrentFilters(true);

}

function clearSessionStorage() {
    sessionStorage.removeItem(filterArrSessionStorage);
}

function displayCurrentFilters(display) {
    var ul = document.getElementById("currentFilters");

    var p = document.getElementById("filterHeading");

    ul.innerHTML = "";

    if (display) {
        p.innerText = "Currently filtering by";

        for (var i = 0; i < filterArr.length; i++) {
            var filterListItem = document.createElement("li");
            var filterListVal = document.createTextNode(filterArr[i]);

            filterListItem.appendChild(filterListVal);

            ul.appendChild(filterListItem);
        }

        showHideClearBtn("");
    }
    else {
        ul.innerHTML = "";
        p.innerText = "";
        showHideClearBtn("none");
    }
}

function resetTable() {
    ShowAllRows();
    clearSessionStorage();
    displayCurrentFilters(false);    
}

function ShowAllRows() {
    for (var i = 1; i < tr.length; i++) {
        tr[i].style.display = "";
        tr[i].style.transitionDuration = 400, 0;
        tr[i].style.transitionDelay = 0, 800;
    }
}

function setFilterArr(filter) {
    filterArr = JSON.parse(sessionStorage.getItem(filterArrSessionStorage));

    if (filterArr !== null) {
        if (filter !== "") {
            if (filterArr.indexOf(filter) === -1) {
                filterArr.push(filter);

                sessionStorage.setItem(filterArrSessionStorage, JSON.stringify(filterArr));
            }
        }
    }
    else {
        filterArr = [];

        if (filter !== "") {
            filterArr.push(filter);

            sessionStorage.setItem(filterArrSessionStorage, JSON.stringify(filterArr));
        }
    }
}

function showHideClearBtn(display) {
    document.getElementById("clearBtn").style.display = display;
}