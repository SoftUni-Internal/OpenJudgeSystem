import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import AdministrationModal from 'src/components/administration/common/modals/administration-modal/AdministrationModal';
import MentorPromptTemplateEdit from 'src/components/administration/mentor-prompt-templates/MentorPromptTemplateEdit';
import mentorPromptTemplatesFilterableColumns
, { returnNonFilterableColumns } from 'src/pages/administration-new/mentor-prompt-templates/mentorPromptTemplates';
import { useGetAllMentorPromptTemplatesQuery } from 'src/redux/services/admin/mentorPromptTemplatesAdminService';
import { getAndSetExceptionMessage } from 'src/utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from 'src/utils/render-utils';

import { IGetAllAdminParams } from '../../../common/types';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView from '../AdministrationGridView';

const AdministrationMentorPromptTemplatesPage = () => {
    const [ searchParams ] = useSearchParams();

    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', '', searchParams));
    const [ openMentorPromptTemplateEditModal, setOpenMentorPromptTemplateEditModal ] = useState<boolean>(false);
    const [ mentorPromptTemplateId, setMentorPromptTemplateId ] = useState<number>();
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const {
        refetch: refetchMentorPromptTemplates,
        data: mentorPromptTemplates,
        isLoading: isDataLoading,
        error,
    } = useGetAllMentorPromptTemplatesQuery(queryParams);

    useEffect(() => {
        getAndSetExceptionMessage([ error ], setErrorMessages);
    }, [ error ]);

    if (isDataLoading) {
        return <SpinningLoader />;
    }

    const onSuccessfulEdit = () => {
        setOpenMentorPromptTemplateEditModal(false);
        refetchMentorPromptTemplates();
    };

    const onEditClick = (id: number) => {
        setOpenMentorPromptTemplateEditModal(true);
        setMentorPromptTemplateId(id);
    };

    const renderContestModal = (index: number) => 
        <AdministrationModal
          key={index}
          index={index}
          open={openMentorPromptTemplateEditModal}
          onClose={() => setOpenMentorPromptTemplateEditModal(false)}
        >
            <MentorPromptTemplateEdit
              mentorPromptTemplateId={mentorPromptTemplateId}
              onSuccess={() => onSuccessfulEdit()}
              setParentSuccessMessage={setSuccessMessage}
            />
        </AdministrationModal>
    ;

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            {renderErrorMessagesAlert(errorMessages)}
            <AdministrationGridView
              filterableGridColumnDef={mentorPromptTemplatesFilterableColumns}
              notFilterableGridColumnDef={returnNonFilterableColumns(onEditClick)}
              modals={[
                  { showModal: openMentorPromptTemplateEditModal, modal: (i) => renderContestModal(i) },
              ]}
              data={mentorPromptTemplates}
              error={error}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              defaultSorter=""
              defaultFilter=""
            />
        </>
    );
};

export default AdministrationMentorPromptTemplatesPage;
