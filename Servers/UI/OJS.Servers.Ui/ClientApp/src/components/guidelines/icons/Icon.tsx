import React, { useCallback } from 'react';
import { IconBaseProps } from 'react-icons/lib/cjs/iconBase';

import concatClassNames from '../../../utils/class-names';
import { IHaveOptionalClassName } from '../../common/Props';

import IconSize from './common/icon-sizes';

import styles from './Icon.module.scss';

interface IIconProps extends IHaveOptionalClassName {
    size?: IconSize;
    helperText?: string;
}

interface IIconInternalProps extends IIconProps {
    Component: React.FC<IconBaseProps>;
}

const Icon = ({
    Component,
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IIconInternalProps) => {
    const sizeClassName =
        size === IconSize.Small
            ? styles.small
            : size === IconSize.Medium
                ? styles.medium
                : size === IconSize.Large
                    ? styles.large
                    : styles.extraLarge;

    const iconClassName = concatClassNames(
        styles.icon,
        className,
    );

    const iconContainerClassName = concatClassNames(
        styles.iconContainer,
        sizeClassName,
    );

    const renderHelperText = useCallback(
        () => {
            if (helperText === '') {
                return null;
            }

            return <span className={styles.helperText}>{helperText}</span>;
        },
        [ helperText ],
    );

    return (
        <div className={iconContainerClassName}>
            <Component className={iconClassName} />
            {renderHelperText()}
        </div>
    );
};

export default Icon;

export type {
    IIconProps,
};
