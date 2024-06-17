/* eslint-disable @typescript-eslint/no-use-before-define */
/* eslint-disable jsx-a11y/no-static-element-interactions */
/* eslint-disable jsx-a11y/click-events-have-key-events */
/* eslint-disable no-restricted-syntax */
/* eslint-disable consistent-return */
import { useEffect, useMemo } from 'react';
import { FaAngleDown, FaAngleRight } from 'react-icons/fa';
import { useNavigate } from 'react-router';
import { useSearchParams } from 'react-router-dom';

import { IContestCategory } from '../../../common/types';
import usePreserveScrollPosition from '../../../hooks/common/use-preserve-scroll-position';
import useTheme from '../../../hooks/use-theme';
import {
    setContestCategories,
    setContestCategory,
    setContestStrategy,
    updateContestCategoryBreadcrumbItem,
} from '../../../redux/features/contestsSlice';
import { useGetContestCategoriesQuery } from '../../../redux/services/contestsService';
import { useAppDispatch } from '../../../redux/store';
import concatClassNames from '../../../utils/class-names';
import SpinningLoader from '../../guidelines/spinning-loader/SpinningLoader';

import styles from './ContestCategories.module.scss';

interface IContestCategoriesProps {
    isRenderedOnHomePage?: boolean;
}

const ContestCategories = (props: IContestCategoriesProps) => {
    const { isRenderedOnHomePage = false } = props;

    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { themeColors, getColorClassName } = useTheme();
    usePreserveScrollPosition();
    const saveScrollPosition = usePreserveScrollPosition();

    const textColorClassName = getColorClassName(themeColors.textColor);

    const {
        data: contestCategories,
        isLoading: areCategoriesLoading,
        error: categoriesError,
    } = useGetContestCategoriesQuery();

    const selectedId = useMemo(() => Number(searchParams.get('category')), [ searchParams ]);

    useEffect(() => {
        dispatch(setContestCategories({ contestCategories: contestCategories || [] }));
    }, [ contestCategories, dispatch ]);

    useEffect(() => {
        const selectedCategory = findContestCategoryByIdRecursive(contestCategories, selectedId);
        const breadcrumbItems = findParentNames(contestCategories, selectedId);

        dispatch(setContestCategory(selectedCategory));
        dispatch(updateContestCategoryBreadcrumbItem({ elements: breadcrumbItems }));
    /* eslint-disable-next-line react-hooks/exhaustive-deps */
    }, [ selectedId, contestCategories ]);

    const onContestCategoryClick = (id: number) => {
        saveScrollPosition();
        if (isRenderedOnHomePage) {
            navigate(`/contests?category=${id}`);
            return;
        }

        const selectedContestCategory = findContestCategoryByIdRecursive(contestCategories, id);
        if (!selectedContestCategory) {
            return;
        }

        const parents = findParentNames(contestCategories, selectedContestCategory?.id);
        // click is on already selected category
        if (searchParams.get('category') === selectedContestCategory?.id.toString()) {
            if (parents && parents?.length > 1) {
                const selectedParentCategory = parents[parents.length - 2];
                searchParams.set('category', selectedParentCategory.id.toString());
                searchParams.delete('strategy');

                const contestElementInFormat = findContestCategoryByIdRecursive(contestCategories, selectedParentCategory.id);
                dispatch(setContestCategory(contestElementInFormat));
                dispatch(setContestStrategy(null));
            } else {
                searchParams.delete('category');

                dispatch(setContestCategory(null));
                dispatch(setContestStrategy(null));
            }
        } else {
            searchParams.set('page', '1');
            searchParams.set('category', id.toString());
            searchParams.delete('strategy');

            dispatch(setContestCategory(selectedContestCategory));
            dispatch(setContestStrategy(null));
        }

        setSearchParams(searchParams);
    };

    const renderCategory = (category: IContestCategory, isChildElement = false) => {
        const isActiveOrHasActiveChild = findActiveChildrenByIdRecursive(category.children, selectedId) || selectedId === category.id;
        const categoryItemClassNames = concatClassNames(
            styles.categoryItem,
            selectedId === category.id
                ? styles.selectedCategory
                : '',
            isChildElement
                ? styles.childCategoryItem
                : '',
        );

        return (
            <div
              key={`contest-category-item-${category.id}`}
            >
                <div
                  style={{
                      borderBottom: `${isChildElement
                          ? 0
                          : 1}px solid ${themeColors.textColor}`,
                  }}
                  className={categoryItemClassNames}
                  onClick={() => onContestCategoryClick(category.id)}
                >
                    { isChildElement && category.children.length > 0 && <FaAngleRight /> }
                    <div>
                        {category.name}
                    </div>
                    { !isChildElement && category.children.length > 0 && <FaAngleDown />}
                </div>
                <div
                  className={`${styles.categoryChildren} ${isActiveOrHasActiveChild
                      ? styles.activeChildren
                      : ''}`}
                >
                    {category.children.map((child) => renderCategory(child, true))}
                </div>
            </div>
        );
    };

    if (areCategoriesLoading) {
        return (
            <div className={styles.contestCategoriesWrapper}>
                <SpinningLoader />
            </div>
        );
    }
    return (
        <div className={styles.contestCategoriesWrapper}>
            <div
              className={`${styles.contestCategoriesHeader} ${textColorClassName}`}
              style={{
                  marginTop: isRenderedOnHomePage
                      ? 0
                      : 32,
              }}
            >
                <div>Contest Categories</div>
            </div>
            { categoriesError
                ? <div className={textColorClassName}>Error loading categories</div>
                : (
                    <div
                      className={`${styles.contestCategoriesInnerWrapper} ${textColorClassName}`}
                    >
                        {contestCategories?.map((contestCategory: IContestCategory) => renderCategory(contestCategory))}
                    </div>
                )}
        </div>
    );
};

