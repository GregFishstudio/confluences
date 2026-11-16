function allowDrop(event) {
    event.preventDefault();
}

function drag(event) {
    event.dataTransfer.setData("text", event.target.id);
}

function drop(event) {
    event.preventDefault();
    var data = event.dataTransfer.getData("text");
    var draggedElement = document.getElementById(data);
    var dropTarget = event.target;

    // Assurez-vous que l'élément de destination est le conteneur d'images
    if (dropTarget.classList.contains("image-container")) {
        dropTarget.appendChild(draggedElement);
    }
}
