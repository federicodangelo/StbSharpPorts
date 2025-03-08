//
// Initialize a shader program, so WebGL knows how to draw our data
//
function initShaderProgram(gl, vsSource, fsSource) {
    const vertexShader = loadShader(gl, gl.VERTEX_SHADER, vsSource);
    const fragmentShader = loadShader(gl, gl.FRAGMENT_SHADER, fsSource);

    // Create the shader program

    const shaderProgram = gl.createProgram();
    gl.attachShader(shaderProgram, vertexShader);
    gl.attachShader(shaderProgram, fragmentShader);
    gl.linkProgram(shaderProgram);

    // If creating the shader program failed, alert

    if (!gl.getProgramParameter(shaderProgram, gl.LINK_STATUS)) {
        alert(
            `Unable to initialize the shader program: ${gl.getProgramInfoLog(
                shaderProgram,
            )}`,
        );
        return null;
    }

    return shaderProgram;
}

//
// creates a shader of the given type, uploads the source and
// compiles it.
//
function loadShader(gl, type, source) {
    const shader = gl.createShader(type);

    // Send the source to the shader object

    gl.shaderSource(shader, source);

    // Compile the shader program

    gl.compileShader(shader);

    // See if it compiled successfully

    if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
        alert(
            `An error occurred compiling the shaders: ${gl.getShaderInfoLog(shader)}`,
        );
        gl.deleteShader(shader);
        return null;
    }

    return shader;
}

export function initColorShaders(gl) {
    const vs = `
        attribute vec2 aVertexPosition;
        attribute vec4 aVertexColor;

        uniform mat4 uProjectionMatrix;

        varying lowp vec4 vColor;

        void main() {
            gl_Position = uProjectionMatrix * vec4(aVertexPosition, 0, 1);
            vColor = aVertexColor;
        }
    `;

    const fs = `
        varying lowp vec4 vColor;
        
        void main() {
            gl_FragColor = vColor;
        }
    `;


    const shaderProgram = initShaderProgram(gl, vs, fs);

    const programInfo = {
        program: shaderProgram,
        attribLocations: {
            vertexPosition: gl.getAttribLocation(shaderProgram, "aVertexPosition"),
            vertexColor: gl.getAttribLocation(shaderProgram, "aVertexColor"),
        },
        uniformLocations: {
            projectionMatrix: gl.getUniformLocation(shaderProgram, "uProjectionMatrix"),
        },
    };

    return programInfo;
}

export function initTextureColorShaders(gl) {
    const vs_color = `
        attribute vec2 aVertexPosition;
        attribute vec4 aVertexColor;
        attribute vec2 aTextureCoord;

        uniform mat4 uProjectionMatrix;

        varying lowp vec4 vColor;
        varying mediump vec2 vTextureCoord;

        void main() {
            gl_Position = uProjectionMatrix * vec4(aVertexPosition, 0, 1);
            vColor = aVertexColor;
            vTextureCoord = aTextureCoord;
        }
    `;

    const fs_color = `
        varying lowp vec4 vColor;
        varying mediump vec2 vTextureCoord;

        uniform sampler2D uSampler;
        
        void main() {
            gl_FragColor = texture2D(uSampler, vTextureCoord) * vColor;
        }
    `;


    const shaderProgram = initShaderProgram(gl, vs_color, fs_color);

    const programInfo = {
        program: shaderProgram,
        attribLocations: {
            vertexPosition: gl.getAttribLocation(shaderProgram, "aVertexPosition"),
            vertexColor: gl.getAttribLocation(shaderProgram, "aVertexColor"),
            textureCoord: gl.getAttribLocation(shaderProgram, "aTextureCoord"),
        },
        uniformLocations: {
            projectionMatrix: gl.getUniformLocation(shaderProgram, "uProjectionMatrix"),
            uSampler: gl.getUniformLocation(shaderProgram, "uSampler"),
        },
    };

    return programInfo;
}
