import { useNavigate } from 'react-router-dom';

import { getContestSubmissionPageUrl } from '../../../common/urls/compose-client-urls';
import { setSelectedContestDetailsProblem } from '../../../redux/features/contestsSlice';
import { useAppDispatch } from '../../../redux/store';
import Button, { ButtonSize, ButtonState } from '../../guidelines/buttons/Button';

interface IContestButtonProps {
    isCompete: boolean;
    isDisabled: boolean;
    id: number;
    problemId?: number;
    onClick?: () => void;
    skipRedirectHistory?: boolean;
}

const COMPETE_STRING = 'COMPETE';
const PRACTICE_STRING = 'PRACTICE';

const ContestButton = (props: IContestButtonProps) => {
    const {
        isCompete,
        isDisabled,
        id,
        problemId,
        onClick,
        skipRedirectHistory,
    } = props;
    const dispatch = useAppDispatch();

    const navigate = useNavigate();

    const onButtonClick = async () => {
        dispatch(setSelectedContestDetailsProblem({ selectedProblem: null }));
        if (onClick) {
            onClick();
            return;
        }

        navigate(getContestSubmissionPageUrl(isCompete, id, problemId), { replace: skipRedirectHistory });
    };

    const btnText = isCompete
        ? COMPETE_STRING
        : PRACTICE_STRING;

    return (
        <Button
          text={btnText}
          state={isDisabled
              ? ButtonState.disabled
              : ButtonState.enabled}
          size={ButtonSize.small}
          isCompete={isCompete}
          onClick={onButtonClick}
        />
    );
};

export default ContestButton;
