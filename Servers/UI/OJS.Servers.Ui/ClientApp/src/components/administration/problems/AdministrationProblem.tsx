import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router';
import { ResourceType } from 'src/common/enums';

import useScrollToTab from '../../../hooks/common/use-scroll-to-tab';
import { useGetContestActivityQuery } from '../../../redux/services/admin/contestsAdminService';
import { getAndSetExceptionMessage } from '../../../utils/messages-utils';
import { renderErrorMessagesAlert } from '../../../utils/render-utils';
import TabsInView from '../common/tabs/TabsInView';

import ProblemForm from './problem-form/ProblemForm';
import ResourcesInView from './problem-resources-in-problem-view/ResourcesInView';
import TestsInProblemView from './tests-in-problem-view/TestsInProblemView';

enum PROBLEM_LISTED_DATA {
    TESTS = 'tests',
    RESOURCES = 'resources'
}
const AdministrationProblem = () => {
    const { pathname, hash } = useLocation();
    const [ , , , problemId ] = pathname.split('/');
    const [ tabName, setTabName ] = useState(PROBLEM_LISTED_DATA.RESOURCES);
    const [ problemName, setProblemName ] = useState<string>('');
    const [ errorMessages, setErrorMessages ] = useState <Array<string>>([]);
    const [ contestId, setContestId ] = useState<number>(0);
    const [ skipGettingContestActivity, setSkipGettingContestActivity ] = useState<boolean>(true);

    const { refetch, data: activityData, error: activityError } =
    useGetContestActivityQuery(Number(contestId), { skip: skipGettingContestActivity });

    const onTabChange = (event: React.SyntheticEvent, newValue: PROBLEM_LISTED_DATA) => {
        setTabName(newValue);
    };

    useScrollToTab({ hash, tabName, setTabName, tabNames: Object.values(PROBLEM_LISTED_DATA) });

    useEffect(() => {
        getAndSetExceptionMessage([ activityError ], setErrorMessages);
    }, [ activityError ]);

    useEffect(() => {
        if (!skipGettingContestActivity) {
            refetch();
        }
    }, [ contestId, refetch, skipGettingContestActivity ]);

    const returnProblemForm = () => 
        <ProblemForm
          problemId={Number(problemId)}
          isEditMode
          contestId={null}
          contestName={null}
          getName={(name: string) => setProblemName(name)}
          getContestId={(id: number) => {
              setContestId(id);
              setSkipGettingContestActivity(false);
          }}
        />
    ;

    const returnResourceInProblemView = (key:string) => 
        <div id={PROBLEM_LISTED_DATA.RESOURCES}>
            <ResourcesInView key={key} parentId={Number(problemId)} type={ResourceType.ProblemResource} />
        </div>
    ;

    const returnTests = (key: string) => 
        <div id={PROBLEM_LISTED_DATA.TESTS}>
            <TestsInProblemView
              key={key}
              problemId={Number(problemId)}
              problemName={problemName}
              canBeCompeted={activityData?.canBeCompeted ?? false}
              contestId={contestId}
            />
        </div>
    ;

    return (
        <>
            {renderErrorMessagesAlert(errorMessages)}
            <TabsInView
              form={returnProblemForm}
              onTabChange={onTabChange}
              tabName={tabName}
              tabs={[
                  { value: PROBLEM_LISTED_DATA.RESOURCES, label: 'Resources', node: returnResourceInProblemView },
                  { value: PROBLEM_LISTED_DATA.TESTS, label: 'Tests', node: returnTests },
              ]}
            />
        </>
    );
};

export default AdministrationProblem;
