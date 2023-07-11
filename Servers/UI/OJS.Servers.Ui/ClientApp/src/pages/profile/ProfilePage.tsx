import React, { useEffect } from 'react';

import Heading from '../../components/guidelines/headings/Heading';
import ProfileAboutInfo from '../../components/profile/profile-about-info/ProfileAboutInfo';
import { useUsers } from '../../hooks/use-users';
import { makePrivate } from '../shared/make-private';
import { setLayout } from '../shared/set-layout';
// import Tabs from '../../components/guidelines/tabs/Tabs';
// import ProfileContestParticipations
//     from '../../components/profile/profile-contest-participations/ProfileContestParticipations';
// import ProfileSubmissions from '../../components/profile/profile-submissions/ProfileSubmisssions'

const ProfilePage = () => {
    const { profile, getProfile } = useUsers();

    useEffect(() => {
        (async () => {
            await getProfile();
        })();
    }, [ getProfile ]);
    return (
        <>
            <Heading>Profile</Heading>
            <ProfileAboutInfo value={profile} />
            {/* <Tabs */}
            {/*    labels={['Submissions', 'Contest Participations']} */}
            {/*    contents={[<ProfileSubmissions/>, <ProfileContestParticipations/>]} */}
            {/* /> */}
        </>
    );
};

export default makePrivate(setLayout(ProfilePage));
