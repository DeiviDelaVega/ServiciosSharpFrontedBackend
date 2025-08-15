let pos = 1;

function rotacion() {
    pos++;
    if (pos > 4) pos = 1;
    document.getElementById("baner").src = "/imagenes/baner" + pos + ".jpg";
    document.getElementById("baner").style.opacity = 1;
    document.getElementById("baner").style.transition = "2s";
    setTimeout(opacidad, 1000);
}

function opacidad() {
    document.getElementById("baner").style.opacity = 0.6;
    document.getElementById("baner").style.transition = "2s";
    setTimeout(rotacion, 1000);
}

setTimeout(opacidad, 1000);