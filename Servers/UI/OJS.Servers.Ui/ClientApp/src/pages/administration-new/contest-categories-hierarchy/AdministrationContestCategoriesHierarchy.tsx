import React, { useCallback, useEffect, useRef, useState } from 'react';
import { Tree } from 'react-arborist';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Typography } from '@mui/material';
import AdministrationModal from 'src/components/administration/common/modals/administration-modal/AdministrationModal';
import { useAdministrationTheme } from 'src/hooks/use-administration-theme-provider';
import ContestsBulkEdit from 'src/pages/administration-new/contest-categories/contests-bulk-edit/ContestsBulkEdit';
import { useAppSelector } from 'src/redux/store';
import { CONTESTS_BULK_EDIT } from 'src/utils/constants';

import { ThemeMode } from '../../../common/enums';
import {
    AdjacencyList,
    IContestCategory,
    IContestCategoryHierarchy,
    IContestCategoryHierarchyEdit,
} from '../../../common/types';
import ConfirmDialog from '../../../components/guidelines/dialog/ConfirmDialog';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import useSuccessMessageEffect from '../../../hooks/common/use-success-message-effect';
import {
    useEditContestCategoriesHierarchyMutation,
    useGetContestCategoriesHierarchyQuery,
} from '../../../redux/services/admin/contestCategoriesAdminService';
import { getAndSetExceptionMessage } from '../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../utils/render-utils';
import clearSuccessMessages from '../../../utils/success-messages-utils';

import Node from './Node/Node';
import ResizableContainer from './ResizeableContainer/ResizableContainer';

import styles from './AdministrationContestCategoriesHierarchy.module.scss';

