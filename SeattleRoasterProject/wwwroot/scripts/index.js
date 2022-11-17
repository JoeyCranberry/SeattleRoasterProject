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
