// Communication layer with the web worker
let worker = null;
const pendingRequests = new Map();
let messageId = 0;

// Initialize the worker
function initializeWorker() {
    if (!worker) {
        worker = new Worker('js/indexedDbWorker.js');

        worker.onmessage = (event) => {
            const { id, result, error } = event.data;

            if (pendingRequests.has(id)) {
                const { resolve, reject } = pendingRequests.get(id);
                pendingRequests.delete(id);

                if (error) {
                    reject(error);
                } else {
                    resolve(result);
                }
            }
        };
    }
    return worker;
}

// Send a message to the worker and return a promise
function sendToWorker(action, payload) {
    return new Promise((resolve, reject) => {
        const worker = initializeWorker();
        const id = messageId++;

        pendingRequests.set(id, { resolve, reject });
        worker.postMessage({ id, action, payload });
    });
}

// API for .NET to call
export function storeObject(key, jsonString) {
    return sendToWorker('store', { key, value: jsonString });
}

export function readObject(key) {
    return sendToWorker('read', { key });
}

export function getKeys() {
    return sendToWorker('getKeys', {});
}

export function removeObject(key) {
    return sendToWorker('remove', { key });
}
