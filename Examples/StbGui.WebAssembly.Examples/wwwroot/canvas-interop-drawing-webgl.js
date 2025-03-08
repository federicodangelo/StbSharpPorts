import { initColorShaders, initTextureColorShaders } from "./canvas-interop-drawing-webgl-shaders.js";

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

export function initDrawing() {
    canvas = document.getElementById("myCanvas");
    //console.log(canvas);
    gl = canvas.getContext("webgl", {});

    // Disable depth testing (we are going to draw 2D)
    gl.disable(gl.DEPTH_TEST);
    gl.depthMask(false);

    // Setup color
    gl.colorMask(true, true, true, true)

    // Setup blending
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);

    // Compile shaders
    colorProgramInfo = initColorShaders(gl);
    textureColorProgramInfo = initTextureColorShaders(gl);

    // Init buffers
    buffers = {
        position: gl.createBuffer(),
        color: gl.createBuffer(),
        textureCoord: gl.createBuffer(),
        indices: gl.createBuffer(),
    };


    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.position);
    gl.bufferData(gl.ARRAY_BUFFER, tempVertPositions.byteLength, gl.DYNAMIC_DRAW);

    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.color);
    gl.bufferData(gl.ARRAY_BUFFER, tempVertColors.byteLength, gl.DYNAMIC_DRAW);

    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.textureCoord);
    gl.bufferData(gl.ARRAY_BUFFER, tempVertTextureCoords.byteLength, gl.DYNAMIC_DRAW);

    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, buffers.indices);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, tempVertIndices.byteLength, gl.DYNAMIC_DRAW);

    return canvas;
}

export function getWidth() {
    return canvas.width;
}

export function getHeight() {
    return canvas.height;
}

let lastWidth = -1;
let lastHeight = -1;

export function clear(c) {
    if (!RENDER) return;

    // Setup viewport and projection matrix
    const width = getWidth();
    const height = getHeight();

    if (width != lastWidth || height != lastHeight) {
        lastWidth = width;
        lastHeight = height;
        gl.viewport(0, 0, width, height);
        projectionMatrix = buildOrthographicMatrix(0, width, height, 0, 0, 1);
    }

    // Clear
    gl.clearColor(getRed(c) / 255, getGreen(c) / 255, getBlue(c) / 255, getAlpha(c) / 255);
    gl.clear(gl.COLOR_BUFFER_BIT);
}

function buildOrthographicMatrix(left, right, bottom, top, near, far) {
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

    submitVertices();

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

    submitVertices();

    clip_rects.pop();

    if (clip_rects.length > 0) {
        var rect_clipped = clip_rects[clip_rects.length - 1];
        setScissor(rect_clipped);
    } else {
        gl.disable(gl.SCISSOR_TEST);
    }
}

export function createTexture(width, height, pixels_memory_view) {
    var texture = gl.createTexture();
    var pixels = pixels_memory_view.slice();

    gl.bindTexture(gl.TEXTURE_2D, texture);

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
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

    var id = next_texture_id++;
    textures[id] = { texture, width, height, id };
    return id;
}

export function destroyTexture(id) {
    gl.deleteTexture(id);
    delete textures[id].texture;
}

function getTextureInfo(id) {
    return textures[id];
}

export function drawTextureRectangle(id, fromX, fromY, fromWidth, fromHeight, toX, toY, toWidth, toHeight, color) {
    if (!RENDER) return;

    if (id === 0) {
        drawRectangleColor(toX, toY, toWidth, toHeight, color);
    } else {
        drawRectangleColorTexture(toX, toY, toWidth, toHeight, color, id, fromX, fromY, fromWidth, fromHeight);
    }
}

export function presentFrame() {
    submitVertices();
}

var lastProgram;
var lastProgramUsesPositions = false;
var lastProgramUsesColors = false;
var lastProgramUsesTextureCoords = false;

const tempRectanglesCount = 8192;
const tempVerticesCount = tempRectanglesCount * 4; // 4 vertices per rectangle

var tempVertPositions = new Float32Array(tempVerticesCount * 2);        // x,y (can be changed to Uint16Array if needed)
var tempVertColors = new Uint32Array(tempVerticesCount);                // rgba (encoded in a single uint32)
var tempVertTextureCoords = new Float32Array(tempVerticesCount * 2);    // u,v (can be changed to Uint16Array if needed)
var tempVertIndices = new Uint16Array(tempRectanglesCount * 6);         // 6 indices per rectangle (2 triangles)

var tempVertIndex = 0;
var tempVertIndicesIndex = 0;

function submitVerticesIfNoRoom(vertices, indexes) {
    if (tempVertIndex + vertices >= tempVerticesCount ||
        tempVertIndicesIndex + indexes >= tempVerticesCount) {

        submitVertices();
    }
}

