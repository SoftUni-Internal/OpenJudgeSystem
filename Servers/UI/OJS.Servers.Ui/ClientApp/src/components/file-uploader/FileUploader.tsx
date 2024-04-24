import React, { useCallback, useEffect, useRef, useState } from 'react';
import { MdAttachFile } from 'react-icons/md';
import isNil from 'lodash/isNil';

import { FileValidationError } from '../../common/constants';
import { useSubmissions } from '../../hooks/submissions/use-submissions';
import { IErrorDataType } from '../../hooks/use-http';
import useTheme from '../../hooks/use-theme';
import Button, { ButtonSize, ButtonType } from '../guidelines/buttons/Button';

import styles from './FileUploader.module.scss';

interface IFileUploaderProps {
    file?: File | null;
    problemId?: number;
    allowedFileExtensions: string[];
    onInvalidFileExtension: (error: IErrorDataType) => void;
    onFileUpload: (file: File) => void;
}

const FileUploader = ({ file, problemId, allowedFileExtensions, onInvalidFileExtension, onFileUpload }: IFileUploaderProps) => {
    const { isDarkMode } = useTheme();
    const hiddenFileInput = useRef<HTMLInputElement | null>(null);
    const { actions: { updateSubmissionCode } } = useSubmissions();
    const [ internalFile, setInternalFile ] = useState<File | null>(null);
    const [ internalProblemId, setInternalProblemId ] = useState<number | null>(null);
    const { actions: { closeErrorMessage } } = useSubmissions();
    const handleClick = () => {
        hiddenFileInput.current?.click();
    };

    useEffect(
        () => {
            if (problemId !== internalProblemId && isNil(file)) {
                setInternalFile(null);
                return;
            }

            if (!isNil(file) && !isNil(problemId)) {
                setInternalFile(file);
                setInternalProblemId(problemId);
            }
        },
        [ file, internalFile, internalProblemId, problemId ],
    );

    const handleChange = useCallback(
        (event: any) => {
            const { target: { files: eventTarget } } = event;
            if (!eventTarget || isNil(problemId)) {
                return;
            }

            const uploadedFile = eventTarget[0];
            onFileUpload(uploadedFile);
            const extension = uploadedFile.name.split('.').pop();

            if (allowedFileExtensions && !allowedFileExtensions.includes(extension)) {
                onInvalidFileExtension({
                    detail: FileValidationError,
                    extensions: { Data: JSON.stringify({ ProblemId: problemId }) },
                } as unknown as IErrorDataType);
            } else {
                closeErrorMessage(problemId.toString());
            }

            updateSubmissionCode(eventTarget[0]);
            setInternalFile(eventTarget[0]);
            setInternalProblemId(problemId);

            // eslint-disable-next-line no-param-reassign
            event.target.value = null;
        },
        [ updateSubmissionCode, problemId, allowedFileExtensions, onInvalidFileExtension, closeErrorMessage, onFileUpload ],
    );

    return (
        <>
            <div className={styles.fileUploadContainer}>
                <Button
                  onClick={handleClick}
                  type={isDarkMode
                      ? ButtonType.lightNeutral
                      : ButtonType.darkNeutral}
                  size={ButtonSize.medium}
                >
                    Upload
                </Button>
            </div>
            <div className={styles.fileName}>
                {isNil(internalFile)
                    ? ''
                    : (
                        <div className={styles.uploadedFileWrapper}>
                            <MdAttachFile />
                            {internalFile.name}
                        </div>
                    )}
            </div>
            <input
              type="file"
              ref={hiddenFileInput}
              onChange={handleChange}
              style={{ display: 'none' }}
            />
        </>
    );
};

export default FileUploader;
