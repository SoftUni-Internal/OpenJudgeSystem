import { isRouteErrorResponse, useRouteError } from 'react-router-dom';

import { LinkButton } from '../../components/guidelines/buttons/Button';
import Heading, { HeadingType } from '../../components/guidelines/headings/Heading';
import NotFoundPage from '../not-found/NotFoundPage';
import image from '../not-found/page-not-found.png';
import setLayout from '../shared/set-layout';
import withTitle from '../shared/with-title';

import styles from '../not-found/NotFoundPage.module.scss';

const ErrorPage = () => {
    const error = useRouteError();
    let errorMessage: string;

    if (isRouteErrorResponse(error)) {
        if (error.status === 404) {
            return <NotFoundPage message={decodeURIComponent(error.statusText)} />;
        }
    }

    if (error instanceof Error) {
        errorMessage = error.message;
    } else if (typeof error === 'string') {
        errorMessage = error;
    } else {
        errorMessage = 'Unknown error. Please contact an administrator.';
    }

    return (
        <div className={styles.container}>
            <img
              className={styles.image}
              src={image}
              alt="Error"
            />
            <div className={styles.paragraphAndBtnContainer}>
                <Heading type={HeadingType.secondary}>Unexpected error occured</Heading>
                <Heading type={HeadingType.small} className={styles.message}>{errorMessage}</Heading>
                <LinkButton
                  to="/"
                  text="Back to Home"
                  className={styles.backBtn}
                />
            </div>
        </div>
    );
};

export default setLayout(withTitle(ErrorPage, 'Unexpected error'));
