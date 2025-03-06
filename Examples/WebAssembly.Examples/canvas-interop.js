let canvas;
let ctx;
let next_canvas_id = 1;

const canvases = {};

const canvases_tinted = {};

function buildFillStyle(r, g, b, a) {
    return `rgba(${r}, ${g}, ${b}, ${a / 255})`;
}

const input_events = [];

export function init() {
    canvas = document.getElementById("myCanvas");
    //console.log(canvas);
    ctx = canvas.getContext("2d");
    ctx.imageSmoothingEnabled = false;

    canvas.addEventListener('mousedown', (evt) => {
        input_events.push({ type: 1, x: evt.offsetX, y: evt.offsetY, button: evt.button });
    });

    canvas.addEventListener('mouseup', (evt) => {
        input_events.push({ type: 2, x: evt.offsetX, y: evt.offsetY, button: evt.button });
    });

    canvas.addEventListener('mousemove', (evt) => {
        input_events.push({ type: 3, x: evt.offsetX, y: evt.offsetY });
    });

    canvas.addEventListener('wheel', (evt) => {
        input_events.push({ type: 4, x: evt.offsetX, y: evt.offsetY, dx: Math.round(evt.deltaX), dy: Math.round(evt.deltaY) });
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

export function setCanvasPixels(id, width, height, pixels) {
    const canvas = canvases[id];
    const canvasCtx = canvas.getContext("2d");

    const imageData = canvasCtx.createImageData(width, height);
    imageData.data.set(pixels);
    canvasCtx.putImageData(imageData, 0, 0);
}

export function copyCanvasPixels(id, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight, r, g, b, a) {
    var style = buildFillStyle(r, g, b, a);
    if (id === 0) {
        ctx.fillStyle = style;
        ctx.fillRect(toX, toY, toWidth, toHeight);        
    } else {
        const tinted_id = id + "-" + style;
        let canvas_tinted = canvases_tinted[tinted_id];

        if (!canvas_tinted) {
            const canvas = canvases[id];

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

        ctx.drawImage(canvas_tinted, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight);
    }
}