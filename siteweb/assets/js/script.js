let profile = document.querySelector(".profile");
let popup_container = document.querySelector(".popup_container");
profile.addEventListener("click", function() {
    popup_container.classList.toggle("active")
})
popup_container.addEventListener("click", function() {
    popup_container.classList.toggle("active")
})
