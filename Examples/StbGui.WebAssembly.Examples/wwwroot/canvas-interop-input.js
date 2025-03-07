const input_events = [];

const MOUSE_DOWN_EVENT_TYPE = 1;
const MOUSE_UP_EVENT_TYPE = 2;
const MOUSE_MOVE_EVENT_TYPE = 3;
const MOUSE_WHEEL_EVENT_TYPE = 4;

const KEY_DOWN_EVENT_TYPE = 10;
const KEY_UP_EVENT_TYPE = 11;

export function initInputEvents(target) {
    target.addEventListener('mousedown', (evt) => {
        input_events.push({ type: MOUSE_DOWN_EVENT_TYPE, x: evt.offsetX, y: evt.offsetY, button: evt.button });
    });

    target.addEventListener('mouseup', (evt) => {
        input_events.push({ type: MOUSE_UP_EVENT_TYPE, x: evt.offsetX, y: evt.offsetY, button: evt.button });
    });

    target.addEventListener('mousemove', (evt) => {
        input_events.push({ type: MOUSE_MOVE_EVENT_TYPE, x: evt.offsetX, y: evt.offsetY });
    });

    target.addEventListener('wheel', (evt) => {
        input_events.push({ type: MOUSE_WHEEL_EVENT_TYPE, x: evt.offsetX, y: evt.offsetY, dx: Math.round(evt.deltaX), dy: Math.round(evt.deltaY) });
    });

    document.addEventListener('keydown', (evt) => {
        input_events.push({ type: KEY_DOWN_EVENT_TYPE, alt: evt.altKey ? 1 : 0, ctrl: evt.ctrlKey ? 1 : 0, meta: evt.metaKey ? 1 : 0, shift: evt.shiftKey ? 1 : 0, key: evt.key, code: evt.code });
    });

    document.addEventListener('keyup', (evt) => {
        input_events.push({ type: KEY_UP_EVENT_TYPE, alt: evt.altKey ? 1 : 0, ctrl: evt.ctrlKey ? 1 : 0, meta: evt.metaKey ? 1 : 0, shift: evt.shiftKey ? 1 : 0, key: evt.key, code: evt.code });
    });    
}

export function getInputEventsCount() {
    return input_events.length;
}

export function getInputEvent(index) {
    return input_events[index].type;
}

export function getInputEventProperty(index, property) {
    return input_events[index][property];
}

export function getInputEventPropertyString(index, property, buffer) {
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

export function clearInputEvents() {
    input_events.length = 0;
}