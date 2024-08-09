import { CREATE, EDIT } from '../../../../common/labels';
import FormActionButton from '../../form-action-button/FormActionButton';

// eslint-disable-next-line css-modules/no-unused-class
import formStyles from '../styles/FormStyles.module.scss';

interface IAdministrationFormButtons{
    isEditMode: boolean;
    onEditClick?: () => void;
    onCreateClick?: () => void;
    disabled?: boolean;
}

const AdministrationFormButtons = (props: IAdministrationFormButtons) => {
    const { isEditMode, onEditClick, onCreateClick, disabled = false } = props;

    const onEdit = () => {
        if (onEditClick) {
            onEditClick();
        }
    };

    const onCreate = () => {
        if (onCreateClick) {
            onCreateClick();
        }
    };

    return (
        isEditMode
            ? (
                <FormActionButton
                  className={formStyles.buttonsWrapper}
                  buttonClassName={formStyles.button}
                  onClick={onEdit}
                  disabled={disabled}
                  name={EDIT}
                />
            )
            : (
                <FormActionButton
                  className={formStyles.buttonsWrapper}
                  buttonClassName={formStyles.button}
                  onClick={onCreate}
                  disabled={disabled}
                  name={CREATE}
                />
            )
    );
};

export default AdministrationFormButtons;
