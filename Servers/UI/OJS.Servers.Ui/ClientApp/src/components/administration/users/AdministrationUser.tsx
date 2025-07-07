import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { renderSuccessfullAlert } from 'src/utils/render-utils';

import { LECTURER } from '../../../common/constants';
import { IUserAdministrationModel } from '../../../common/types';
import useScrollToTab from '../../../hooks/common/use-scroll-to-tab';
import { useGetUserByIdQuery } from '../../../redux/services/admin/usersAdminService';
import SpinningLoader from '../../guidelines/spinning-loader/SpinningLoader';
import TabsInView from '../common/tabs/TabsInView';

import UserForm from './form/UserForm';
import LecturerInCategories from './lecturer/LecturerInCategories';
import LecturerInContests from './lecturer/LecturerInContests';

enum USER_LISTED_DATA {
    LECTURER_IN_CONTESTS = 'lecturerincontests',
    LECTURER_IN_CATEGORIES = 'lecturerincategories'
}

const AdministrationUser = () => {
    const { pathname, hash } = useLocation();
    const [ , , , userId ] = pathname.split('/');

    const [ user, setUser ] = useState<IUserAdministrationModel | undefined>(undefined);
    const [ successMessage, setSuccessMessage ] = useState<string | null>(null);

    const [ tabName, setTabName ] = useState(USER_LISTED_DATA.LECTURER_IN_CONTESTS);

    const {
        refetch,
        data: userData,
        isLoading: isGetting,
        isSuccess,
    } = useGetUserByIdQuery(userId);

    useScrollToTab({ hash, tabName, setTabName, tabNames: Object.values(USER_LISTED_DATA) });

    useEffect(() => {
        if (userData && isSuccess) {
            setUser(user);
        }
    }, [ isSuccess, refetch, user, userData ]);

    const onTabChange = (event: React.SyntheticEvent, newValue: USER_LISTED_DATA) => {
        setTabName(newValue);
    };

    const renderForm = () => 
        <UserForm
          id={userId}
          providedUser={user}
          onSuccess={refetch}
        />
    ;

    const renderProblemsInContestView = (key:string) => 
        userData?.roles.find((x) => x.name === LECTURER)
            ? <div id={USER_LISTED_DATA.LECTURER_IN_CONTESTS}>
                <LecturerInContests
                      key={key}
                      userId={userData.id}
                      setParentSuccessMessage={setSuccessMessage}
                    />
            </div>
            
            : <>
            </>
            
    ;

    const renderParticipantsInContestView = (key: string) => 
        userData?.roles.find((x) => x.name === LECTURER)
            ? <div id={USER_LISTED_DATA.LECTURER_IN_CATEGORIES}>
                <LecturerInCategories
                      key={key}
                      userId={userData.id}
                      setParentSuccessMessage={setSuccessMessage}
                    />
            </div>
            
            : <>
            </>
            
    ;

    if (isGetting) {
        return <SpinningLoader />;
    }

    return (
        <>
            {renderSuccessfullAlert(successMessage)}
            <TabsInView
              form={renderForm}
              onTabChange={onTabChange}
              tabName={tabName}
              tabs={userData?.roles.find((x) => x.name === LECTURER) && [
                  {
                      value: USER_LISTED_DATA.LECTURER_IN_CONTESTS,
                      label: 'Lecturer in Contests',
                      node: renderProblemsInContestView,
                  },
                  {
                      value: USER_LISTED_DATA.LECTURER_IN_CATEGORIES,
                      label: 'Lecturer in Categories',
                      node: renderParticipantsInContestView,
                  },
              ]}
            />
        </>
    );
};

export default AdministrationUser;
