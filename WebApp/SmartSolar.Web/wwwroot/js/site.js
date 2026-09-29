document.querySelectorAll("#appNav .side-link").forEach(function (link) {
  link.addEventListener("click", function () {
    if (window.innerWidth >= 992) return;
    var nav = document.getElementById("appNav");
    if (!nav || !window.bootstrap) return;
    var instance = bootstrap.Offcanvas.getInstance(nav);
    if (instance) instance.hide();
  });
});

document.querySelectorAll("[data-toggle-password]").forEach(function (button) {
  button.addEventListener("click", function () {
    var input = document.querySelector(button.getAttribute("data-toggle-password"));
    if (!input) return;
    var showing = input.getAttribute("type") === "text";
    input.setAttribute("type", showing ? "password" : "text");
    button.textContent = showing ? "Show" : "Hide";
  });
});
