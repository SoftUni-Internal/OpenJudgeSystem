/* eslint-disable @typescript-eslint/no-use-before-define */
/* eslint-disable jsx-a11y/no-static-element-interactions */
/* eslint-disable jsx-a11y/click-events-have-key-events */
/* eslint-disable no-restricted-syntax */
/* eslint-disable consistent-return */
import { SyntheticEvent, useEffect, useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import { SimpleTreeView } from '@mui/x-tree-view/SimpleTreeView';
import { TreeItem2 } from '@mui/x-tree-view/TreeItem2';

import { IContestCategory } from '../../../common/types';
import { getAllContestsPageUrl } from '../../../common/urls/compose-client-urls';
import useTheme from '../../../hooks/use-theme';
import {
    setContestCategories,
    setContestCategory,
    updateContestCategoryBreadcrumbItem,
} from '../../../redux/features/contestsSlice';
import { useGetContestCategoriesQuery } from '../../../redux/services/contestsService';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import concatClassNames from '../../../utils/class-names';
import { LinkButton, LinkButtonType } from '../../guidelines/buttons/Button';
import SpinningLoader from '../../guidelines/spinning-loader/SpinningLoader';

import styles from './ContestCategories.module.scss';

interface IContestCategoriesProps {
    isRenderedOnHomePage?: boolean;
}

const ContestCategories = (props: IContestCategoriesProps) => {
    const { isRenderedOnHomePage = false } = props;
    const [ expandedItems, setExpandedItems ] = useState<string[]>([]);
    const { breadcrumbItems } = useAppSelector((state) => state.contests);

    const dispatch = useAppDispatch();
    const { categoryId } = useParams();
    const { themeColors, getColorClassName, isDarkMode } = useTheme();

    const textColorClassName = getColorClassName(themeColors.textColor);

    const {
        data: contestCategories,
        isLoading: areCategoriesLoading,
        error: categoriesError,
    } = useGetContestCategoriesQuery();

    const selectedId = useMemo(() => Number(categoryId), [ categoryId ]);

    useEffect(() => {
        dispatch(setContestCategories({ contestCategories: contestCategories || [] }));
    }, [ contestCategories, dispatch ]);

    useEffect(() => {
        const category = findContestCategoryByIdRecursive(contestCategories, selectedId);
        const breadcrumbCategories = findParentNames(contestCategories, selectedId);

        dispatch(setContestCategory(category));
        dispatch(updateContestCategoryBreadcrumbItem({ elements: breadcrumbCategories }));
    }, [ selectedId, contestCategories, dispatch ]);

    useEffect(() => {
        const breadcrumbItemIds = breadcrumbItems.map((item) => item.id.toString());

        if (breadcrumbItemIds) {
            setExpandedItems((prev) => {
                // Create a unique Set with previous open categories and the breadcrumb categories,
                // to prevent already open categories from closing and to open the breadcrumb ones.
                const updatedItems = new Set([ ...prev, ...breadcrumbItemIds ]);
                return Array.from(updatedItems);
            });
        }
    }, [ breadcrumbItems ]);

    useEffect(() => {
        // If no category is selected, we want to collapse all categories
        if (!categoryId) {
            setExpandedItems([]);
        }
    }, [ categoryId ]);

    const handleExpandedItemsChange = (
        event: SyntheticEvent,
        itemIds: string[],
    ) => {
        setExpandedItems(itemIds);
    };

    const renderCategory = (category: IContestCategory, isChildElement = false) => {
        const categoryItemClassNames = concatClassNames(
            styles.categoryItem,
            selectedId === category.id
                ? styles.selectedCategory
                : '',
            isChildElement
                ? styles.childCategoryItem
                : '',
            isDarkMode
                ? styles.darkCategoryItem
                : '',
            category.parentId === null
                ? styles.mainTreeCategoryItem
                : '',
        );

        const categoryListItemClassNames = concatClassNames(
            styles.categoryListItem,
            isDarkMode
                ? styles.darkCategoryListItem
                : styles.lightCategoryListItem,
            isChildElement
                ? styles.isChild
                : '',
        );

        const categoryListItemContentClassNames = concatClassNames(styles.categoryListItemContent);

        if (category.children.length > 0) {
            return (
                <TreeItem2
                  itemId={`${category.id}`}
                  key={category.id}
                  label={category.name}
                  className={categoryListItemClassNames}
                  classes={{ content: categoryListItemContentClassNames }}
                >
                    {category.children.map((child) => renderCategory(child, true))}
                </TreeItem2>
            );
        }

        // Most inner category is the link that will load the contests.
        return (
            <LinkButton
              to={getAllContestsPageUrl({ categoryName: category.name, categoryId: category.id })}
              type={LinkButtonType.plain}
              preventScrollReset
              className={categoryItemClassNames}
              text={category.name}
              key={category.id.toString()}
            />
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
            {categoriesError
                ? <div className={textColorClassName}>Error loading categories</div>
                : (
                    <div
                      className={`${styles.contestCategoriesInnerWrapper} ${textColorClassName}`}
                    >
                        <Box>
                            <SimpleTreeView
                              expandedItems={expandedItems}
                              onExpandedItemsChange={handleExpandedItemsChange}
                            >
                                {contestCategories?.map((contestCategory: IContestCategory) => renderCategory(contestCategory))}
                            </SimpleTreeView>
                        </Box>
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
    ): Array<{ name: string; id: number }> | null => {
        if (element.id === targetId) {
            return [ ...parents, { name: element.name, id: element.id } ];
        }

        for (const child of element.children || []) {
            const result = findParentsRecursive(child, targetId, [ ...parents, {
                name: element.name,
                id: element.id,
            } ]);

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

export { ContestCategories, findContestCategoryByIdRecursive, findParentNames };
