import BackToTop from 'src/components/common/back-to-top/BackToTop';

import { LinkButton } from '../../components/guidelines/buttons/Button';
import Heading, { HeadingType } from '../../components/guidelines/headings/Heading';
import setLayout from '../shared/set-layout';
import withTitle from '../shared/with-title';

import image from './page-not-found.png';

import styles from './NotFoundPage.module.scss';

interface INotFoundPageProps {
    message?: string;
}

const NotFoundPage = ({ message }: INotFoundPageProps) => 
    <div className={styles.container}>
        <BackToTop />
        <img
          className={styles.image}
          src={image}
          alt="Page Not Found"
        />
        <div className={styles.paragraphAndBtnContainer}>
            <p className={styles.httpCodeParagraph}>404</p>
            <Heading type={HeadingType.secondary}>Page Not Found</Heading>
            {message && <Heading type={HeadingType.small} className={styles.message}>{message}</Heading>}
            <LinkButton
              to="/"
              text="Back to Home"
              className={styles.backBtn}
            />
        </div>
    </div>
;

export default setLayout(withTitle(NotFoundPage, 'Page Not Found'));
