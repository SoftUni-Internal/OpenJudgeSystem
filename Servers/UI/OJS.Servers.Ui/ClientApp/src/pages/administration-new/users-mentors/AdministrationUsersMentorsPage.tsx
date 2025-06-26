import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import AdministrationModal from 'src/components/administration/common/modals/administration-modal/AdministrationModal';
import UserMentorEdit from 'src/components/administration/users-mentors/UserMentorEdit';
import usersMentorsFilterableColumns, {
    returnNonFilterableColumns,
} from 'src/pages/administration-new/users-mentors/usersMentorsGridColumns';
import { useGetAllUsersMentorsQuery } from 'src/redux/services/admin/usersMentorsAdminService';
import { getAndSetExceptionMessage } from 'src/utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from 'src/utils/render-utils';

import { IGetAllAdminParams } from '../../../common/types';
import SpinningLoader from '../../../components/guidelines/spinning-loader/SpinningLoader';
import { applyDefaultFilterToQueryString } from '../administration-filters/AdministrationFilters';
import AdministrationGridView from '../AdministrationGridView';

const AdministrationUsersMentorsPage = () => {
    const [ searchParams ] = useSearchParams();

    const [ queryParams, setQueryParams ] = useState<IGetAllAdminParams>(applyDefaultFilterToQueryString('', '', searchParams));
    const [ openUserMentorEditModal, setOpenUserMentorEditModal ] = useState<boolean>(false);
    const [ userMentorId, setUserMentorId ] = useState<string>();
    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const {
        refetch: refetchUsersMentors,
        data: usersMentors,
        isLoading: isDataLoading,
        error,
    } = useGetAllUsersMentorsQuery(queryParams);

    useEffect(() => {
        getAndSetExceptionMessage([ error ], setErrorMessages);
    }, [ error ]);

    if (isDataLoading) {
        return <SpinningLoader />;
    }

    const onSuccessfulEdit = () => {
        setOpenUserMentorEditModal(false);
        refetchUsersMentors();
    };

    const onEditClick = (id: string) => {
        setOpenUserMentorEditModal(true);
        setUserMentorId(id);
    };

    const renderContestModal = (index: number) => 
        <AdministrationModal
          key={index}
          index={index}
          open={openUserMentorEditModal}
          onClose={() => setOpenUserMentorEditModal(false)}
        >
            <UserMentorEdit
              userMentorId={userMentorId}
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
              filterableGridColumnDef={usersMentorsFilterableColumns}
              notFilterableGridColumnDef={returnNonFilterableColumns(onEditClick)}
              modals={[
                  { showModal: openUserMentorEditModal, modal: (i) => renderContestModal(i) },
              ]}
              data={usersMentors}
              error={error}
              queryParams={queryParams}
              setQueryParams={setQueryParams}
              defaultSorter=""
              defaultFilter=""
            />
        </>
    );
};

export default AdministrationUsersMentorsPage;
