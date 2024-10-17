import React, { useCallback, useMemo } from 'react';
import { Link } from 'react-router-dom';
import isNil from 'lodash/isNil';
import { IHaveOptionalClassName } from 'src/components/common/Props';
import useTheme from 'src/hooks/use-theme';
import isNilOrEmpty from 'src/utils/check-utils';
import concatClassNames from 'src/utils/class-names';

import styles from 'src/components/guidelines/breadcrumb/Breadcrumbs.module.scss';

interface IPageBreadcrumbsItem {
    text: string;
    /* If left as null, breadcrumb item is not rendered as link */
    to?: string;
}

interface IBreadcrumbProps extends IHaveOptionalClassName {
    keyPrefix: string;
    items?: IPageBreadcrumbsItem[] | null;
    isLoading?: boolean;
    isHidden?: boolean;
}

const Breadcrumbs = ({
    keyPrefix,
    items = null,
    className = '',
    isLoading = false,
    isHidden = false,
}: IBreadcrumbProps) => {
    const { themeColors, getColorClassName } = useTheme();

    const textColorClassName = getColorClassName(themeColors.textColor);
    const backgroundColorClassName = getColorClassName(themeColors.baseColor300);

    const itemsList = useMemo(() => Array.prototype.concat(
        [ { text: 'Home', to: '/' } as IPageBreadcrumbsItem ],
        items,
    ), [ items ]);

    const renderItems = useCallback(
        () => {
            if (isNil(items)) {
                return null;
            }

            const renderItemContent = (item: IPageBreadcrumbsItem, isLast: boolean) => (
                <div
                  className={concatClassNames(
                      styles.item,
                      isLast
                          ? textColorClassName
                          : '',
                  )}
                >
                    <p>
                        {item.text}
                    </p>
                    <p className={textColorClassName}>{!isLast && '/'}</p>
                </div>
            );

            return itemsList
                .map((item: IPageBreadcrumbsItem, idx: number) => {
                    const isLast = idx === itemsList.length - 1;

                    return isNilOrEmpty(item.to) || isLast
                        ? renderItemContent(item, isLast)
                        : (
                            <Link
                              key={`${keyPrefix}-breadcrumb-item-${idx}`}
                              to={item.to!}
                            >
                                {renderItemContent(item, isLast)}
                            </Link>
                        );
                });
        },
        [ items, itemsList, keyPrefix, textColorClassName ],
    );

    const internalClassName = concatClassNames(
        styles.breadcrumbsWrapper,
        textColorClassName,
        backgroundColorClassName,
        isHidden
            ? styles.nonVisible
            : '',
        className,
    );

    if (isLoading) {
        return (
            <div className={internalClassName}>
                Loading breadcrumbs...
            </div>
        );
    }

    return (
        <div className={internalClassName}>
            {renderItems()}
        </div>
    );
};

export type {
    IPageBreadcrumbsItem,
};

export default Breadcrumbs;
