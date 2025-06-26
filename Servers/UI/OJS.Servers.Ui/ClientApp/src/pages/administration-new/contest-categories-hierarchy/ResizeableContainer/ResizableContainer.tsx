
import React, { ReactElement, useEffect } from 'react';
import { Box, Button, Typography } from '@mui/material';
import useResizeObserver from 'use-resize-observer';

import styles from './ResizableContainer.module.scss';

interface IResizableContainerProps {
    onSaveButtonClick: Function;
    onClearButtonClick: Function;
    isButtonDisabled: boolean;
    children: (dimensions: { width: number; height: number }) => ReactElement;
}

const ResizableContainer = (props: IResizableContainerProps) => {
    const { onSaveButtonClick, onClearButtonClick, isButtonDisabled, children } = props;
    // Used to properly resize the node's container ( when a dropdown is expanded, etc... )
    const { ref: resizeRef, width, height } = useResizeObserver();

    // Used to hide the parent's scrollbar
    useEffect(() => {
        document.body.style.overflow = 'hidden';

        return () => {
            document.body.style.overflow = 'auto';
        };
    }, [ ]);

    return (
        <>
            <div className={styles.cardContainer}>
                <Typography variant="h3" align="center" className={styles.title}>
                    Contest Categories Hierarchy
                </Typography>
                <div className={styles.buttonsContainer}>
                    <Box className={styles.buttonWrapper}>
                        <Button
                          variant="contained"
                          color="primary"
                          className={styles.button}
                          onClick={() => onSaveButtonClick()}
                          disabled={isButtonDisabled}
                        >
                            Save
                        </Button>
                    </Box>
                    <Box className={styles.buttonWrapper}>
                        <Button
                          variant="contained"
                          color="error"
                          className={styles.button}
                          onClick={() => onClearButtonClick()}
                          disabled={isButtonDisabled}
                        >
                            Clear
                        </Button>
                    </Box>
                </div>
            </div>
            <div className={styles.container} ref={resizeRef}>
                {width && height
                    ? children({ width, height })
                    : null}
            </div>
        </>
    );
};

export default ResizableContainer;
