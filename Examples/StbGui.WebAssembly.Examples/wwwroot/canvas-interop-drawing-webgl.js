import { initBuffers, initShaders } from "./canvas-webgl-shaders.js";

let canvas;
let gl;
let programInfo;
let buffers;
let next_canvas_id = 1;
let projectionMatrix;

const canvases = {};

const RENDER = true;
const WIP = true;

function getAlpha(color) {
    return color & 0xFF;
}

function getRed(color) {
    return (color >> 24) & 0xFF;
}

function getGreen(color) {
    return (color >> 16) & 0xFF;
}

function getBlue(color) {
    return (color >> 8) & 0xFF;
}


export function initDrawing() {
    canvas = document.getElementById("myCanvas");
    //console.log(canvas);
    gl = canvas.getContext("webgl");

    // Disable depth testing (we are going to draw 2D)
    gl.disable(gl.DEPTH_TEST);

    programInfo = initShaders(gl);
    buffers = initBuffers(gl);

    console.log({ programInfo, buffers });

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
    gl.clearColor(getRed(c) / 255, getGreen(c) / 255, getBlue(c) / 255, getAlpha(c) / 255);
    // Clear the color buffer with specified clear color
    gl.clear(gl.COLOR_BUFFER_BIT);
    gl.colorMask(true, true, true, true)

    // Setup viewport and projection matrix
    const width = gl.canvas.width;
    const height = gl.canvas.height;
    gl.viewport(0, 0, width, height);
    projectionMatrix = buildOrthographicMatrix(0, width, height, 0, 0, 1);

    // Setup blending
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);

    // Setup shader
    gl.useProgram(programInfo.program);
    gl.uniformMatrix4fv(
        programInfo.uniformLocations.projectionMatrix,
        false,
        projectionMatrix,
    );
}

export function buildOrthographicMatrix(left, right, bottom, top, near, far) {
    var out = new Float32Array(16);
    var lr = 1 / (left - right);
    var bt = 1 / (bottom - top);
    var nf = 1 / (near - far);
    out[0] = -2 * lr;
    out[1] = 0;
    out[2] = 0;
    out[3] = 0;
    out[4] = 0;
    out[5] = -2 * bt;
    out[6] = 0;
    out[7] = 0;
    out[8] = 0;
    out[9] = 0;
    out[10] = 2 * nf;
    out[11] = 0;
    out[12] = (left + right) * lr;
    out[13] = (top + bottom) * bt;
    out[14] = (far + near) * nf;
    out[15] = 1;
    return out;
}

export function drawBorder(color_fill, color_border, x, y, w, h, border_size) {
    if (!RENDER) return;

    if (getAlpha(color_fill) != 0) {
        drawRectangleWebGL(x, y, w, h, color_fill);
    }

    if (getAlpha(color_border) != 0) {
        drawRectangleWebGL(x, y, w, border_size, color_border);
        drawRectangleWebGL(x, y + h - border_size, w, border_size, color_border);
        drawRectangleWebGL(x, y, border_size, h, color_border);
        drawRectangleWebGL(x + w - border_size, y, border_size, h, color_border);
    }
}

export function drawRectangle(color, x, y, w, h) {
    if (!RENDER) return;

    if (getAlpha(color) != 0) {
        drawRectangleWebGL(x, y, w, h, color);
    }
}

function buildClipRect(x, y, w, h) {
    return { x, y, w, h };
}

function clamp(value, min, max) {
    if (value < min) {
        return min;
    } else if (value > max) {
        return max;
    } else {
        return value;
    }
}

function clampClipRect(rect, prev_clip) {
    let x = clamp(rect.x, prev_clip.x, prev_clip.x + prev_clip.w);
    let y = clamp(rect.y, prev_clip.y, prev_clip.y + prev_clip.h);
    let x2 = clamp(rect.x + rect.w, prev_clip.x, prev_clip.x + prev_clip.w);
    let y2 = clamp(rect.y + rect.h, prev_clip.y, prev_clip.y + prev_clip.h);
    return { x: x, y: y, w: x2 - x, h: y2 - y };
}

let clip_rects = [];

function setScissor(rect) {
    gl.scissor(rect.x, canvas.height - (rect.y + rect.h), rect.w, rect.h);
}

export function pushClip(x, y, w, h) {
    if (!RENDER) return;

    var rect = buildClipRect(x, y, w, h);

    var prev_clip = clip_rects.length > 0 ? clip_rects[clip_rects.length - 1] : undefined;

    var rect_clipped = prev_clip ? clampClipRect(rect, prev_clip) : rect;

    clip_rects.push(rect_clipped);

    if (clip_rects.length == 1) {
        gl.enable(gl.SCISSOR_TEST);
    }

    setScissor(rect_clipped);
}

export function popClip() {
    if (!RENDER) return;

    clip_rects.pop();

    if (clip_rects.length > 0) {
        var rect_clipped = clip_rects[clip_rects.length - 1];
        setScissor(rect_clipped);
    } else {
        gl.disable(gl.SCISSOR_TEST);
    }
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
    if (WIP) return;
    delete canvases[id];
}

function getCanvas(id) {
    return canvases[id];
}

export function setCanvasPixels(id, width, height, pixels) {
    if (WIP) return;

    const canvas = getCanvas(id);
    const canvasCtx = canvas.getContext("2d");
    const imageData = canvasCtx.createImageData(width, height);
    pixels.copyTo(new Uint8Array(imageData.data.buffer));
    canvasCtx.putImageData(imageData, 0, 0);
}



let last_copy_canvas_pixels_id = 0;
let last_copy_canvas_pixels_color = 0;

let last_copy_canvas_pixels_canvas_tinted;
let last_copy_canvas_pixels_fill_style;

export function drawCanvasRectangle(id, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight, color) {
    if (!RENDER) return;
    if (WIP) return;

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

}

function drawRectangleWebGL(x, y, w, h, color) {

    setPositionBuffer([
        x, y,
        x + w, y,
        x + w, y + h,
        x + w, y + h,
        x, y + h,
        x, y,
    ]);

    const r = getRed(color) / 255;
    const g = getGreen(color) / 255;
    const b = getBlue(color) / 255;
    const a = getAlpha(color) / 255;

    setColorsBuffer([
        r, g, b, a,
        r, g, b, a,
        r, g, b, a,
        r, g, b, a,
        r, g, b, a,
        r, g, b, a,
    ]);

    setPositionAttribute();
    setColorAttribute();

    const offset = 0;
    const vertexCount = 6;
    gl.drawArrays(gl.TRIANGLES, offset, vertexCount);
}

function setPositionBuffer(positions) {
    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.position);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(positions), gl.DYNAMIC_DRAW);
}

function setColorsBuffer(colors) {
    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.color);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(colors), gl.DYNAMIC_DRAW);
}

function setPositionAttribute() {
    const numComponents = 2;
    const type = gl.FLOAT;
    const normalize = false;
    const stride = 0;
    const offset = 0;
    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.position);

    gl.vertexAttribPointer(
        programInfo.attribLocations.vertexPosition,
        numComponents,
        type,
        normalize,
        stride,
        offset,
    );
    gl.enableVertexAttribArray(programInfo.attribLocations.vertexPosition);
}

function setColorAttribute() {
    const numComponents = 4;
    const type = gl.FLOAT;
    const normalize = false;
    const stride = 0;
    const offset = 0;
    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.color);

    gl.vertexAttribPointer(
        programInfo.attribLocations.vertexColor,
        numComponents,
        type,
        normalize,
        stride,
        offset,
    );
    gl.enableVertexAttribArray(programInfo.attribLocations.vertexColor);
}
