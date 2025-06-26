
import { FaFileUpload } from 'react-icons/fa';
import { MdOutlineRemoveCircle } from 'react-icons/md';
import { Button, FormControl, FormGroup, IconButton, styled, Tooltip, Typography } from '@mui/material';

import { CLEAR_SELECTION } from '../../../../common/messages';

import styles from './FileUpload.module.scss';

interface IFileUploadProps {
    handleFileUpload: Function;
    propName: string;
    setSkipDownload: Function;
    showDownloadButton: boolean;
    uploadButtonName?: string;
    onClearSelectionClicked: Function;
    buttonLabel?: string;
    disableClearButton: boolean;
}

const VisuallyHiddenInput = styled('input')({
    clip: 'rect(0 0 0 0)',
    clipPath: 'inset(50%)',
    height: 1,
    overflow: 'hidden',
    position: 'absolute',
    bottom: 0,
    left: 0,
    whiteSpace: 'nowrap',
    width: 1,
});

const FileUpload = (props: IFileUploadProps) => {
    const {
        handleFileUpload,
        propName,
        setSkipDownload,
        showDownloadButton,
        uploadButtonName,
        onClearSelectionClicked,
        disableClearButton,
        buttonLabel = 'Upload',
    } = props;

    return (
        <FormGroup
          sx={{
              display: 'flex',
              flexDirection: 'row',
              alignItems: 'flex-start',
              justifyContent: 'flex-start',
              gap: '1rem',
              margin: '1rem 0',
          }}
        >
            <FormControl sx={{ display: 'flex', alignItems: 'start' }}>
                <Tooltip title={CLEAR_SELECTION}>
                    <span>
                        <IconButton onClick={() => onClearSelectionClicked(propName)} disabled={disableClearButton}>
                            <MdOutlineRemoveCircle color={disableClearButton
                                ? 'grey'
                                : 'red'}
                            />
                        </IconButton>
                    </span>
                </Tooltip>
            </FormControl>
            <FormControl className={styles.fileUpload} sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                <Button
                  sx={{ width: '200px' }}
                  component="label"
                  variant="contained"
                  startIcon={<FaFileUpload />}
                >
                    {buttonLabel}
                    <VisuallyHiddenInput type="file" onChange={(e) => handleFileUpload(e, propName)} />
                </Button>
                <Typography
                  variant="caption"
                  sx={{
                      minHeight: '1rem',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                      maxWidth: '200px',
                  }}
                >
                    {uploadButtonName || 'No file selected'}
                </Typography>
            </FormControl>
            {showDownloadButton &&
                <FormControl sx={{ display: 'flex', flexDirection: 'column', alignItems: 'start' }}>
                    <Button sx={{ width: '200px' }} variant="contained" onClick={() => setSkipDownload(false)}>
                        Download
                    </Button>
                </FormControl>
            }
        </FormGroup>
    );
};

export default FileUpload;
