import React, { FC } from 'react';

import { Anything } from '../../common/common-types';
import { IHaveChildrenProps } from '../../components/common/Props';
import concatClassNames from '../../utils/class-names';

import styles from './set-layout.module.scss';

interface ILayoutProps extends IHaveChildrenProps {
    isWide: boolean;
}

const Layout = ({ children, isWide }: ILayoutProps) => {
    const wideClassName = isWide
        ? styles.wideContentWrapper
        : '';

    const className = concatClassNames(styles.contentWrapper, wideClassName);

    return (
        <div className={className}>
            <div style={{ height: '18px', backgroundColor: 'rebeccapurple', color: 'white' }}>Testing environment</div>
            {children}
        </div>
    );
};

const setLayout = (ComponentToWrap: FC, isWide = false) => (props: Anything) => (
    <Layout isWide={isWide}>
        <ComponentToWrap {...props} />
    </Layout>
);

export default setLayout;
