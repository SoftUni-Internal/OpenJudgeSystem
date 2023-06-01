import React, { useEffect, useState } from 'react';

import { LinkButton, LinkButtonType } from '../../components/guidelines/buttons/Button';
import List, { Orientation } from '../../components/guidelines/lists/List';
import { useAuth } from '../../hooks/use-auth';

import styles from './PageNav.module.scss';

interface IRouteType {
    name: string;
    link: string;
    isPrimary: boolean;
    id: string;
}

const userRoutes = [
    { id: 'nav-my-profile-link', name: 'My Profile', link: '/profile', isPrimary: true },
    { id: 'nav-logout-link', name: 'Log out', link: '/logout', isPrimary: false },
];

const anonymousRoutes = [
    { name: 'Login', link: '/login', isPrimary: false, id: 'anonymous-login-link' },
    { name: 'Register', link: '/register', isPrimary: true, id: 'anonymous-register-link' },
];

const PageNav = () => {
    const { state: { user } } = useAuth();
    const [ routes, setRoutes ] = useState(anonymousRoutes);

    useEffect(() => {
        setRoutes(user.isLoggedIn
            ? userRoutes
            : anonymousRoutes);
    }, [ user.isLoggedIn ]);

    const itemFunc = ({ name, link, isPrimary, id }: IRouteType) => {
        const type = isPrimary
            ? LinkButtonType.primary
            : LinkButtonType.secondary;

        return (
            <LinkButton to={link} id={id} text={name} type={type} className={styles.btn} />
        );
    };

    return (
        <nav className={styles.navigation}>
            <List
              values={routes}
              itemFunc={itemFunc}
              orientation={Orientation.horizontal}
              className={styles.listNav}
            />
        </nav>
    );
};

export default PageNav;
