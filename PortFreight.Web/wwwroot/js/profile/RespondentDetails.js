$(document).ready(function () {
    AssociatedPorts();
    accessibleAutocomplete({
        element: document.querySelector('#Input_SenderPortLocode-container'),
        id: 'Input_SenderPortLocode',
        source: ukPorts,
        autoselect: true,
        name: "locode"
    });
});
function AssociatedPorts() {
    if (chkPort) {
        $('#associatedPorts').show();
        $('#noAssociatedPorts').hide();
        }
    else {
        $('#associatedPorts').hide();
        $('#noAssociatedPorts').show();
    }
}
