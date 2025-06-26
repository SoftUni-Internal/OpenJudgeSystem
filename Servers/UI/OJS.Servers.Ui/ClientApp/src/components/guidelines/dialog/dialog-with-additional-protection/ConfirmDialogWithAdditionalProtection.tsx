import React, { useState } from 'react';
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, TextField, Typography } from '@mui/material';

import { CANCEL } from '../../../../common/labels';

import styles from '../ConfirmDialog.module.scss';

interface IConfirmDialogWithAdditionalProtectionProps {
    passWordToMatch: string;
    title?: string;
    text: string;
    confirmFunction? : () => void;
    declineFunction? : () => void;
    onClose? : () => void;
    confirmButtonText?: string;
}

const ConfirmDialogWithAdditionalProtection = (props: IConfirmDialogWithAdditionalProtectionProps) => {
    const { passWordToMatch, title, text, confirmFunction, declineFunction, onClose, confirmButtonText = 'Confirm' } = props;

    const [ passWord, setPassword ] = useState<string>('');

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

    const onChange = (pass: string) => {
        setPassword(pass);
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
                    <Typography color="error">
                        Type
                        {' '}
                        {passWordToMatch}
                        {' '}
                        to confirm action.
                    </Typography>
                    <TextField onChange={(e) => onChange(e.target.value)} />
                </DialogContent>
                <DialogActions>
                    <Button autoFocus onClick={handleDecline}>
                        {CANCEL}
                    </Button>
                    <Button color="error" onClick={handleConfirmation} autoFocus disabled={passWord !== passWordToMatch}>
                        {confirmButtonText}
                    </Button>
                </DialogActions>
            </Dialog>
        </div>
    );
};
export default ConfirmDialogWithAdditionalProtection;
