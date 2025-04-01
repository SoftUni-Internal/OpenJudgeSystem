import { useNavigate } from 'react-router';
import { IHaveOptionalClassName } from 'src/components/common/Props';
import useTheme from 'src/hooks/use-theme';
import concatClassNames from 'src/utils/class-names';

import { getContestsSolutionSubmitPageUrl } from '../../../common/urls/compose-client-urls';
import { setSelectedContestDetailsProblem } from '../../../redux/features/contestsSlice';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import Button, { ButtonSize, ButtonState } from '../../guidelines/buttons/Button';

import styles from './ContestButton.module.scss';

    interface IContestButtonProps extends IHaveOptionalClassName {
        isCompete: boolean;
        isDisabled: boolean;
        id: number;
        problemId?: number;
        orderBy?: number;
        onClick?: () => void;
        name: string;
    }

const COMPETE_STRING = 'COMPETE';
const PRACTICE_STRING = 'PRACTICE';

const ContestButton = (props: IContestButtonProps) => {
    const {
        isCompete,
        isDisabled,
        id,
        problemId,
        orderBy,
        onClick,
        name,
        className,
    } = props;

    const { internalUser } = useAppSelector((reduxState) => reduxState.authorization);
    const dispatch = useAppDispatch();

    const navigate = useNavigate();
    const { isDarkMode } = useTheme();

    const onButtonClick = async () => {
        dispatch(setSelectedContestDetailsProblem({ selectedProblem: null }));
        if (onClick) {
            onClick();
            return;
        }

        navigate(getContestsSolutionSubmitPageUrl({
            isCompete,
            contestId: id,
            contestName: name,
            problemId,
            orderBy,
        }));
    };

    const isUserAdminOrLecturer = internalUser.isAdmin || internalUser.isLecturer;

    const btnText = isCompete
        ? COMPETE_STRING
        : PRACTICE_STRING;

    return (
        <Button
          text={btnText}
          state={!isUserAdminOrLecturer && isDisabled
              ? ButtonState.disabled
              : ButtonState.enabled}
          size={ButtonSize.small}
          isCompete={isCompete}
          onClick={onButtonClick}
          className={concatClassNames(
              className,
              isUserAdminOrLecturer && isDisabled
                  ? isDarkMode
                      ? styles.adminDisabledDark
                      : styles.adminDisabledLight
                  : '',
          )}
        />
    );
};

export default ContestButton;
