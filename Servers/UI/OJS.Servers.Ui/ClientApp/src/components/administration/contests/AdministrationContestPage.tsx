import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { ResourceType } from 'src/common/enums';
import ResourcesInProblemView
    from 'src/components/administration/problems/problem-resources-in-problem-view/ResourcesInProblemView';

import { ContestVariation } from '../../../common/contest-types';
import useScrollToTab from '../../../hooks/common/use-scroll-to-tab';
import { useGetContestActivityQuery, useGetContestByIdQuery } from '../../../redux/services/admin/contestsAdminService';
import { getAndSetExceptionMessage } from '../../../utils/messages-utils';
import { renderErrorMessagesAlert, renderSuccessfullAlert } from '../../../utils/render-utils';
import SpinningLoader from '../../guidelines/spinning-loader/SpinningLoader';
import TabsInView from '../common/tabs/TabsInView';

import ContestEdit from './contest-edit/ContestEdit';
import ParticipantsInContestView from './participants-in-contest-view/ParticipantsInContestView';
import ProblemsInContestView from './problems-in-contest-view/ProblemsInContestView';

enum CONTEST_LISTED_DATA {
    PROBLEMS = 'problems',
    PARTICIPANTS = 'participants',
    RESOURCES = 'resources'
}

const AdministrationContestPage = () => {
    const { pathname, hash } = useLocation();
    const [ , , , contestId ] = pathname.split('/');
    const [ tabName, setTabName ] = useState(CONTEST_LISTED_DATA.PROBLEMS);

    const [ errorMessages, setErrorMessages ] = useState<Array<string>>([]);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const onTabChange = (event: React.SyntheticEvent, newValue: CONTEST_LISTED_DATA) => {
        setTabName(newValue);
    };

    const { refetch: retake, data, isFetching, isLoading, error } = useGetContestByIdQuery({ id: Number(contestId) });

    const { data: activityData, error: activityError, isLoading: isLoadingActivity, isFetching: isFetchingActivity } =
        useGetContestActivityQuery(Number(contestId));

    useScrollToTab({ hash, tabName, setTabName, tabNames: Object.values(CONTEST_LISTED_DATA) });

    useEffect(() => {
        getAndSetExceptionMessage([ activityError, error ], setErrorMessages);
    }, [ activityError, error ]);

    const renderContestEdit = () => (
        <ContestEdit
          contestId={Number(contestId)}
          currentContest={data}
          onSuccess={retake}
          setParentSuccessMessage={setSuccessMessage}
          skipGettingContest
        />
    );

    const returnResourceInProblemView = (key:string) => (
        <div id={CONTEST_LISTED_DATA.RESOURCES}>
            <ResourcesInProblemView key={key} parentId={Number(contestId)} type={ResourceType.ContestResource} />
        </div>
    );

    const renderProblemsInContestView = (key: string) => (
        <div key={key} id={CONTEST_LISTED_DATA.PROBLEMS}>
            <ProblemsInContestView
              key={key}
              contestId={Number(contestId)}
              contestName={data?.name}
              contestType={ContestVariation[data?.type as keyof typeof ContestVariation]}
              canContestBeCompeted={activityData?.canBeCompeted || false}
            />
        </div>
    );

    const renderParticipantsInContestView = (key: string) => (
        <div key={key} id={CONTEST_LISTED_DATA.PARTICIPANTS}>
            <ParticipantsInContestView
              key={key}
              contestId={Number(contestId)}
              contestName={data!.name}
              contest={data!}
              setParentSuccessMessage={setSuccessMessage}
            />
        </div>
    );

    if (!successMessage && (isFetching || isLoading || isLoadingActivity || isFetchingActivity)) {
        return (<SpinningLoader />);
    }

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            {renderSuccessfullAlert(successMessage, 7000)}
            <TabsInView
              form={renderContestEdit}
              onTabChange={onTabChange}
              tabName={tabName}
              tabs={[
                  { value: CONTEST_LISTED_DATA.PROBLEMS, label: 'Problems', node: renderProblemsInContestView },
                  { value: CONTEST_LISTED_DATA.PARTICIPANTS, label: 'Participants', node: renderParticipantsInContestView },
                  { value: CONTEST_LISTED_DATA.RESOURCES, label: 'Resources', node: returnResourceInProblemView },
              ]}
            />
        </>
    );
};

export default AdministrationContestPage;
