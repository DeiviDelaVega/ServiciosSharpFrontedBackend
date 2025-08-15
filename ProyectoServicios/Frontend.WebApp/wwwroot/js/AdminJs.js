let pos = 1;

function rotacion() {
    pos++;
    if (pos > 4) pos = 1;
    document.getElementById("banner").src = "/imagenes/banner" + pos + ".jpg";
    document.getElementById("banner").style.opacity = 1;
    document.getElementById("banner").style.transition = "2s";
    setTimeout(opacidad, 1000);
}

function opacidad() {
    document.getElementById("banner").style.opacity = 0.6;
    document.getElementById("banner").style.transition = "2s";
    setTimeout(rotacion, 1000);
}

setTimeout(opacidad, 1000);