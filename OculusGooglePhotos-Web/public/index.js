const STEP_ELEMENTS = [
    document.getElementById("step-0"),
    document.getElementById("step-1"),
]
function displayStep(stepNum) {
    STEP_ELEMENTS.forEach((elem, i) => {
        if (i === stepNum) {
            elem.style.display = "block";
        } else {
            elem.style.display = "none";
        }
    })
}

displayStep(0);
