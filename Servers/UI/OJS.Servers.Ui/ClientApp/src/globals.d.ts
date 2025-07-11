/* eslint-disable @typescript-eslint/naming-convention */

declare global {
    interface Window {
        gtag?: (...args: any[]) => void;
    }
}

export {};
