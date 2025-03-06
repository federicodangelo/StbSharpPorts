
let canvas;
let ctx;

const half_pixel = 0.5;

export function init() {
    canvas = document.getElementById("myCanvas");
    console.log(canvas);
    ctx = canvas.getContext("2d");
    ctx.imageSmoothingEnabled = false;
}

export function getWidth() {
    return canvas.width;
}

export function getHeight() {
    return canvas.height;
}

export function clear(r, g, b, a) {
    ctx.fillStyle = `rgba(${r}, ${g}, ${b}, ${a / 255})`;
    ctx.fillRect(0, 0, canvas.width, canvas.height);
}

export function drawBorder(r1, g1, b1, a1, r2, g2, b2, a2, x, y, w, h, border_size) {
    
    if (a1 != 0) {
        ctx.fillStyle = `rgba(${r1}, ${g1}, ${b1}, ${a1 / 255})`;
        ctx.fillRect(x, y, w, h);
    }

    if (a2 != 0) {
        ctx.strokeStyle = `rgba(${r2}, ${g2}, ${b2}, ${a2 / 255})`;
        ctx.lineWidth = border_size;
        ctx.strokeRect(x + border_size / 2, y + border_size / 2, w - 1, h - 1);
    }
}

export function drawRectangle(r1, g1, b1, a1, x, y, w, h) {
    if (a1 != 0) {
        ctx.fillStyle = `rgba(${r1}, ${g1}, ${b1}, ${a1 / 255})`;
        ctx.fillRect(x, y, w, h);
    }
}

export function pushClip(x, y, w, h) {
    ctx.save();
    ctx.rect(x, y, w, h);
    ctx.clip();
}

export function popClip() {
    ctx.restore();
}