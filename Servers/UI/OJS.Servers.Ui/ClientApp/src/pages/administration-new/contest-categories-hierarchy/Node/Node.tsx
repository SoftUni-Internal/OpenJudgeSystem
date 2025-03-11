/* eslint-disable @typescript-eslint/ban-types */
import React from 'react';
import { NodeRendererProps } from 'react-arborist';
import ArrowForwardIosIcon from '@mui/icons-material/ArrowForwardIos';
import BallotIcon from '@mui/icons-material/Ballot';
import { IconButton } from '@mui/material';

import { IContestCategoryHierarchy } from '../../../../common/types';

// eslint-disable-next-line css-modules/no-unused-class
import styles from '../AdministrationContestCategoriesHierarchy.module.scss';

type NodeProps = NodeRendererProps<IContestCategoryHierarchy> & {
    isActive: boolean;
    onContestsBulkEditClick: Function;
};

const Node = ({
    node,
    style,
    dragHandle,
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    tree,
    isActive,
    onContestsBulkEditClick,
}: NodeProps) => (
    <div
      className={styles.node}
      style={style}
      ref={dragHandle}
      onClick={() => {
          // A leaf node is a node ( category ) that does NOT have children
          if (!node.isLeaf) {
              node.toggle();
          }
      }}
    >
        <span className={styles.cardContainer}>
            <ArrowForwardIosIcon
              className={(node.children?.length ?? 0) <= 0 || node.isLeaf
                  ? styles.hidden
                  : `${styles.icon} ${
                      node.isOpen
                          ? styles.iconOpen
                          : styles.iconClosed
                  }`}
              fontSize="inherit"
            />
            <span className={`${styles.nodeName} ${isActive && styles.lightRed}`}>
                {node.data?.name}
            </span>
        </span>
        <IconButton
          className={styles.iconButton}
          onClick={(e) => {
              onContestsBulkEditClick(node.data?.id, node.data?.name);
              e.stopPropagation();
          }}
          disabled={node.data?.children.length !== 0}
        >
            <BallotIcon color={node.data?.children.length !== 0
                ? 'disabled'
                : 'primary'}
            />
        </IconButton>
    </div>
);

export default Node;
