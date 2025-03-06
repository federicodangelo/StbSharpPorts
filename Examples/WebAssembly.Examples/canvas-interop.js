let canvas;
let ctx;
let next_canvas_id = 1;

const canvases = {};

const canvases_tinted = {};

function buildFillStyle(r, g, b, a) {
    return `rgba(${r}, ${g}, ${b}, ${a / 255})`;
}

const input_events = [];

const MOUSE_DOWN_EVENT_TYPE = 1;
const MOUSE_UP_EVENT_TYPE = 2;
const MOUSE_MOVE_EVENT_TYPE = 3;
const MOUSE_WHEEL_EVENT_TYPE = 4;

const KEY_DOWN_EVENT_TYPE = 10;
const KEY_UP_EVENT_TYPE = 11;

export function init() {
    canvas = document.getElementById("myCanvas");
    //console.log(canvas);
    ctx = canvas.getContext("2d");
    ctx.imageSmoothingEnabled = false;

    canvas.addEventListener('mousedown', (evt) => {
        input_events.push({ type: MOUSE_DOWN_EVENT_TYPE, x: evt.offsetX, y: evt.offsetY, button: evt.button });
    });

    canvas.addEventListener('mouseup', (evt) => {
        input_events.push({ type: MOUSE_UP_EVENT_TYPE, x: evt.offsetX, y: evt.offsetY, button: evt.button });
    });

    canvas.addEventListener('mousemove', (evt) => {
        input_events.push({ type: MOUSE_MOVE_EVENT_TYPE, x: evt.offsetX, y: evt.offsetY });
    });

    canvas.addEventListener('wheel', (evt) => {
        input_events.push({ type: MOUSE_WHEEL_EVENT_TYPE, x: evt.offsetX, y: evt.offsetY, dx: Math.round(evt.deltaX), dy: Math.round(evt.deltaY) });
    });

    document.addEventListener('keydown', (evt) => {
        input_events.push({ type: KEY_DOWN_EVENT_TYPE, alt: evt.altKey ? 1 : 0, ctrl: evt.ctrlKey ? 1 : 0, meta: evt.metaKey ? 1 : 0, shift: evt.shiftKey ? 1 : 0, key: evt.key, code: evt.code });
    });

    document.addEventListener('keyup', (evt) => {
        input_events.push({ type: KEY_UP_EVENT_TYPE, alt: evt.altKey ? 1 : 0, ctrl: evt.ctrlKey ? 1 : 0, meta: evt.metaKey ? 1 : 0, shift: evt.shiftKey ? 1 : 0, key: evt.key, code: evt.code });
    });
}

export function setCursor(cursor) {
    canvas.style.cursor = cursor;
}

export function getWidth() {
    return canvas.width;
}

export function getHeight() {
    return canvas.height;
}

export function getEventsCount() {
    return input_events.length;
}

export function getEvent(index) {
    return input_events[index].type;
}

export function getEventProperty(index, property) {
    return input_events[index][property];
}

export function getEventPropertyString(index, property, buffer) {
    const str = input_events[index][property];
    let len = 0;

    const tmpBuffer = new Int32Array(32);

    for (const codePoint of str) {
        tmpBuffer[len] = codePoint.codePointAt(0);
        len++;
    }

    buffer.set(tmpBuffer.slice(0, len));

    return len;
}

export function clearEvents() {
    input_events.length = 0;
}

export function clear(r, g, b, a) {
    ctx.fillStyle = `rgba(${r}, ${g}, ${b}, ${a / 255})`;
    ctx.fillRect(0, 0, canvas.width, canvas.height);
}

export function drawBorder(r1, g1, b1, a1, r2, g2, b2, a2, x, y, w, h, border_size) {
    if (a1 != 0) {
        ctx.fillStyle = buildFillStyle(r1, g1, b1, a1);
        ctx.fillRect(x, y, w, h);
    }

    if (a2 != 0) {
        ctx.strokeStyle = buildFillStyle(r2, g2, b2, a2);
        ctx.lineWidth = border_size;
        ctx.beginPath();
        ctx.strokeRect(x + border_size / 2, y + border_size / 2, w - 1, h - 1);
    }
}

export function drawRectangle(r1, g1, b1, a1, x, y, w, h) {
    if (a1 != 0) {
        ctx.fillStyle = buildFillStyle(r1, g1, b1, a1);
        ctx.fillRect(x, y, w, h);
    }
}

export function pushClip(x, y, w, h) {
    ctx.save();
    ctx.beginPath();
    ctx.rect(x, y, w, h);
    ctx.clip();
}

export function popClip() {
    ctx.restore();
}

export function createCanvas(w, h) {
    var id = next_canvas_id++;

    const new_canvas = document.createElement("canvas");
    new_canvas.width = w;
    new_canvas.height = h;

    canvases[id] = new_canvas;

    return id;
}

export function destroyCanvas(id) {
    delete canvases[id];
}

function getCanvas(id) {
    return canvases[id];
}

function getCanvasTinted(id, r, g, b, a) {
    if (r === 255 && g === 255 && b === 255 && a === 255) {
        return getCanvas(id);
    }

    const style = buildFillStyle(r, g, b, a);
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

export function setCanvasPixels(id, width, height, pixels) {
    const canvas = getCanvas(id);
    const canvasCtx = canvas.getContext("2d");
    const imageData = canvasCtx.createImageData(width, height);
    pixels.copyTo(new Uint8Array(imageData.data.buffer));
    canvasCtx.putImageData(imageData, 0, 0);
}

export function copyCanvasPixelsBatch(id, batch_memory_view) {
    const batch = batch_memory_view.slice();

    for (let i = 0; i < batch.length; i += 12) {
        const fromX = batch[i + 0];
        const fromY = batch[i + 1];
        const fromWidth = batch[i + 2];
        const fromHeight = batch[i + 3];
        const toX = batch[i + 4];
        const toY = batch[i + 5];
        const toWidth = batch[i + 6];
        const toHeight = batch[i + 7];
        const r = batch[i + 8];
        const g = batch[i + 9];
        const b = batch[i + 10];
        const a = batch[i + 11];

        copyCanvasPixels(id, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight, r, g, b, a);
    }
}

let last_copy_canvas_pixels_id = 0;
let last_copy_canvas_pixels_r = 0;
let last_copy_canvas_pixels_g = 0;
let last_copy_canvas_pixels_b = 0;
let last_copy_canvas_pixels_a = 0;

let last_copy_canvas_pixels_canvas_tinted;
let last_copy_canvas_pixels_fill_style;

export function copyCanvasPixels(id, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight, r, g, b, a) {
    if (last_copy_canvas_pixels_id !== id || last_copy_canvas_pixels_r !== r || last_copy_canvas_pixels_g !== g || last_copy_canvas_pixels_b !== b || last_copy_canvas_pixels_a !== a) {
        last_copy_canvas_pixels_r = r;
        last_copy_canvas_pixels_g = g;
        last_copy_canvas_pixels_b = b;
        last_copy_canvas_pixels_a = a;
        last_copy_canvas_pixels_id = id;

        if (id != 0) {
            last_copy_canvas_pixels_canvas_tinted = getCanvasTinted(id, r, g, b, a);
        } else {
            last_copy_canvas_pixels_fill_style = buildFillStyle(r, g, b, a);
        }
    }

    if (id === 0) {
        ctx.fillStyle = last_copy_canvas_pixels_fill_style;
        ctx.fillRect(toX, toY, toWidth, toHeight);
    } else {
        ctx.drawImage(last_copy_canvas_pixels_canvas_tinted, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight);
    }
}