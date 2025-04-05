import React, { useEffect, useState } from 'react';
import { Box, TextField, Typography } from '@mui/material';
import { IMentorPromptTemplateAdministrationModel } from 'src/common/types';
import SpinningLoader from 'src/components/guidelines/spinning-loader/SpinningLoader';
import useDelayedSuccessEffect from 'src/hooks/common/use-delayed-success-effect';
import useSuccessMessageEffect from 'src/hooks/common/use-success-message-effect';
import {
    useGetMentorPromptTemplateByIdQuery,
    useUpdateMentorPromptTemplateMutation,
} from 'src/redux/services/admin/mentorPromptTemplatesAdminService';
import { getAndSetExceptionMessage } from 'src/utils/messages-utils';
import { renderErrorMessagesAlert } from 'src/utils/render-utils';
import clearSuccessMessages from 'src/utils/success-messages-utils';

import AdministrationFormButtons from '../common/administration-form-buttons/AdministrationFormButtons';

import formStyles from '../common/styles/FormStyles.module.scss';

interface IMentorPromptTemplateEditProps {
    mentorPromptTemplateId?: number;
    currentMentorPromptTemplate?: IMentorPromptTemplateAdministrationModel;
    onSuccess?: Function;
    setParentSuccessMessage: Function;
}

const MentorPromptTemplateEdit = (props: IMentorPromptTemplateEditProps) => {
    const {
        mentorPromptTemplateId,
        currentMentorPromptTemplate,
        onSuccess,
        setParentSuccessMessage,
    } = props;

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ isValidForm, setIsValidForm ] = useState<boolean>(true);

    const [ mentorPromptTemplate, setMentorPromptTemplate ] = useState<IMentorPromptTemplateAdministrationModel>({
        id: 0,
        title: '',
        template: '',
        createdOn: new Date(),
        modifiedOne: new Date(),
    });

    const { data, isLoading } = useGetMentorPromptTemplateByIdQuery(
        mentorPromptTemplateId!,
        { skip: mentorPromptTemplateId === undefined },
    );

    const [
        updateMentorPromptTemplate,
        {
            data: updateData,
            isLoading: isUpdating,
            error: updateError,
            isSuccess: isSuccessfullyUpdated,
        },
    ] = useUpdateMentorPromptTemplateMutation();

    useSuccessMessageEffect({
        data: [ { message: updateData, shouldGet: isSuccessfullyUpdated } ],
        setParentSuccessMessage,
        clearFlags: [ isUpdating ],
    });

    useDelayedSuccessEffect({ isSuccess: isSuccessfullyUpdated, onSuccess });

    useEffect(() => {
        if (currentMentorPromptTemplate) {
            setMentorPromptTemplate(currentMentorPromptTemplate);
        }
    }, [ currentMentorPromptTemplate ]);

    useEffect(() => {
        if (data) {
            setMentorPromptTemplate(data);
        }
    }, [ data ]);

    useEffect(() => {
        getAndSetExceptionMessage([ updateError ], setErrorMessages);
        clearSuccessMessages({ setParentSuccessMessage });
    }, [ updateError, setParentSuccessMessage ]);

    const validateForm = () => {
        setIsValidForm(mentorPromptTemplate.title.trim().length > 0 && mentorPromptTemplate.template.trim().length > 0);
    };

    const onChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setMentorPromptTemplate((prevState) => ({
            ...prevState,
            [name]: value,
        }));
        validateForm();
    };

    const edit = () => {
        if (isValidForm) {
            updateMentorPromptTemplate(mentorPromptTemplate);
        }
    };

    if (isLoading || isUpdating) {
        return <SpinningLoader />;
    }

    return (
        <Box className={formStyles.form}>
            {renderErrorMessagesAlert(errorMessages)}
            <Typography className={formStyles.centralize} variant="h5">
                Mentor Prompt Template Form
            </Typography>
            <form className={formStyles.form}>
                <Box className={formStyles.fieldBox}>
                    <Typography className={formStyles.fieldBoxTitle} variant="h6">
                        General Information
                    </Typography>
                    <div className={formStyles.fieldBoxDivider} />
                    <Box className={formStyles.fieldBoxElement}>
                        <Box className={formStyles.row}>
                            <TextField
                              className={formStyles.inputRow}
                              type="text"
                              label="Title"
                              variant="standard"
                              name="title"
                              onChange={onChange}
                              value={mentorPromptTemplate.title}
                              InputLabelProps={{ shrink: true }}
                              error={mentorPromptTemplate.title.trim().length === 0}
                              helperText={
                                    mentorPromptTemplate.title.trim().length === 0
                                        ? 'Title is required.'
                                        : ''
                                }
                            />
                        </Box>
                        <Box className={formStyles.row}>
                            <TextField
                              className={formStyles.inputRow}
                              type="text"
                              label="Template"
                              variant="standard"
                              name="template"
                              onChange={onChange}
                              value={mentorPromptTemplate.template}
                              InputLabelProps={{ shrink: true }}
                              multiline
                              minRows={4}
                              error={mentorPromptTemplate.template.trim().length === 0}
                              helperText={
                                    mentorPromptTemplate.template.trim().length === 0
                                        ? 'Template content is required.'
                                        : ''
                                }
                            />
                        </Box>
                    </Box>
                </Box>
            </form>
            <AdministrationFormButtons
              isEditMode
              onEditClick={edit}
              disabled={!isValidForm}
            />
        </Box>
    );
};

export default MentorPromptTemplateEdit;
