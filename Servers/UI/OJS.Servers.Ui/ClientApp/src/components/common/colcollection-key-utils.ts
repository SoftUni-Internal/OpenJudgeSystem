﻿const defaultKeyFunc = <TValue extends unknown>(value: TValue) => {
    const objWithId = value as { id: string };

    if (objWithId.id) {
        return objWithId.id.toString();
    }

    return JSON.stringify(value);
};

export default defaultKeyFunc;

export {
    defaultKeyFunc,
};
