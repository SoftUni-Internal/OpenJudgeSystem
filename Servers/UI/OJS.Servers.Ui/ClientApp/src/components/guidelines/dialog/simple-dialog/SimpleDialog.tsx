import React, { ReactNode } from 'react';
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@mui/material';
import { CANCEL } from 'src/common/labels';

import styles from 'src/components/guidelines/dialog/ConfirmDialog.module.scss';

interface ISimpleDialogProps {
    title: string;
    content: string | ReactNode;
    declineFunction: () => void;
}

const SimpleDialog = ({ title, content, declineFunction }: ISimpleDialogProps) =>
    <div className={styles.position}>
        <Dialog
          open
          onClose={() => {
          }}
          aria-labelledby="responsive-dialog-title"
        >
            <DialogTitle id="responsive-dialog-title">
                {title}
            </DialogTitle>
            <DialogContent>
                { React.isValidElement(content)
                    ? content
                    : <DialogContentText>
                        {content}
                    </DialogContentText>
                    }
            </DialogContent>
            <DialogActions>
                <Button
                  autoFocus
                  onClick={() => declineFunction()}
                  className={styles.decline}
                >
                    {CANCEL}
                </Button>
            </DialogActions>
        </Dialog>
    </div>
;

export default SimpleDialog;