const AdministrationContestCategoriesHierarchy = () => {
    const [ searchParams ] = useSearchParams();
    const navigate = useNavigate();

    const {
        refetch: refetchInitialContestCategories,
        data: initialContestCategories,
        isLoading: areCategoriesLoading,
        isFetching: areCategoriesFetching,
        error: categoriesError,
    } = useGetContestCategoriesHierarchyQuery();

    const [ editContestCategoriesHierarchy, {
        reset,
        data: updateData,
        isSuccess: isSuccessfullyUpdated,
        isLoading: isUpdatingHierarchy,
        isError: updateError,
    } ] = useEditContestCategoriesHierarchyMutation();

    const { contestCategories: contestCategoriesData } = useAppSelector((state) => state.contests);
    const [ successMessage, setSuccessMessage ] = useState <string | null>(null);
    const [ errorMessages, setErrorMessages ] = useState<string[]>([]);
    const [ contestCategories, setContestCategories ] = useState<IContestCategoryHierarchy[]>([]);
    const [ showConfirmSave, setShowConfirmSave ] = useState<boolean>(false);
    const [ showConfirmClear, setShowConfirmClear ] = useState<boolean>(false);
    const [ showContestsBulkEditModal, setShowContestsBulkEditModal ] = useState<boolean>(false);
    const [ contestCategoryId, setContestCategoryId ] = useState<number>();
    const [ contestCategoryName, setContestCategoryName ] = useState<string>();

    // A collection to store the initial state of all nodes
    const initialCategoriesAdjacencyList = useRef<AdjacencyList<string, IContestCategoryHierarchy>>({});
    // A collection to store the updated nodes
    const updatedCategoriesAdjacencyList = useRef<AdjacencyList<string, IContestCategoryHierarchy>>({});

    const { themeMode } = useAdministrationTheme();

    useSuccessMessageEffect({
        data: [ { message: updateData, shouldGet: isSuccessfullyUpdated } ],
        setSuccessMessage,
        clearFlags: [ isUpdatingHierarchy ],
    });

    useEffect(() => {
        getAndSetExceptionMessage([ updateError, categoriesError ], setErrorMessages);
        clearSuccessMessages({ setSuccessMessage });
    }, [ updateError, categoriesError ]);

    useEffect(() => {
        if (isSuccessfullyUpdated) {
            reset();
            refetchInitialContestCategories();
        }
    }, [ isSuccessfullyUpdated, refetchInitialContestCategories, reset ]);

    // A standard DFS implementation
    const depthFirstSearch = useCallback((node: IContestCategoryHierarchy) => {
        // If the node has already been traversed, exit the current function call
        if (initialCategoriesAdjacencyList.current[node.id]) {
            return;
        }

        // If the node has not been traversed, add it to the collection
        initialCategoriesAdjacencyList.current[node.id] = node;

        // Call DFS for each of the current node's children
        node?.children.forEach(depthFirstSearch);
    }, []);

    useEffect(() => {
        // If the categories have been fetched, save their initial state
        if (initialContestCategories && !areCategoriesLoading && !areCategoriesFetching) {
            setContestCategories(initialContestCategories);

            // For each of the parent nodes ( main categories / nodes at level 0 ) - run DFS
            initialCategoriesAdjacencyList.current = {};
            initialContestCategories.forEach(depthFirstSearch);
        }
    }, [ depthFirstSearch, initialContestCategories, areCategoriesFetching, areCategoriesLoading ]);

    const onContestsBulkEditClick = (id: number, name: string) => {
        setShowContestsBulkEditModal(true);
        setContestCategoryId(id);
        setContestCategoryName(name);
    };

    const renderContestsBulkEditModal = () =>
        <AdministrationModal
          index={1}
          open={showContestsBulkEditModal}
          onClose={() => setShowContestsBulkEditModal(false)}
          className={styles.administrationModal}
        >
            <ContestsBulkEdit
              categoryId={contestCategoryId}
              categoryName={contestCategoryName}
              setParentSuccessMessage={setSuccessMessage}
              onSuccess={() => setShowContestsBulkEditModal(false)}
            />
        </AdministrationModal>
    ;

    // A recursive function to find a given node
    const findNode = useCallback(
        // eslint-disable-next-line max-len
        (nodes: IContestCategoryHierarchy[] | IContestCategory[], id: string): IContestCategoryHierarchy | IContestCategory | null => nodes.reduce<IContestCategoryHierarchy | IContestCategory | null>((found, node) => {
        // If the node has already been found, skip further checks
            if (found) {
                return found;
            }

            // We have found the node, return it
            if (node.id.toString() === id) {
            // This will be the value of 'found'
                return node;
            }

            // If we have not yet found the node, search through each of the nodes' children
            return node.children
                ? findNode(node.children, id)
                : null;
        }, null),
        [],
    );

    useEffect(() => {
        const categoryId = searchParams.get(CONTESTS_BULK_EDIT);
        if (categoryId) {
            onContestsBulkEditClick(
                Number(categoryId),
                findNode(contestCategoriesData, categoryId)?.name ?? '',
            );

            const newParams = new URLSearchParams(searchParams);
            newParams.delete(CONTESTS_BULK_EDIT);
            navigate({ search: newParams.toString() }, { replace: true });
        }
    }, [ contestCategoriesData, findNode, navigate, searchParams ]);

    const removeNode = (nodes: IContestCategoryHierarchy[], id: string): IContestCategoryHierarchy | null => {
        // In case the node is within the main categories ( nodes at level 0 )
        const index = nodes.findIndex((node) => node.id.toString() === id);
        if (index !== -1) {
            return nodes.splice(index, 1)[0];
        }

        // The node must be within a subtree, iterate through each subtree, find it and remove it
        let removed: IContestCategoryHierarchy | null = null;
        nodes.forEach((node) => {
            if (node.children) {
                const childRemoved = removeNode(node.children, id);
                if (childRemoved) {
                    removed = childRemoved;
                }
            }
        });

        return removed;
    };

    // A recursive function to clone the tree
    const cloneTree = (nodes: IContestCategoryHierarchy[]): IContestCategoryHierarchy[] => nodes.map((node) => ({
        ...node,
        children: node.children
            ? cloneTree(node.children)
            : [],
    }));

    const addOrUpdateNode = (newMovedNode: IContestCategoryHierarchy, nodeValue: number | undefined) => {
        // Manage the collection of updated nodes
        // If the node does not exist in the collection, add it
        if (!updatedCategoriesAdjacencyList.current[newMovedNode.id]) {
            updatedCategoriesAdjacencyList.current[newMovedNode.id] = newMovedNode;
        } else {
            // If it already exists, update its 'parentId'
            updatedCategoriesAdjacencyList.current[newMovedNode.id].parentId = nodeValue;
        }
    };

    const handleMove = ({
        dragIds,
        parentId,
        index,
    }: {
        /*
            The option to select multiple nodes is disabled,
            so 'dragIds.length' should be 1
         */
        dragIds: string[];
        parentId: string | null;
        index: number;
    }) => {
        setContestCategories((prevCategories) => {
            // Create a clone of the tree ( we cannot work with immutable data )
            const newCategories = cloneTree(prevCategories);

            // Iterate through all the nodes that have been moved and remove them from the tree
            const movedNodes = dragIds
                .map((id) => removeNode(newCategories, id))
                .filter((item): item is IContestCategoryHierarchy => !!item);

            // If we do not have a 'parentId', this is a new main node, insert it at level 0
            if (!parentId) {
                movedNodes.forEach((mn, idx) => {
                    // This node will not have a parent, set the 'parentId' to 'undefined'
                    const newMovedNode = { ...mn, parentId: undefined };
                    movedNodes[idx] = newMovedNode;

                    // If the node's parent has not changed, do not mark it as updated
                    if (!mn.parentId) {
                        return;
                    }

                    /*
                        The node has returned to its initial state ( location ),
                        remove it from the adjacency list
                     */
                    if (!initialCategoriesAdjacencyList.current[mn.id]?.parentId) {
                        delete updatedCategoriesAdjacencyList.current[mn.id];
                        return;
                    }

                    addOrUpdateNode(newMovedNode, Number(parentId));
                });
                // Insert the moved nodes within the tree
                newCategories.splice(index, 0, ...movedNodes);
            } else {
                // If there is a parent to which we have added the node, find it
                const parent = findNode(newCategories, parentId.toString());
                if (parent?.children) {
                    movedNodes.forEach((mn, idx) => {
                        // Set the node's new 'parentId'
                        const newMovedNode = { ...mn, parentId: Number(parentId) };
                        movedNodes[idx] = newMovedNode;

                        // If the node's parent has not changed, do not mark it as updated
                        // eslint-disable-next-line no-constant-binary-expression
                        if (mn.parentId === (Number(parentId) ?? undefined)) {
                            return;
                        }

                        /*
                            The node has returned to its initial state ( location ),
                            remove it from the adjacency list
                        */
                        if (initialCategoriesAdjacencyList.current[mn.id]?.parentId === Number(parentId)) {
                            delete updatedCategoriesAdjacencyList.current[mn.id];
                            return;
                        }

                        addOrUpdateNode(newMovedNode, Number(parentId));
                    });
                    // Insert the moved nodes within the tree
                    parent.children.splice(index, 0, ...movedNodes);
                }
            }

            return newCategories;
        });
    };

    const onSave = () => {
        // Compare the initial nodes' state to their updated state
        Object.keys(updatedCategoriesAdjacencyList.current).forEach((key) => {
            const id = Number(key);
            const updatedNode = updatedCategoriesAdjacencyList.current[id];
            const initialNode = initialCategoriesAdjacencyList.current[updatedNode.id];

            /*
                If the node's updated state is the same as its initial state
                ( its parentId remained the same ), remove it from the collection
             */
            if (initialNode.parentId === updatedNode.parentId || !initialNode.parentId && !updatedNode.parentId) {
                delete updatedCategoriesAdjacencyList.current[id];
            }
        });

        // Map the remaining nodes to an adjacency list ( dictionary )
        const finalCategories = Object.keys(updatedCategoriesAdjacencyList.current)
            .reduce((acc, key) => {
                const id = Number(key);
                // Retrieve the node's updated state
                const updatedNode = updatedCategoriesAdjacencyList.current[id];
                // Retrieve the node's initial state
                const initialNode = initialCategoriesAdjacencyList.current[updatedNode.id];

                // Validate the node's state
                const hasNewParentId = initialNode.parentId !== updatedNode.parentId;

                if (hasNewParentId) {
                    // If its state is valid, add it to the adjacency list
                    acc[id] = { id, parentId: updatedNode.parentId };
                }

                return acc;
            }, {} as AdjacencyList<number, IContestCategoryHierarchyEdit>);

        // Edit the hierarchy
        editContestCategoriesHierarchy(finalCategories);
        // Clear the collection of nodes to be updated
        updatedCategoriesAdjacencyList.current = {};
    };

    const onClear = () => {
        updatedCategoriesAdjacencyList.current = {};
        refetchInitialContestCategories();
    };

    const onSaveClick = () => {
        setShowConfirmSave(!showConfirmSave);
    };

    const onClearClick = () => {
        clearSuccessMessages({ setSuccessMessage });
        setShowConfirmClear(!showConfirmClear);
    };

    if (areCategoriesLoading || areCategoriesFetching) {
        return (
            <div className={styles.loaderContainer}>
                <SpinningLoader />
            </div>
        );
    }

    if (!contestCategories || contestCategories.length === 0) {
        return (
            <div className={styles.notFoundContainer}>
                <Typography variant="h4">No categories have been found.</Typography>
            </div>
        );
    }

    return (
        <ResizableContainer
          onSaveButtonClick={onSaveClick}
          onClearButtonClick={onClearClick}
          isButtonDisabled={Object.entries(updatedCategoriesAdjacencyList.current).length === 0}
        >
            {({ width, height }) =>
                <div
                  className={`${styles.treeContainer} ${themeMode === ThemeMode.Dark
                      ? styles.darkTheme
                      : styles.lightTheme}`}
                  style={{
                      width,
                      height,
                  }}
                >
                    {renderSuccessfullAlert(successMessage)}
                    {renderErrorMessagesAlert(errorMessages)}
                    <Tree<IContestCategoryHierarchy>
                      data={contestCategories}
                      openByDefault={false}
                      width={width}
                      height={height}
                      disableMultiSelection
                      className={styles.tree}
                      rowHeight={28}
                      /*
                        Our data is using 'number' ids, but the tree works only with 'string' ids,
                        this way we can tell it to convert them to 'string' whenever a node
                        is accessed.
                       */
                      idAccessor={(node) => node.id.toString()}
                      onMove={handleMove}
                      selectionFollowsFocus={undefined}
                    >
                        {({
                            node,
                            style,
                            dragHandle,
                            tree,
                        }) => Node({
                            node,
                            style,
                            dragHandle,
                            tree,
                            isActive: !!updatedCategoriesAdjacencyList.current[node.id],
                            onContestsBulkEditClick,
                        })}
                    </Tree>
                    {showConfirmSave &&
                        <ConfirmDialog
                          title="Save Contest Categories Hierarchy Changes"
                          text="Are you sure you want to save the changes made to the Contest Categories Hierarchy?"
                          confirmButtonText="Save"
                          declineButtonText="Cancel"
                          onClose={() => setShowConfirmSave(!showConfirmSave)}
                          confirmFunction={() => onSave()}
                        />
                    }
                    {showConfirmClear &&
                        <ConfirmDialog
                          title="Clear Contest Categories Hierarchy Changes"
                          text="Are you sure you want to clear the changes made to the Contest Categories Hierarchy?"
                          confirmButtonText="Clear"
                          declineButtonText="Cancel"
                          onClose={() => setShowConfirmClear(!showConfirmClear)}
                          confirmFunction={() => onClear()}
                        />
                    }
                    {showContestsBulkEditModal && renderContestsBulkEditModal()}
                </div>
            }
        </ResizableContainer>
    );
};

export default AdministrationContestCategoriesHierarchy;
