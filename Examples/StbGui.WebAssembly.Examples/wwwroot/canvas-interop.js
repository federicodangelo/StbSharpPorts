import * as input from "./canvas-interop-input.js";
import * as drawing from "./canvas-interop-drawing-webgl.js";
//import * as drawing from "./canvas-interop-drawing-2d.js";

let canvas;

export function init() {
    canvas = drawing.initDrawing();
    input.initInputEvents(canvas);
}

export function setTitle(title) {
    document.title = title;
}

export function setCursor(cursor) {
    canvas.style.cursor = cursor;
}

export const getInputEventsCount = input.getInputEventsCount;
export const getInputEvent = input.getInputEvent;
export const getInputEventProperty = input.getInputEventProperty;
export const getInputEventPropertyString = input.getInputEventPropertyString;
export const clearInputEvents = input.clearInputEvents;

export const getRenderBackend = drawing.getRenderBackend;

export const getWidth = drawing.getWidth;
export const getHeight = drawing.getHeight;

export const clear = drawing.clear;

export const drawBorder = drawing.drawBorder;
export const drawRectangle = drawing.drawRectangle;

export const createTexture = drawing.createTexture;
export const destroyTexture = drawing.destroyTexture;
export const drawTextureRectangle = drawing.drawTextureRectangle;

export const pushClip = drawing.pushClip;
export const popClip = drawing.popClip;

export const presentFrame = drawing.presentFrame;


export function drawTextureRectangleBatch(id, batch_memory_view) {
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

        drawTextureRectangle(id, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight, color);
    }
}

const DRAW_BATCH_BORDER = 1;
const DRAW_BATCH_RECTANGLE = 2;
const DRAW_BATCH_TEXTURE_RECTANGLE = 3;
const DRAW_BATCH_TEXTURE_RECTANGLE_BATCH = 4;
const DRAW_BATCH_PUSH_CLIP_RECT = 5;
const DRAW_BATCH_POP_CLIP_RECT = 6;

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
            case DRAW_BATCH_TEXTURE_RECTANGLE:
                drawTextureRectangle(batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++]);
                break;
            case DRAW_BATCH_TEXTURE_RECTANGLE_BATCH: {
                const id = batch[i++];
                const batch_count = batch[i++];
                for (let j = 0; j < batch_count; j++) {
                    drawTextureRectangle(id, batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++], batch[i++]);
                }
                break;
            }
            case DRAW_BATCH_PUSH_CLIP_RECT:
                pushClip(batch[i++], batch[i++], batch[i++], batch[i++]);
                break;
            case DRAW_BATCH_POP_CLIP_RECT:
                popClip();
                break;
        }
    }
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

export function runBenchmarkJavascript() {
    const start = performance.now();

    var sum = 0;

    for (let i = 0; i < 100_000_000; i++) {
        sum += Math.sqrt(i);
    }

    const end = performance.now();

    const result = `JAVASCRIPT - Sum: ${sum} Time: ${end - start}ms`;

    return result;
}