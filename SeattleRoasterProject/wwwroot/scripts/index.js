/* Modals
 * Note: Requires JQuery to work properly, elementId should use # before id, e.g. #taskModal
 */
function ShowModal(elementId) {
    $(elementId).modal('show');
}

function HideModal(elementId) {
    $(elementId).modal('hide');
}