// eslint-disable-next-line @typescript-eslint/no-shadow
const findParentNames = (collection: Array<IContestCategory> | undefined, selectedId: number) => {
    if (!collection || !selectedId) {
        return;
    }
    const findParentsRecursive = (
        element: IContestCategory,
        targetId: number,
        parents: Array<{ name: string; id: number }> = [],
    ): Array<{name: string; id: number}> | null => {
        if (element.id === targetId) {
            return [ ...parents, { name: element.name, id: element.id } ];
        }

        for (const child of element.children || []) {
            const result = findParentsRecursive(child, targetId, [ ...parents, { name: element.name, id: element.id } ]);

            if (result) {
                return result;
            }
        }

        return null;
    };

    for (const node of collection) {
        const parents = findParentsRecursive(node, selectedId);
        if (parents) {
            return parents;
        }
    }

    return [];
};

const findContestCategoryByIdRecursive =
    (elements: Array<IContestCategory> | undefined, id: number, rootIndex = 0): IContestCategory | null => {
        if (!elements) {
            return null;
        }
        // eslint-disable-next-line no-restricted-syntax
        for (const contestCategory of elements) {
            if (contestCategory.id === id) {
                return contestCategory;
            }
            if (contestCategory.children.length) {
                const foundCategory = findContestCategoryByIdRecursive(contestCategory.children, id, rootIndex + 1);
                if (foundCategory !== null) {
                    return foundCategory;
                }
            }
        }
        return null;
    };

const findActiveChildrenByIdRecursive = (elements: Array<IContestCategory> | undefined, id: number) => {
    if (!elements) {
        return null;
    }
    // eslint-disable-next-line no-restricted-syntax
    for (const element of elements) {
        if (element.id === id) {
            return true;
        }
        if (element.children) {
            const found = findActiveChildrenByIdRecursive(element.children, id);
            if (found) {
                return true;
            }
        }
    }
    return false;
};

export { ContestCategories, findContestCategoryByIdRecursive, findParentNames };
