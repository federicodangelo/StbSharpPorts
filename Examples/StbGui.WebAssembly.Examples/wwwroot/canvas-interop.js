let canvas;
let ctx;
let next_canvas_id = 1;

const canvases = {};

const canvases_tinted = {};

const RENDER = true;

function buildFillStyle(argb) {
    return "#" + argb.toString(16).padStart(8, '0');
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

export function setTitle(title) {
    document.title = title;
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

export function clear(c) {
    if (!RENDER) return;
    ctx.fillStyle = buildFillStyle(c);
    ctx.fillRect(0, 0, canvas.width, canvas.height);
}

function getAlpha(color) {
    const a = color & 0xFF;
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

export function setCanvasPixels(id, width, height, pixels) {
    const canvas = getCanvas(id);
    const canvasCtx = canvas.getContext("2d");
    const imageData = canvasCtx.createImageData(width, height);
    pixels.copyTo(new Uint8Array(imageData.data.buffer));
    canvasCtx.putImageData(imageData, 0, 0);
}

export function drawCanvasRectangleBatch(id, batch_memory_view) {
    if (!RENDER) return;
    const batch = batch_memory_view.slice();

    for (let i = 0; i < batch.length; i += 9) {
        const fromX = batch[i + 0];
        const fromY = batch[i + 1];
        const fromWidth = batch[i + 2];
        const fromHeight = batch[i + 3];
        const toX = batch[i + 4];
        const toY = batch[i + 5];
        const toWidth = batch[i + 6];
        const toHeight = batch[i + 7];
        const color = batch[i + 8];

        drawCanvasRectangle(id, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight, color);
    }
}

let last_copy_canvas_pixels_id = 0;
let last_copy_canvas_pixels_color = 0;

let last_copy_canvas_pixels_canvas_tinted;
let last_copy_canvas_pixels_fill_style;

export function drawCanvasRectangle(id, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight, color) {
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

const DRAW_BATCH_BORDER = 1;
const DRAW_BATCH_RECTANGLE = 2;
const DRAW_BATCH_CANVAS_RECTANGLE = 3;
const DRAW_BATCH_CANVAS_RECTANGLE_BATCH = 4;
const DRAW_BATCH_CANVAS_PUSH_CLIP_RECT = 5;
const DRAW_BATCH_CANVAS_POP_CLIP_RECT = 6;

export function drawBatch(batch_memory_view) {
    const batch = batch_memory_view.slice();

    let i = 0;

    while (i < batch.length) {
        const type = batch[i++];

        switch (type) {
            case DRAW_BATCH_BORDER:
                drawBorder(batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++]);
                break;
            case DRAW_BATCH_RECTANGLE:
                drawRectangle(batch[i++], batch[i++], batch[i++], batch[i++], batch[i++]);
                break;
            case DRAW_BATCH_CANVAS_RECTANGLE:
                drawCanvasRectangle(batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++]);
                break;
            case DRAW_BATCH_CANVAS_RECTANGLE_BATCH: {
                const id = batch[i++];
                const batch_count = batch[i++];
                for (let j = 0; j < batch_count; j++) {
                    drawCanvasRectangle(id, batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++]);
                }
                break;
            }
            case DRAW_BATCH_CANVAS_PUSH_CLIP_RECT:
                pushClip(batch[i++], batch[i++], batch[i++], batch[i++]);
                break;
            case DRAW_BATCH_CANVAS_POP_CLIP_RECT:
                popClip();
                break;
        }
    }
}

export function presentFrame() {
    // Nothing to do
}

let clipboard_text = "";

export function copyToClipboard(text) {
    // TODO: Browser clipboard API is async...
    clipboard_text = text;
}

export function getFromClipboard() {
    // TODO: Browser clipboard API is async...
    return clipboard_text ?? "";
}
