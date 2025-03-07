import { initBuffers, initColorShaders, initTextureColorShaders } from "./canvas-webgl-shaders.js";

let canvas;
let gl;

let projectionMatrix;

let next_texture_id = 1;

let colorProgramInfo;
let textureColorProgramInfo;
let buffers;

const textures = {};

const RENDER = true;

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
    gl = canvas.getContext("webgl", {});

    // Disable depth testing (we are going to draw 2D)
    gl.disable(gl.DEPTH_TEST);

    colorProgramInfo = initColorShaders(gl);
    textureColorProgramInfo = initTextureColorShaders(gl);

    buffers = initBuffers(gl);

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
    const width = canvas.width;
    const height = canvas.height;
    gl.viewport(0, 0, width, height);
    projectionMatrix = buildOrthographicMatrix(0, width, height, 0, 0, 1);

    // Setup blending
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
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
        drawRectangleColor(x, y, w, h, color_fill);
    }

    if (getAlpha(color_border) != 0) {
        drawRectangleColor(x, y, w, border_size, color_border);
        drawRectangleColor(x, y + h - border_size, w, border_size, color_border);
        drawRectangleColor(x, y, border_size, h, color_border);
        drawRectangleColor(x + w - border_size, y, border_size, h, color_border);
    }
}

export function drawRectangle(color, x, y, w, h) {
    if (!RENDER) return;

    if (getAlpha(color) != 0) {
        drawRectangleColor(x, y, w, h, color);
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
    var id = next_texture_id++;
    var texture = gl.createTexture();
    textures[id] = texture;
    return id;
}

export function destroyCanvas(id) {
    gl.deleteTexture(id);
    delete textures[id];
}

function getTexture(id) {
    return textures[id];
}

export function setCanvasPixels(id, width, height, pixels_memory_view) {
    if (!RENDER) return;

    var texture = getTexture(id);
    var pixels = new Uint8Array(pixels_memory_view.length);
    pixels_memory_view.copyTo(pixels);
    //var pixels = pixels_memory_view.slice();

    /*

    const canvas = getTexture(id);
    const canvasCtx = canvas.getContext("2d");
    const imageData = canvasCtx.createImageData(width, height);
    pixels.copyTo(new Uint8Array(imageData.data.buffer));
    canvasCtx.putImageData(imageData, 0, 0);*/

    gl.bindTexture(gl.TEXTURE_2D, texture);
    
    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);

    // Because images have to be downloaded over the internet
    // they might take a moment until they are ready.
    // Until then put a single pixel in the texture so we can
    // use it immediately. When the image has finished downloading
    // we'll update the texture with the contents of the image.
    const level = 0;
    const internalFormat = gl.RGBA;
    const border = 0;
    const srcFormat = gl.RGBA;
    const srcType = gl.UNSIGNED_BYTE;
    gl.texImage2D(
        gl.TEXTURE_2D,
        level,
        internalFormat,
        width,
        height,
        border,
        srcFormat,
        srcType,
        pixels,
    );

    gl.generateMipmap(gl.TEXTURE_2D);
    //gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    //gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    //gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
}

export function drawCanvasRectangle(id, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight, color) {
    if (!RENDER) return;

    if (id === 0) {
        drawRectangleColor(toX, toY, toWidth, toHeight, color);
    } else {
        drawRectangleColorTexture(toX, toY, toWidth, toHeight, color, id, fromX, fromY, fromWidth, fromHeight);
    }
}

export function presentFrame() {

}

var lastProgram;

function drawRectangleColor(x, y, w, h, color) {

    if (lastProgram != colorProgramInfo) {
        lastProgram = colorProgramInfo;

        // Setup shader
        gl.useProgram(colorProgramInfo.program);
        gl.uniformMatrix4fv(
            colorProgramInfo.uniformLocations.projectionMatrix,
            false,
            projectionMatrix,
        );

        setPositionAttribute(colorProgramInfo);
        setColorAttribute(colorProgramInfo);
    }

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


    const offset = 0;
    const vertexCount = 6;
    gl.drawArrays(gl.TRIANGLES, offset, vertexCount);
}

var lastTextureId = -1;

function drawRectangleColorTexture(x, y, w, h, color, texture_id, tx, ty, tw, th) {

    if (lastProgram != textureColorProgramInfo) {
        lastProgram = textureColorProgramInfo;
        lastTextureId = -1;

        // Setup shader
        gl.useProgram(textureColorProgramInfo.program);
        gl.uniformMatrix4fv(
            textureColorProgramInfo.uniformLocations.projectionMatrix,
            false,
            projectionMatrix,
        );

        // Setup attributes
        setPositionAttribute(textureColorProgramInfo);
        setColorAttribute(textureColorProgramInfo);
        setTextureCoordAttribute(textureColorProgramInfo);
    }

    if (lastTextureId != texture_id) {
        lastTextureId = texture_id;
        const texture = getTexture(texture_id);
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture);
        gl.uniform1i(textureColorProgramInfo.uniformLocations.uSampler, 0);
    }

    // Setup buffers

    // Positions

    setPositionBuffer([
        x, y,
        x + w, y,
        x + w, y + h,
        x + w, y + h,
        x, y + h,
        x, y,
    ]);

    // Colors

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

    // Texture coordinates
    const texture_size = 512; //TODO!!

    const texture_x1 = tx / texture_size;
    const texture_y1 = 1 - (ty / texture_size);
    const texture_x2 = (tx + tw) / texture_size;
    const texture_y2 = 1 - ((ty + th) / texture_size);

    setTextureCoords([
        texture_x1, texture_y1,
        texture_x2, texture_y1,
        texture_x2, texture_y2,
        
        texture_x2, texture_y2,
        texture_x1, texture_y2,
        texture_x1, texture_y1,
    ]);

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

function setTextureCoords(textureCoords) {
    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.textureCoord);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(textureCoords), gl.DYNAMIC_DRAW);
}

function setPositionAttribute(programInfo) {
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

function setColorAttribute(programInfo) {
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

function setTextureCoordAttribute(programInfo) {
    const numComponents = 2;
    const type = gl.FLOAT;
    const normalize = false;
    const stride = 0;
    const offset = 0;
    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.textureCoord);

    gl.vertexAttribPointer(
        programInfo.attribLocations.textureCoord,
        numComponents,
        type,
        normalize,
        stride,
        offset,
    );
    gl.enableVertexAttribArray(programInfo.attribLocations.textureCoord);
}
