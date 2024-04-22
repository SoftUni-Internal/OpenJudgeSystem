const splitByCapitalLetter = (input: string) => input.split(/(?=[A-Z])/).join(' ');

const toLowerCase = (input: string) => input.toLowerCase();

const getEnumMemberName = <T extends object>(enumObj: T, enumMember: T[keyof T] | string): string => {
    const enumName = (Object.keys(enumObj) as Array<keyof T>).find((key) => enumObj[key] === enumMember);
    return enumName
        ? String(enumName)
        : '';
};

const capitalizeFirstLetter = (value: string) => value.charAt(0).toUpperCase() + value.slice(1);

export default {
    splitByCapitalLetter,
    toLowerCase,
    capitalizeFirstLetter,
};

export {
    splitByCapitalLetter,
    toLowerCase,
    getEnumMemberName,
    capitalizeFirstLetter,
};
