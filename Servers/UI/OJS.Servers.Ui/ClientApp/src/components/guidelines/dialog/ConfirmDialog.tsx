import { ReactElement } from 'react';
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@mui/material';

import styles from './ConfirmDialog.module.scss';

interface IConfirmDialogProps {
    title?: string;
    text: string | ReactElement;
    confirmFunction?: () => void;
    declineFunction?: () => void;
    onClose?: () => void;
    confirmButtonText?: string;
    declineButtonText?: string;
    confirmButtonColor?: string;
    declineButtonColor?: string;
}

const ConfirmDialog = (props: IConfirmDialogProps) => {
    const {
        title,
        text,
        confirmFunction,
        declineFunction,
        onClose,
        confirmButtonText,
        declineButtonText,
        confirmButtonColor,
        declineButtonColor,
    } = props;

    const handleClose = () => {
        if (onClose) {
            onClose();
        }
    };

    const handleConfirmation = () => {
        if (confirmFunction) {
            confirmFunction();
        }
        handleClose();
    };

    const handleDecline = () => {
        if (declineFunction) {
            declineFunction();
        }
        handleClose();
    };

    return (
        <div className={styles.position}>
            <Dialog
              open
              onClose={handleClose}
              aria-labelledby="responsive-dialog-title"
            >
                {title && 
                    <DialogTitle id="responsive-dialog-title">
                        {title}
                    </DialogTitle>
                }
                <DialogContent>
                    <DialogContentText>
                        {text}
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    {declineButtonText && 
                        <Button
                          autoFocus
                          onClick={handleDecline}
                          className={declineButtonColor ?? styles.confirm}
                        >
                            {declineButtonText}
                        </Button>
                    }
                    {confirmButtonText && 
                        <Button
                          autoFocus
                          onClick={handleConfirmation}
                          className={confirmButtonColor ?? styles.decline}
                        >
                            {confirmButtonText}
                        </Button>
                    }
                </DialogActions>
            </Dialog>
        </div>
    );
};

export default ConfirmDialog;
