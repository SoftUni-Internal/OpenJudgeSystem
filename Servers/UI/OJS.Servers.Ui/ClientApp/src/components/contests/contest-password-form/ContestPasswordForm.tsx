import { useState } from 'react';
import { useParams } from 'react-router';

import { ContestParticipationType } from '../../../common/constants';
import useTheme from '../../../hooks/use-theme';
import { useRegisterUserForContestMutation } from '../../../redux/services/contestsService';
import Form from '../../guidelines/forms/Form';
import FormControl, { FormControlType } from '../../guidelines/forms/FormControl';
import Heading, { HeadingType } from '../../guidelines/headings/Heading';

import styles from './ContestPasswordForm.module.scss';

interface IContestPasswordFormProps {
  contestName: string;
  onSuccess: () => void;
  hasConfirmedParticipation: boolean;
}

const ContestPasswordForm = (props: IContestPasswordFormProps) => {
    const { contestName, onSuccess, hasConfirmedParticipation } = props;

    const { themeColors, getColorClassName } = useTheme();
    const { contestId: id, participationType } = useParams();

    const [ password, setPassword ] = useState<string>('');
    const [ errorMessage, setErrorMessage ] = useState<string>('');
    const [ isLoading, setIsLoading ] = useState<boolean>(false);

    const [ registerUserForContest ] = useRegisterUserForContestMutation();

    const textColorClassName = getColorClassName(themeColors.textColor);
    const isOfficial = participationType === ContestParticipationType.Compete;

    const onPasswordSubmit = async () => {
        setIsLoading(true);
        setErrorMessage('');
        await registerUserForContest({ id: Number(id), isOfficial, password, hasConfirmedParticipation })
            .unwrap()
            .then(() => onSuccess())
            .catch((err) => setErrorMessage(err?.data?.detail ?? 'Unexpected error.'));

        setPassword('');
        setIsLoading(false);
    };

    const handlePasswordChange = (e: unknown) => {
        if (typeof e === 'string') {
            setPassword(e);
        } else if (e && typeof e === 'object' && 'target' in e) {
            // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
            setPassword((e as any).target?.value || '');
        } else {
            setPassword('');
        }
    };

    return (
        <Form
          isLoading={isLoading}
          className={`${styles.contestPasswordForm} ${textColorClassName}`}
          onSubmit={onPasswordSubmit}
          submitButtonClassName={styles.submitBtn}
        >
            <header className={styles.formHeader}>
                <Heading type={HeadingType.primary}>Enter contest password</Heading>
                <Heading type={HeadingType.secondary} className={styles.contestName}>{contestName}</Heading>
                { errorMessage && <div className={styles.errorMessage}>{errorMessage}</div>}
            </header>
            <FormControl
              name="contest-password"
              type={FormControlType.password}
              onChange={handlePasswordChange}
              value={password}
            />
        </Form>
    );
};

export default ContestPasswordForm;
