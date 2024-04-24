import { useNavigate } from 'react-router-dom';

import { LOGIN_PATH } from '../../../common/urls/client-urls';
import { getContestSubmissionPageUrl } from '../../../common/urls/compose-client-urls';
import { useAppSelector } from '../../../redux/store';
import Button, { ButtonSize, ButtonState } from '../../guidelines/buttons/Button';

interface IContestButtonProps {
    isCompete: boolean;
    isDisabled: boolean;
    id: number;
    onClick?: () => void;
}

const COMPETE_STRING = 'COMPETE';
const PRACTICE_STRING = 'PRACTICE';

const ContestButton = (props: IContestButtonProps) => {
    const { isCompete, isDisabled, id, onClick } = props;

    const navigate = useNavigate();

    const { isLoggedIn } = useAppSelector((state) => state.authorization);

    const onButtonClick = async () => {
        if (onClick) {
            onClick();
            return;
        }

        if (!isLoggedIn) {
            navigate(`/${LOGIN_PATH}`);
            return;
        }

        navigate(getContestSubmissionPageUrl(isCompete, id));
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
