import { ReactNode } from 'react';

type ClassNameType = string | string[];

interface IHaveChildrenProps {
    children: ReactNode;
}

interface IHaveOptionalChildrenProps {
    children?: ReactNode;
}

interface IHaveOptionalClassName {
    className?: ClassNameType;
}

interface IHavePagesProps {
    itemsPerPage: number,
    pageNumber: number,
    totalItemsCount: number,
    pagesCount: number,
}

export type {
    IHaveChildrenProps,
    IHaveOptionalChildrenProps,
    IHaveOptionalClassName,
    ClassNameType,
    IHavePagesProps,
};
