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

function DisableBackgroundScrolling() {
    $("body").css("overflow", "hidden");
}

function EnableBackgroundScrolling() {
    $("body").css("overflow", "");
}