/* Modals
 * Note: Requires JQuery to work properly, elementId should use # before id, e.g. #taskModal
 */
function ShowModal(elementId) {
    $(elementId).modal('show');
}

function HideModal(elementId) {
    $(elementId).modal('hide');
}

const toastTrigger = document.getElementById('liveToastBtn')

if (toastTrigger) {
    toastTrigger.addEventListener('click', () => {
        
    })
}

function ShowNewToast(elementId) {
    const toastToShow = document.getElementById(elementId)
    const toast = new bootstrap.Toast(toastToShow)

    toast.show()
}

function HideToast(elementId) {
    const toastToShow = document.getElementById(elementId)
    const toast = new bootstrap.Toast(toastToShow)

    toast.hide()
}

function ScrollToElement(elementId) {
    document.getElementById(elementId).scrollIntoView();
}

function UnfocusElemnent(elementId) {
    document.getElementById(elementId).blur();
}

function SetToggleButtonInactive(elementId) {
    let element = document.getElementById(elementId);
    element.classList.remove("active");
    element.setAttribute("aria-pressed", "false");
}

function SetToggleButtonActive(elementId) {
    let element = document.getElementById(elementId);
    element.classList.add("active");
    element.setAttribute("aria-pressed", "true");
}

function IntializeTooltips() {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))
}