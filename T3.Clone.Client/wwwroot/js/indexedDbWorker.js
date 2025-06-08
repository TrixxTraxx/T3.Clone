// IndexedDB Web Worker implementation
let db = null;
const DB_NAME = 'blazorKeyValueStore';
const STORE_NAME = 'jsonBlobStore';
const DB_VERSION = 1;

// Initialize the database
function openDatabase() {
    return new Promise((resolve, reject) => {
        if (db) {
            resolve(db);
            return;
        }

        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = (event) => {
            const db = event.target.result;
            if (!db.objectStoreNames.contains(STORE_NAME)) {
                db.createObjectStore(STORE_NAME);
            }
        };

        request.onsuccess = (event) => {
            db = event.target.result;
            resolve(db);
        };

        request.onerror = (event) => {
            reject(`IndexedDB error: ${event.target.error}`);
        };
    });
}

// Store an object
async function storeObject(key, value) {
    const db = await openDatabase();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORE_NAME], 'readwrite');
        const store = transaction.objectStore(STORE_NAME);

        const request = store.put(value, key);

        request.onsuccess = () => resolve(true);
        request.onerror = (event) => reject(`Error storing object: ${event.target.error}`);
    });
}

// Read an object
async function readObject(key) {
    const db = await openDatabase();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORE_NAME], 'readonly');
        const store = transaction.objectStore(STORE_NAME);

        const request = store.get(key);

        request.onsuccess = () => resolve(request.result);
        request.onerror = (event) => reject(`Error reading object: ${event.target.error}`);
    });
}

// Get all keys
async function getAllKeys() {
    const db = await openDatabase();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORE_NAME], 'readonly');
        const store = transaction.objectStore(STORE_NAME);

        const request = store.getAllKeys();

        request.onsuccess = () => resolve(request.result);
        request.onerror = (event) => reject(`Error getting keys: ${event.target.error}`);
    });
}

// Remove an object
async function removeObject(key) {
    const db = await openDatabase();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction([STORE_NAME], 'readwrite');
        const store = transaction.objectStore(STORE_NAME);

        const request = store.delete(key);

        request.onsuccess = () => resolve(true);
        request.onerror = (event) => reject(`Error removing object: ${event.target.error}`);
    });
}

// Handle messages from the main thread
self.onmessage = async (event) => {
    const { id, action, payload } = event.data;

    try {
        let result;

        switch (action) {
            case 'store':
                result = await storeObject(payload.key, payload.value);
                break;
            case 'read':
                result = await readObject(payload.key);
                break;
            case 'getKeys':
                result = await getAllKeys();
                break;
            case 'remove':
                result = await removeObject(payload.key);
                break;
            default:
                throw new Error(`Unknown action: ${action}`);
        }

        self.postMessage({ id, result });
    } catch (error) {
        self.postMessage({ id, error: error.toString() });
    }
};
