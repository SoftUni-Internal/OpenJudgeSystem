import React, { FC, useEffect, useMemo } from 'react';
import { Params, useParams, useSearchParams } from 'react-router-dom';

import { Anything } from '../../common/common-types';

interface IHaveChildrenPropsWithTitle {
    children: React.ReactNode;
    title: string | ((params: Params, searchParams: URLSearchParams) => string);
}

const Page = ({ children, title } : IHaveChildrenPropsWithTitle) => {
    const params = useParams();
    const [ searchParams ] = useSearchParams();

    const pageTitle = useMemo(
        () => typeof title === 'function'
            ? title(params, searchParams)
            : title,
        [ title, params, searchParams ],
    );

    useEffect(() => {
        document.title = `${pageTitle} - SoftUni Judge`;
    }, [ pageTitle ]);

    return (
         
        <>
            {children}
        </>
    );
};

const withTitle = (
    ComponentToWrap: FC,
    title: string | ((params: Params, searchParams: URLSearchParams) => string),
) => (props: Anything) => 
    <Page title={title}>
        <ComponentToWrap {...props} />
    </Page>
;

export default withTitle;
