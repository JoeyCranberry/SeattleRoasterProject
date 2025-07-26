function InitializeFlowbite() {
    initFlowbite();
}

function MakeModalDraggable(modalSelector) {
    console.log(modalSelector);
    $(modalSelector).draggable({
        handle: ".modal-header",
        containment: "window"
    });
}