function addVertexColor(x, y, c) {
    tempVertPositions[tempVertIndex * 2] = x;
    tempVertPositions[tempVertIndex * 2 + 1] = y;
    tempVertColors[tempVertIndex] = c;
    tempVertIndex++;
}

function addVertexColorTexture(x, y, c, tx, ty) {
    tempVertPositions[tempVertIndex * 2] = x;
    tempVertPositions[tempVertIndex * 2 + 1] = y;
    tempVertColors[tempVertIndex] = c;
    tempVertTextureCoords[tempVertIndex * 2] = tx;
    tempVertTextureCoords[tempVertIndex * 2 + 1] = ty;
    tempVertIndex++;
}

function addRectangleIndices() {
    const index = tempVertIndex - 4;

    tempVertIndices[tempVertIndicesIndex++] = index;
    tempVertIndices[tempVertIndicesIndex++] = index + 1;
    tempVertIndices[tempVertIndicesIndex++] = index + 2;
    tempVertIndices[tempVertIndicesIndex++] = index + 2;
    tempVertIndices[tempVertIndicesIndex++] = index + 3;
    tempVertIndices[tempVertIndicesIndex++] = index;
}

function submitVertices() {
    if (tempVertIndex == 0) return;

    if (lastProgramUsesPositions)
        setPositionBuffer(tempVertPositions.subarray(0, tempVertIndex * 2));

    if (lastProgramUsesColors)
        setColorsBuffer(tempVertColors.subarray(0, tempVertIndex));

    if (lastProgramUsesTextureCoords)
        setTextureCoords(tempVertTextureCoords.subarray(0, tempVertIndex * 2));

    setElementIndices(tempVertIndices.subarray(0, tempVertIndicesIndex));

    gl.drawElements(gl.TRIANGLES, tempVertIndicesIndex, gl.UNSIGNED_SHORT, 0);
    tempVertIndex = 0;
    tempVertIndicesIndex = 0;
}

function drawRectangleColor(x, y, w, h, color) {

    if (lastProgram != colorProgramInfo) {
        submitVertices();

        lastProgram = colorProgramInfo;

        // Setup shader
        gl.useProgram(colorProgramInfo.program);
        gl.uniformMatrix4fv(
            colorProgramInfo.uniformLocations.projectionMatrix,
            false,
            projectionMatrix,
        );

        // Setup attributes
        setPositionAttribute(colorProgramInfo);
        setColorAttribute(colorProgramInfo);

        lastProgramUsesPositions = true;
        lastProgramUsesColors = true;
        lastProgramUsesTextureCoords = false;
    }

    submitVerticesIfNoRoom(4, 6);

    addVertexColor(x, y, color);
    addVertexColor(x + w, y, color);
    addVertexColor(x + w, y + h, color);
    addVertexColor(x, y + h, color);

    addRectangleIndices();
}

var lastTextureId = -1;
var lastTextureInfo = null;

function drawRectangleColorTexture(x, y, w, h, color, texture_id, tx, ty, tw, th) {

    if (lastProgram != textureColorProgramInfo) {
        submitVertices();

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

        lastProgramUsesColors = true;
        lastProgramUsesPositions = true;
        lastProgramUsesTextureCoords = true;
    }

    if (lastTextureId != texture_id) {
        submitVertices();

        lastTextureId = texture_id;
        lastTextureInfo = getTextureInfo(texture_id);

        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, lastTextureInfo.texture);
        gl.uniform1i(textureColorProgramInfo.uniformLocations.uSampler, 0);
    }

    const texture_x1 = tx / lastTextureInfo.width;
    const texture_y1 =  ty / lastTextureInfo.height;
    const texture_x2 = (tx + tw) / lastTextureInfo.width;
    const texture_y2 =  (ty + th) / lastTextureInfo.height;

    submitVerticesIfNoRoom(4, 6);

    addVertexColorTexture(x, y, color, texture_x1, texture_y1);
    addVertexColorTexture(x + w, y, color, texture_x2, texture_y1);
    addVertexColorTexture(x + w, y + h, color, texture_x2, texture_y2);
    addVertexColorTexture(x, y + h, color, texture_x1, texture_y2);

    addRectangleIndices();
}

function setPositionBuffer(positions) {
    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.position);
    gl.bufferSubData(gl.ARRAY_BUFFER, 0, positions);
}

function setColorsBuffer(colors) {
    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.color);
    gl.bufferSubData(gl.ARRAY_BUFFER, 0, colors);
}

function setTextureCoords(textureCoords) {
    gl.bindBuffer(gl.ARRAY_BUFFER, buffers.textureCoord);
    gl.bufferSubData(gl.ARRAY_BUFFER, 0, textureCoords);
}

function setElementIndices(indices) {
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, buffers.indices);
    gl.bufferSubData(gl.ELEMENT_ARRAY_BUFFER, 0, indices);
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
    const type = gl.UNSIGNED_BYTE;
    const normalize = true;
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
