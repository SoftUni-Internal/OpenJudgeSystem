/* eslint-disable @typescript-eslint/no-use-before-define */
/* eslint-disable jsx-a11y/no-static-element-interactions */
/* eslint-disable jsx-a11y/click-events-have-key-events */
/* eslint-disable react-hooks/exhaustive-deps */

import React, { useEffect, useMemo, useState } from 'react';
import { FaAngleDown, FaAngleUp, FaRegFileAlt } from 'react-icons/fa';
import { useDispatch } from 'react-redux';
import { useNavigate } from 'react-router';
import { useSearchParams } from 'react-router-dom';

import { IContestCategory } from '../../../common/types';
import useTheme from '../../../hooks/use-theme';
import {
    setContestCategory,
    setContestStrategy,
    updateContestCategoryBreadcrumbItem,
} from '../../../redux/features/contestsSlice';
import { useGetContestCategoriesQuery } from '../../../redux/services/contestsService';
import SpinningLoader from '../../guidelines/spinning-loader/SpinningLoader';

import styles from './ContestCategories.module.scss';

interface IContestCategoriesProps {
    isRenderedOnHomePage?: boolean;
}

const ContestCetegories = (props: IContestCategoriesProps) => {
    const { isRenderedOnHomePage = false } = props;

    const navigate = useNavigate();
    const dispatch = useDispatch();
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { themeColors } = useTheme();

    const {
        data: contestCategories,
        isLoading: areCategoriesLoading,
        error: categoriesError,
    } = useGetContestCategoriesQuery();

    const [ isExpanded, setIsExpanded ] = useState<boolean>(true);

    const selectedId = useMemo(() => Number(searchParams.get('category')), [ searchParams ]);

    useEffect(() => {
        const categoryId = searchParams.get('category');
        if (categoryId) {
            const selectedContestCategory = findContestCategoryByIdRecursive(contestCategories, Number(categoryId));
            dispatch(setContestCategory(selectedContestCategory));
        }
    }, [ searchParams.get('category') ]);

    const findContestCategoryByIdRecursive =
        (elements: Array<IContestCategory> | undefined, id: number, rootIndex = 0): IContestCategory | null => {
            if (!elements) {
                return null;
            }
            // eslint-disable-next-line no-restricted-syntax
            for (const contestCategory of elements) {
                if (contestCategory.id === id) {
                    dispatch(updateContestCategoryBreadcrumbItem({ index: rootIndex, element: contestCategory }));
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

    const onContestCategoryClick = (id: number) => {
        if (isRenderedOnHomePage) {
            navigate(`/contests?category=${id}`);
            return;
        }
        const selectedContestCategory = findContestCategoryByIdRecursive(contestCategories, id);
        searchParams.set('page', '1');
        searchParams.set('category', id.toString());

        setSearchParams(searchParams);
        dispatch(setContestCategory(selectedContestCategory));
        dispatch(setContestStrategy(null));
    };

    const renderCategory = (category: IContestCategory) => {
        const isActiveOrHasActiveChild = findActiveChildrenByIdRecursive(category.children, selectedId) || selectedId === category.id;
        return (
            <div
              key={`contest-category-item-${category.id}`}
            >
                <div
                  style={{ borderBottom: `1px solid ${themeColors.textColor}` }}
                  className={`${styles.categoryItem} ${selectedId === category.id
                      ? styles.selectedCategory
                      : ''}`}
                  onClick={() => onContestCategoryClick(category.id)}
                >
                    <FaRegFileAlt />
                    <div>
                        {category.name}
                    </div>
                </div>
                <div
                  className={`${styles.categoryChildren} ${isActiveOrHasActiveChild
                      ? styles.activeChildren
                      : ''}`}
                >
                    {category.children.map((child) => renderCategory(child))}
                </div>
            </div>
        );
    };

    if (areCategoriesLoading) {
        return <SpinningLoader />;
    }
    return (
        <div className={styles.contestCategoriesWrapper}>
            <div
              className={styles.contestCategoriesHeader}
              style={{
                  color: themeColors.textColor,
                  marginTop: isRenderedOnHomePage
                      ? 0
                      : 32,
              }}
              onClick={() => setIsExpanded(!isExpanded)}
            >
                <div>Contest Categories</div>
                {isExpanded
                    ? <FaAngleDown />
                    : <FaAngleUp />}
            </div>
            { categoriesError
                ? <div style={{ color: themeColors.textColor }}>Error loading categories</div>
                : (
                    <div
                      className={`${styles.contestCategoriesInnerWrapper} ${isExpanded
                          ? styles.show
                          : ''}`}
                      style={{ color: themeColors.textColor }}
                    >
                        {contestCategories?.map((contestCategory: IContestCategory) => renderCategory(contestCategory))}
                    </div>
                )}
        </div>
    );
};

export default ContestCetegories;
