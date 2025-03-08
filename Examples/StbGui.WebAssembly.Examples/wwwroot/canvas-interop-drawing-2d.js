let canvas;
let ctx;
let next_canvas_id = 1;

const canvases = {};

const canvases_tinted = {};

const RENDER = true;

function buildFillStyle(argb) {
    return `#${getRed(argb).toString(16).padStart(2, '0')}${getGreen(argb).toString(16).padStart(2, '0')}${getBlue(argb).toString(16).padStart(2, '0')}${getAlpha(argb).toString(16).padStart(2, '0')}`;
}

function getAlpha(color) {
    return (color >> 24) & 0xFF;
}

function getRed(color) {
    return (color >> 0) & 0xFF;
}

function getGreen(color) {
    return (color >> 8) & 0xFF;
}

function getBlue(color) {
    return (color >> 16) & 0xFF;
}

export function getRenderBackend() {
    return "canvas2d";
}

export function initDrawing() {
    canvas = document.getElementById("myCanvas");
    //console.log(canvas);
    ctx = canvas.getContext("2d");
    ctx.imageSmoothingEnabled = false;

    return canvas;
}

export function getWidth() {
    return canvas.width;
}

export function getHeight() {
    return canvas.height;
}

export function clear(c) {
    if (!RENDER) return;
    ctx.fillStyle = buildFillStyle(c);
    ctx.fillRect(0, 0, canvas.width, canvas.height);
}

export function drawBorder(color_fill, color_border, x, y, w, h, border_size) {
    if (!RENDER) return;

    if (getAlpha(color_fill) != 0) {
        ctx.fillStyle = buildFillStyle(color_fill);
        ctx.fillRect(x, y, w, h);
    }

    if (getAlpha(color_border) != 0) {
        ctx.strokeStyle = buildFillStyle(color_border);
        ctx.lineWidth = border_size;
        ctx.beginPath();
        ctx.strokeRect(x + border_size / 2, y + border_size / 2, w - 1, h - 1);
    }
}

export function drawRectangle(color, x, y, w, h) {
    if (!RENDER) return;

    if (getAlpha(color) != 0) {
        ctx.fillStyle = buildFillStyle(color);
        ctx.fillRect(x, y, w, h);
    }
}

export function pushClip(x, y, w, h) {
    if (!RENDER) return;

    ctx.save();
    ctx.beginPath();
    ctx.rect(x, y, w, h);
    ctx.clip();
}

export function popClip() {
    if (!RENDER) return;

    ctx.restore();
}

export function createTexture(w, h, pixels, bytes_per_pixel) {
    let id = next_canvas_id++;
    let source = pixels.slice();

    const new_canvas = document.createElement("canvas");
    new_canvas.width = w;
    new_canvas.height = h;

    const canvasCtx = new_canvas.getContext("2d");
    const imageData = canvasCtx.createImageData(w, h);

    let target = imageData.data;
    let target_idx = 0;

    if (bytes_per_pixel == 4) {
        for (let i = 0; i < source.length; i += 4) {
            target[target_idx + 0] = source[i + 0];
            target[target_idx + 1] = source[i + 1];
            target[target_idx + 2] = source[i + 2];
            target[target_idx + 3] = source[i + 3];
            target_idx += 4;
        }
    } else if (bytes_per_pixel == 3) {
        for (let i = 0; i < source.length; i += 3) {
            target[target_idx + 0] = source[i + 0];
            target[target_idx + 1] = source[i + 1];
            target[target_idx + 2] = source[i + 2];
            target[target_idx + 3] = 255;
            target_idx += 4;
        }
    }

    canvasCtx.putImageData(imageData, 0, 0);

    canvases[id] = new_canvas;

    return id;
}

export function destroyTexture(id) {
    delete canvases[id];
}

function getCanvas(id) {
    return canvases[id];
}

function getCanvasTinted(id, color) {
    if (color === 0xFFFFFFFF) {
        return getCanvas(id);
    }

    const style = buildFillStyle(color);
    const tinted_id = id + "-" + style;
    let canvas_tinted = canvases_tinted[tinted_id];

    if (!canvas_tinted) {
        const canvas = getCanvas(id);

        canvas_tinted = document.createElement("canvas");
        canvas_tinted.width = canvas.width;
        canvas_tinted.height = canvas.height;

        const ctx_tinted = canvas_tinted.getContext("2d");
        ctx_tinted.fillStyle = style;
        ctx_tinted.fillRect(0, 0, canvas.width, canvas.height);
        ctx_tinted.globalCompositeOperation = "destination-atop";
        ctx_tinted.drawImage(canvas, 0, 0);

        canvases_tinted[tinted_id] = canvas_tinted;
    }

    return canvas_tinted;
}

let last_copy_canvas_pixels_id = 0;
let last_copy_canvas_pixels_color = 0;

let last_copy_canvas_pixels_canvas_tinted;
let last_copy_canvas_pixels_fill_style;

export function drawTextureRectangle(id, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight, color) {
    if (!RENDER) return;

    if (last_copy_canvas_pixels_id !== id || last_copy_canvas_pixels_color !== color) {
        last_copy_canvas_pixels_color = color;
        last_copy_canvas_pixels_id = id;

        if (id != 0) {
            last_copy_canvas_pixels_canvas_tinted = getCanvasTinted(id, color);
        } else {
            last_copy_canvas_pixels_fill_style = buildFillStyle(color);
        }
    }

    if (id === 0) {
        ctx.fillStyle = last_copy_canvas_pixels_fill_style;
        ctx.fillRect(toX, toY, toWidth, toHeight);
    } else {
        ctx.drawImage(last_copy_canvas_pixels_canvas_tinted, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight);
    }
}

export function presentFrame() {
    // Nothing to do
}
