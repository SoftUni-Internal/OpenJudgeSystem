 
declare module 'redux-persist-indexeddb-storage' {
    import type { Storage } from 'redux-persist';

    /**
     * Create an IndexedDB-backed Storage adapter for redux-persist.
     * @param dbName  Optional DB name (defaults to "keyval-store")
     */
    export default function createIDBStorage(dbName?: string): Storage;
}
