/* eslint-disable @typescript-eslint/ban-types */
import React, { ReactNode } from 'react';
import { IoMdClose } from 'react-icons/io';
import { Box, IconButton, Modal } from '@mui/material';

import concatClassNames from '../../../../../utils/class-names';

import styles from './AdministrationModal.module.scss';

interface IAdministrationModalProps {
    index: number;
    open: boolean;
    onClose: Function;
    children: ReactNode;
    className?: string;
}

enum ModalReasonsToClose {
EscapeKeyDown = 'escapeKeyDown',
BackDropClick = 'backdropClick'
}

const AdministrationModal = (props: IAdministrationModalProps) => {
    const { index, children, open, onClose, className } = props;

    const onModalClose = (event: object, reason: string) => {
        if (reason === ModalReasonsToClose.BackDropClick) {
            return;
        }
        if (onClose) {
            onClose();
        }
    };

    return (
        <Modal
          key={index}
          open={open}
          onClose={(event, reason) => onModalClose(event, reason)}
        >
            <Box className={concatClassNames(styles.wrapper, 'box-wrapper', className)}>
                <Box className={styles.closeIcon}>
                    <IconButton onClick={(event) => onModalClose(event, ModalReasonsToClose.EscapeKeyDown)}>
                        <IoMdClose />
                    </IconButton>
                </Box>
                {children}
            </Box>
        </Modal>
    );
};
export default AdministrationModal;
